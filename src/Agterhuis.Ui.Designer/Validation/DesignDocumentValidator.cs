using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Registry;

namespace Agterhuis.Ui.Designer.Validation;

public static partial class DesignDocumentValidator
{
    private static readonly Regex RouteRegex = RoutePattern();
    private static readonly Regex HexColorRegex = HexColorPattern();
    private static readonly Regex TokenReferenceRegex = TokenReferencePattern();

    private static readonly HashSet<string> AllowedThemeTokens = new(
    [
        "--agt-color-primary-500",
        "--agt-color-accent-400",
        "--agt-text-body",
        "--agt-text-heading",
        "--agt-surface-0",
        "--agt-surface-1",
        "--agt-input-border",
        "--agt-color-danger-500",
        "--agt-on-accent",
        "--agt-alpha-primary-15"
    ],
    StringComparer.Ordinal);

    private static readonly HashSet<string> RequiredChildContentComponents = new(StringComparer.Ordinal)
    {
        "AgtCard",
        "RadzenCard",
        "RadzenButton",
        "AgtButton"
    };

    private static readonly HashSet<string> LayoutOnlyComponents = new(StringComparer.Ordinal)
    {
        "RadzenDataGrid",
        "RadzenDataGridColumn",
        "RadzenChart"
    };

    public static IReadOnlyList<DesignValidationError> Validate(DesignDocument document, DesignerComponentRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        registry ??= DesignerComponentRegistry.Instance;
        var errors = new List<DesignValidationError>();

        if (document.Pages.Count == 0)
        {
            errors.Add(new DesignValidationError(
                "Pages",
                "EmptyDocument",
                "Document bevat geen pagina's.",
                DesignValidationSeverity.Error));

            return errors;
        }

        var routeToIndexes = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        var nodeIdMap = new Dictionary<string, List<(int PageIndex, string Path)>>(StringComparer.Ordinal);
        var entities = BuildEntityMap(document.DataModel);

        for (var pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
        {
            var page = document.Pages[pageIndex];
            var pagePath = $"Pages[{pageIndex}]";
            var normalizedRoute = (page.Route ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalizedRoute))
            {
                errors.Add(new DesignValidationError(
                    $"{pagePath}/Route",
                    "MissingRoute",
                    "Page route is verplicht.",
                    DesignValidationSeverity.Error,
                    pageIndex));
            }
            else
            {
                if (!normalizedRoute.StartsWith("/", StringComparison.Ordinal) || !RouteRegex.IsMatch(normalizedRoute))
                {
                    errors.Add(new DesignValidationError(
                        $"{pagePath}/Route",
                        "InvalidRoute",
                        $"Route '{normalizedRoute}' is ongeldig.",
                        DesignValidationSeverity.Error,
                        pageIndex));
                }

                if (!routeToIndexes.TryGetValue(normalizedRoute, out var indexes))
                {
                    indexes = [];
                    routeToIndexes[normalizedRoute] = indexes;
                }

                indexes.Add(pageIndex);
            }

            if (string.IsNullOrWhiteSpace(page.Title))
            {
                errors.Add(new DesignValidationError(
                    $"{pagePath}/Title",
                    "MissingTitle",
                    "Page title is verplicht.",
                    DesignValidationSeverity.Error,
                    pageIndex));
            }

            if (page.Nodes.Count == 0)
            {
                errors.Add(new DesignValidationError(
                    $"{pagePath}/Nodes",
                    "EmptyPage",
                    "Pagina bevat geen componenten.",
                    DesignValidationSeverity.Warning,
                    pageIndex));
            }

            for (var nodeIndex = 0; nodeIndex < page.Nodes.Count; nodeIndex++)
            {
                ValidateNode(
                    page.Nodes[nodeIndex],
                    $"{pagePath}/Nodes[{nodeIndex}]",
                    registry,
                    errors,
                    pageIndex,
                    entities,
                    nodeIdMap,
                    currentEntityName: null,
                    parentComponentType: null,
                    parentSlotName: null);
            }
        }

        foreach (var duplicate in routeToIndexes.Where(static pair => pair.Value.Count > 1))
        {
            foreach (var pageIndex in duplicate.Value)
            {
                errors.Add(new DesignValidationError(
                    $"Pages[{pageIndex}]/Route",
                    "DuplicateRoute",
                    $"Route '{duplicate.Key}' wordt meerdere keren gebruikt.",
                    DesignValidationSeverity.Error,
                    pageIndex));
            }
        }

        foreach (var duplicate in nodeIdMap.Where(static pair => pair.Value.Count > 1))
        {
            foreach (var entry in duplicate.Value)
            {
                errors.Add(new DesignValidationError(
                    entry.Path,
                    "DuplicateNodeId",
                    $"Node-id '{duplicate.Key}' komt meerdere keren voor.",
                    DesignValidationSeverity.Error,
                    entry.PageIndex,
                    duplicate.Key));
            }
        }

        return errors;
    }

