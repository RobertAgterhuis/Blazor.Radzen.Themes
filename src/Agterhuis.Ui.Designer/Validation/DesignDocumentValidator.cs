using System.Text.Json.Nodes;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Registry;

namespace Agterhuis.Ui.Designer.Validation;

public static class DesignDocumentValidator
{
    public static IReadOnlyList<DesignValidationError> Validate(DesignDocument document, DesignerComponentRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        registry ??= DesignerComponentRegistry.Instance;
        var errors = new List<DesignValidationError>();
        var routeToIndexes = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

        for (var pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
        {
            var page = document.Pages[pageIndex];
            var pagePath = $"Pages[{pageIndex}]";
            var normalizedRoute = (page.Route ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalizedRoute))
            {
                errors.Add(new DesignValidationError(pagePath, "MissingRoute", "Page route is required."));
            }
            else
            {
                if (!routeToIndexes.TryGetValue(normalizedRoute, out var indexes))
                {
                    indexes = [];
                    routeToIndexes[normalizedRoute] = indexes;
                }

                indexes.Add(pageIndex);
            }

            if (string.IsNullOrWhiteSpace(page.Title))
            {
                errors.Add(new DesignValidationError(pagePath, "MissingTitle", "Page title is required."));
            }

            for (var nodeIndex = 0; nodeIndex < page.Nodes.Count; nodeIndex++)
            {
                ValidateNode(page.Nodes[nodeIndex], $"{pagePath}/Nodes[{nodeIndex}]", registry, errors);
            }
        }

        foreach (var duplicate in routeToIndexes.Where(static pair => pair.Value.Count > 1))
        {
            foreach (var pageIndex in duplicate.Value)
            {
                errors.Add(new DesignValidationError($"Pages[{pageIndex}]/Route", "DuplicateRoute", $"Route '{duplicate.Key}' is used by multiple pages."));
            }
        }

        return errors;
    }

    private static void ValidateNode(
        DesignNode node,
        string path,
        DesignerComponentRegistry registry,
        ICollection<DesignValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(node.ComponentType))
        {
            errors.Add(new DesignValidationError(path, "MissingComponentType", "Component type is required."));
            return;
        }

        if (!registry.TryGetDescriptor(node.ComponentType, out var descriptor))
        {
            errors.Add(new DesignValidationError(path, "UnknownComponentType", $"Unknown component type '{node.ComponentType}'."));
            return;
        }

        foreach (var parameter in node.Parameters)
        {
            if (!descriptor.Parameters.Any(candidate => string.Equals(candidate.Name, parameter.Key, StringComparison.Ordinal)))
            {
                errors.Add(new DesignValidationError($"{path}/Parameters/{parameter.Key}", "UnknownParameter", $"Unknown parameter '{parameter.Key}' on '{node.ComponentType}'."));
            }
        }

        foreach (var requiredParameter in descriptor.Parameters.Where(static parameter => parameter.IsEditorRequired))
        {
            if (!node.Parameters.TryGetValue(requiredParameter.Name, out var value) || !HasMeaningfulValue(value))
            {
                errors.Add(new DesignValidationError($"{path}/Parameters/{requiredParameter.Name}", "MissingRequiredParameter", $"Parameter '{requiredParameter.Name}' is required on '{node.ComponentType}'."));
            }
        }

        if (descriptor.IsWrapper &&
            descriptor.Parameters.Any(static parameter => parameter.Name == "Label") &&
            descriptor.Parameters.Any(static parameter => parameter.Name == "AriaLabel") &&
            !HasMeaningfulValue(node.Parameters.GetValueOrDefault("Label")) &&
            !HasMeaningfulValue(node.Parameters.GetValueOrDefault("AriaLabel")))
        {
            errors.Add(new DesignValidationError(path, "MissingAccessibleLabel", $"'{node.ComponentType}' requires Label or AriaLabel."));
        }

        foreach (var slot in node.Children)
        {
            if (!descriptor.Slots.Contains(slot.Key, StringComparer.Ordinal))
            {
                errors.Add(new DesignValidationError($"{path}/Children/{slot.Key}", "UnknownSlot", $"Unknown slot '{slot.Key}' on '{node.ComponentType}'."));
            }

            var children = slot.Value ?? [];
            for (var childIndex = 0; childIndex < children.Count; childIndex++)
            {
                ValidateNode(children[childIndex], $"{path}/Children[{slot.Key}][{childIndex}]", registry, errors);
            }
        }
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
}