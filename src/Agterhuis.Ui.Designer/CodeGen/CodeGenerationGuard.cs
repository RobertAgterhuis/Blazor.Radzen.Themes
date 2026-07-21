using System.Text.RegularExpressions;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Registry;

namespace Agterhuis.Ui.Designer.CodeGen;

internal static partial class CodeGenerationGuard
{
    public static IReadOnlyList<DesignValidationError> ValidatePageForCodeGen(DesignPage page, DesignerComponentRegistry registry, int pageIndex)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        return ValidatePageForCodeGen(page, registry, $"Pages[{pageIndex}]");
    }

    public static IReadOnlyList<DesignValidationError> ValidatePageForCodeGen(DesignPage page, DesignerComponentRegistry registry)
        => ValidatePageForCodeGen(page, registry, "Page");

    private static IReadOnlyList<DesignValidationError> ValidatePageForCodeGen(DesignPage page, DesignerComponentRegistry registry, string pagePath)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(pagePath);

        var errors = new List<DesignValidationError>();

        for (var nodeIndex = 0; nodeIndex < page.Nodes.Count; nodeIndex++)
        {
            ValidateNode(page.Nodes[nodeIndex], $"{pagePath}/Nodes[{nodeIndex}]", registry, errors);
        }

        return errors;
    }

    private static void ValidateNode(DesignNode node, string path, DesignerComponentRegistry registry, ICollection<DesignValidationError> errors)
    {
        if (!registry.TryGetDescriptor(node.ComponentType, out var descriptor))
        {
            return;
        }

        foreach (var parameter in descriptor.Parameters)
        {
            if (!node.Parameters.TryGetValue(parameter.Name, out var value) || value.Literal is null)
            {
                continue;
            }

            if (parameter.ParameterType == typeof(string))
            {
                var text = value.Literal.GetValue<string?>();
                if (!string.IsNullOrWhiteSpace(text) && HexColorOccurrencePattern().IsMatch(text))
                {
                    errors.Add(new DesignValidationError($"{path}/Parameters/{parameter.Name}", "HardcodedColor", $"Parameter '{parameter.Name}' on '{node.ComponentType}' uses a hardcoded hex color."));
                }
            }
        }

        if (descriptor.IsWrapper
            && descriptor.Parameters.Any(static parameter => parameter.Name == "Label")
            && descriptor.Parameters.Any(static parameter => parameter.Name == "AriaLabel")
            && !HasMeaningfulText(node.Parameters.GetValueOrDefault("Label"))
            && !HasMeaningfulText(node.Parameters.GetValueOrDefault("AriaLabel")))
        {
            errors.Add(new DesignValidationError(path, "MissingAccessibleLabel", $"'{node.ComponentType}' requires Label or AriaLabel before code generation."));
        }

        foreach (var slot in node.Children.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            for (var childIndex = 0; childIndex < slot.Value.Count; childIndex++)
            {
                ValidateNode(slot.Value[childIndex], $"{path}/Children[{slot.Key}][{childIndex}]", registry, errors);
            }
        }
    }

    private static bool HasMeaningfulText(DesignParameterValue? value)
        => value?.Literal?.GetValue<string?>() is { } text && !string.IsNullOrWhiteSpace(text);

    [GeneratedRegex("#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\\b", RegexOptions.CultureInvariant)]
    private static partial Regex HexColorOccurrencePattern();
}