    private static Dictionary<string, HashSet<string>> BuildEntityMap(DesignDataModel model)
    {
        var entities = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var entity in model.Entities)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                continue;
            }

            entities[entity.Name] = entity.Fields
                .Where(static field => !string.IsNullOrWhiteSpace(field.Name))
                .Select(static field => field.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return entities;
    }

    private static void ValidateNode(
        DesignNode node,
        string path,
        DesignerComponentRegistry registry,
        ICollection<DesignValidationError> errors,
        int pageIndex,
        IReadOnlyDictionary<string, HashSet<string>> entities,
        IDictionary<string, List<(int PageIndex, string Path)>> nodeIdMap,
        string? currentEntityName,
        string? parentComponentType,
        string? parentSlotName)
    {
        if (!string.IsNullOrWhiteSpace(node.Id))
        {
            if (!nodeIdMap.TryGetValue(node.Id, out var entries))
            {
                entries = [];
                nodeIdMap[node.Id] = entries;
            }

            entries.Add((pageIndex, path));
        }

        if (string.IsNullOrWhiteSpace(node.ComponentType))
        {
            errors.Add(new DesignValidationError(
                path,
                "MissingComponentType",
                "Component type is verplicht.",
                DesignValidationSeverity.Error,
                pageIndex,
                node.Id));

            return;
        }

        if (!registry.TryGetDescriptor(node.ComponentType, out var descriptor))
        {
            errors.Add(new DesignValidationError(
                path,
                "UnknownComponentType",
                $"Onbekend component type '{node.ComponentType}'.",
                DesignValidationSeverity.Error,
                pageIndex,
                node.Id));

            return;
        }

        if (descriptor.IsDeprecated)
        {
            errors.Add(new DesignValidationError(
                path,
                "DeprecatedComponent",
                $"Component '{node.ComponentType}' is verouderd.",
                DesignValidationSeverity.Warning,
                pageIndex,
                node.Id));
        }

        if (!string.IsNullOrWhiteSpace(parentSlotName)
            && string.Equals(parentSlotName, "Columns", StringComparison.Ordinal)
            && !string.Equals(node.ComponentType, "RadzenDataGridColumn", StringComparison.Ordinal))
        {
            errors.Add(new DesignValidationError(
                path,
                "IncompatibleNesting",
                $"Component '{node.ComponentType}' is niet toegestaan in slot '{parentSlotName}'.",
                DesignValidationSeverity.Warning,
                pageIndex,
                node.Id));
        }

        if (!string.IsNullOrWhiteSpace(parentSlotName)
            && string.Equals(parentSlotName, "ChildContent", StringComparison.Ordinal)
            && string.Equals(parentComponentType, "RadzenButton", StringComparison.Ordinal)
            && LayoutOnlyComponents.Contains(node.ComponentType))
        {
            errors.Add(new DesignValidationError(
                path,
                "IncompatibleNesting",
                $"Component '{node.ComponentType}' hoort niet in de '{parentComponentType}'-content.",
                DesignValidationSeverity.Warning,
                pageIndex,
                node.Id));
        }

        foreach (var parameter in node.Parameters)
        {
            if (!descriptor.Parameters.Any(candidate => string.Equals(candidate.Name, parameter.Key, StringComparison.Ordinal)))
            {
                errors.Add(new DesignValidationError(
                    $"{path}/Parameters/{parameter.Key}",
                    "UnknownParameter",
                    $"Onbekende parameter '{parameter.Key}' op '{node.ComponentType}'.",
                    DesignValidationSeverity.Error,
                    pageIndex,
                    node.Id,
                    parameter.Key));
            }
        }

        foreach (var requiredParameter in descriptor.Parameters.Where(static parameter => parameter.IsEditorRequired))
        {
            if (!node.Parameters.TryGetValue(requiredParameter.Name, out var value) || !HasMeaningfulValue(value))
            {
                errors.Add(new DesignValidationError(
                    $"{path}/Parameters/{requiredParameter.Name}",
                    "MissingRequiredParameter",
                    $"Parameter '{requiredParameter.Name}' is verplicht op '{node.ComponentType}'.",
                    DesignValidationSeverity.Error,
                    pageIndex,
                    node.Id,
                    requiredParameter.Name));
            }
        }

        ValidateAccessibility(node, descriptor, path, errors, pageIndex);
        var resolvedEntityName = ValidateDataBinding(node, descriptor, path, errors, pageIndex, entities, currentEntityName);
        ValidateTokenPolicy(node, descriptor, path, errors, pageIndex);

        foreach (var slot in node.Children)
        {
            if (!descriptor.Slots.Contains(slot.Key, StringComparer.Ordinal))
            {
                errors.Add(new DesignValidationError(
                    $"{path}/Children/{slot.Key}",
                    "UnknownSlot",
                    $"Onbekend slot '{slot.Key}' op '{node.ComponentType}'.",
                    DesignValidationSeverity.Error,
                    pageIndex,
                    node.Id));
            }

            var children = slot.Value ?? [];
            if (children.Count == 0
                && string.Equals(slot.Key, "ChildContent", StringComparison.Ordinal)
                && RequiredChildContentComponents.Contains(node.ComponentType))
            {
                errors.Add(new DesignValidationError(
                    $"{path}/Children/{slot.Key}",
                    "RequiredSlotEmpty",
                    $"Slot '{slot.Key}' van '{node.ComponentType}' is leeg.",
                    DesignValidationSeverity.Warning,
                    pageIndex,
                    node.Id));
            }

            for (var childIndex = 0; childIndex < children.Count; childIndex++)
            {
                ValidateNode(
                    children[childIndex],
                    $"{path}/Children[{slot.Key}][{childIndex}]",
                    registry,
                    errors,
                    pageIndex,
                    entities,
                    nodeIdMap,
                    resolvedEntityName,
                    descriptor.ComponentType,
                    slot.Key);
            }
        }
    }

    private static void ValidateAccessibility(
        DesignNode node,
        DesignerComponentDescriptor descriptor,
        string path,
        ICollection<DesignValidationError> errors,
        int pageIndex)
    {
        if (descriptor.IsWrapper
            && descriptor.Parameters.Any(static parameter => parameter.Name == "Label")
            && descriptor.Parameters.Any(static parameter => parameter.Name == "AriaLabel")
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("Label"))
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("AriaLabel")))
        {
            errors.Add(new DesignValidationError(
                path,
                "MissingFormLabel",
                $"'{node.ComponentType}' vereist Label of AriaLabel.",
                DesignValidationSeverity.Error,
                pageIndex,
                node.Id));
        }

        if ((string.Equals(node.ComponentType, "AgtButton", StringComparison.Ordinal)
            || string.Equals(node.ComponentType, "RadzenButton", StringComparison.Ordinal))
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("Text"))
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("AriaLabel")))
        {
            errors.Add(new DesignValidationError(
                path,
                "EmptyButtonText",
                $"'{node.ComponentType}' heeft geen Text of AriaLabel.",
                DesignValidationSeverity.Warning,
                pageIndex,
                node.Id,
                "Text"));
        }

        if ((string.Equals(node.ComponentType, "AgtImage", StringComparison.Ordinal)
            || string.Equals(node.ComponentType, "RadzenImage", StringComparison.Ordinal))
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("Alt"))
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("AlternateText"))
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("AriaLabel")))
        {
            errors.Add(new DesignValidationError(
                path,
                "ImageWithoutAlt",
                $"'{node.ComponentType}' mist alt-tekst.",
                DesignValidationSeverity.Warning,
                pageIndex,
                node.Id,
                "Alt"));
        }
    }

    private static string? ValidateDataBinding(
        DesignNode node,
        DesignerComponentDescriptor descriptor,
        string path,
        ICollection<DesignValidationError> errors,
        int pageIndex,
        IReadOnlyDictionary<string, HashSet<string>> entities,
        string? inheritedEntityName)
    {
        var entityName = GetEntityReference(node) ?? inheritedEntityName;
        if (entityName is not null && !entities.ContainsKey(entityName))
        {
            errors.Add(new DesignValidationError(
                $"{path}/Parameters/Data",
                "BrokenEntityReference",
                $"Entiteit '{entityName}' bestaat niet in het datamodel.",
                DesignValidationSeverity.Error,
                pageIndex,
                node.Id,
                "Data"));

            return inheritedEntityName;
        }

        if (string.Equals(node.ComponentType, "RadzenDataGrid", StringComparison.Ordinal)
            && !HasMeaningfulValue(node.Parameters.GetValueOrDefault("Data")))
        {
            errors.Add(new DesignValidationError(
                $"{path}/Parameters/Data",
                "UnboundDataGrid",
                "DataGrid heeft geen databinding.",
                DesignValidationSeverity.Info,
                pageIndex,
                node.Id,
                "Data"));
        }

        if (string.Equals(node.ComponentType, "RadzenDataGridColumn", StringComparison.Ordinal)
            && HasMeaningfulValue(node.Parameters.GetValueOrDefault("Property")))
        {
            var fieldName = GetStringLiteral(node.Parameters.GetValueOrDefault("Property"));
            if (!string.IsNullOrWhiteSpace(fieldName) && entityName is not null && entities.TryGetValue(entityName, out var fields) && !fields.Contains(fieldName))
            {
                errors.Add(new DesignValidationError(
                    $"{path}/Parameters/Property",
                    "BrokenFieldReference",
                    $"Veld '{fieldName}' bestaat niet op entiteit '{entityName}'.",
                    DesignValidationSeverity.Error,
                    pageIndex,
                    node.Id,
                    "Property"));
            }
        }

        if (string.Equals(node.ComponentType, "RadzenDataGridColumn", StringComparison.Ordinal)
            && string.IsNullOrWhiteSpace(entityName)
            && HasMeaningfulValue(node.Parameters.GetValueOrDefault("Property")))
        {
            errors.Add(new DesignValidationError(
                $"{path}/Parameters/Property",
                "BrokenEntityReference",
                "DataGrid-kolom verwijst naar een veld maar de bijbehorende entiteit kon niet worden bepaald.",
                DesignValidationSeverity.Error,
                pageIndex,
                node.Id,
                "Property"));
        }

        if (descriptor.Parameters.Any(static parameter => string.Equals(parameter.Name, "ValueProperty", StringComparison.Ordinal))
            && HasMeaningfulValue(node.Parameters.GetValueOrDefault("ValueProperty"))
            && entityName is not null
            && entities.TryGetValue(entityName, out var knownFields))
        {
            var valueProperty = GetStringLiteral(node.Parameters.GetValueOrDefault("ValueProperty"));
            if (!string.IsNullOrWhiteSpace(valueProperty) && !knownFields.Contains(valueProperty))
            {
                errors.Add(new DesignValidationError(
                    $"{path}/Parameters/ValueProperty",
                    "BrokenFieldReference",
                    $"Veld '{valueProperty}' bestaat niet op entiteit '{entityName}'.",
                    DesignValidationSeverity.Error,
                    pageIndex,
                    node.Id,
                    "ValueProperty"));
            }
        }

        return entityName;
    }

    private static void ValidateTokenPolicy(
        DesignNode node,
        DesignerComponentDescriptor descriptor,
        string path,
        ICollection<DesignValidationError> errors,
        int pageIndex)
    {
        foreach (var parameter in descriptor.Parameters)
        {
            if (parameter.ParameterType != typeof(string))
            {
                continue;
            }

            if (!node.Parameters.TryGetValue(parameter.Name, out var value))
            {
                continue;
            }

            var textValue = GetStringLiteral(value);
            if (string.IsNullOrWhiteSpace(textValue))
            {
                continue;
            }

            if (HexColorRegex.IsMatch(textValue))
            {
                errors.Add(new DesignValidationError(
                    $"{path}/Parameters/{parameter.Name}",
                    "HardcodedColor",
                    $"Parameter '{parameter.Name}' op '{node.ComponentType}' gebruikt een hardcoded kleur.",
                    DesignValidationSeverity.Warning,
                    pageIndex,
                    node.Id,
                    parameter.Name));
            }

            var tokenMatch = TokenReferenceRegex.Match(textValue);
            if (tokenMatch.Success)
            {
                var token = tokenMatch.Groups["token"].Value;
                if (!AllowedThemeTokens.Contains(token))
                {
                    errors.Add(new DesignValidationError(
                        $"{path}/Parameters/{parameter.Name}",
                        "InvalidTokenReference",
                        $"Token '{token}' bestaat niet in het actieve thema.",
                        DesignValidationSeverity.Warning,
                        pageIndex,
                        node.Id,
                        parameter.Name));
                }
            }
        }
    }

    private static string? GetEntityReference(DesignNode node)
    {
        if (!node.Parameters.TryGetValue("Data", out var dataParameter))
        {
            return null;
        }

        var value = GetStringLiteral(dataParameter);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var dataValue = value.Trim();
        if (dataValue.EndsWith("Records", StringComparison.OrdinalIgnoreCase))
        {
            dataValue = dataValue[..^"Records".Length];
        }

        if (dataValue.EndsWith("()", StringComparison.OrdinalIgnoreCase))
        {
            dataValue = dataValue[..^2];
        }

        return dataValue.Trim();
    }

    private static string? GetStringLiteral(DesignParameterValue? value)
    {
        if (value is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(value.Expression))
        {
            return value.Expression.Trim();
        }

        if (value.Literal is JsonValue literal && literal.TryGetValue<string>(out var text))
        {
            return text?.Trim();
        }

        return value.Literal?.ToJsonString().Trim('"', ' ');
    }

    private static bool HasMeaningfulValue(DesignParameterValue? value)
    {
        if (value is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(value.Expression))
        {
            return true;
        }

        return value.Literal switch
        {
            null => false,
            JsonValue jsonValue when jsonValue.TryGetValue<string>(out var stringValue) => !string.IsNullOrWhiteSpace(stringValue),
            _ => true
        };
    }

    [GeneratedRegex("^/[a-zA-Z0-9/_-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex RoutePattern();

    [GeneratedRegex("#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\\b", RegexOptions.CultureInvariant)]
    private static partial Regex HexColorPattern();

    [GeneratedRegex("var\\((?<token>--agt-[a-z0-9-]+)\\)", RegexOptions.CultureInvariant)]
    private static partial Regex TokenReferencePattern();
}
