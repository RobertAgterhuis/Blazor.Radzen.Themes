using System.Reflection;
using System.Text.RegularExpressions;
using Agterhuis.Ui.Components.Layout;
using Agterhuis.Ui.Designer.Introspection;
using Agterhuis.Ui.Designer.Registry;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerComponentRegistryTests
{
    private static readonly HashSet<string> PaletteRawComponents = new(StringComparer.Ordinal)
    {
        "RadzenRow",
        "RadzenColumn",
        "RadzenStack",
        "RadzenCard",
        "RadzenTabs",
        "RadzenAccordion",
        "RadzenDataGrid"
    };

    [Fact]
    public void GeneratedRegistry_MatchesReflectionMetadata_ForAllComponents()
    {
        var generated = DesignerComponentRegistry.Instance;
        var reflected = BuildReflectionRegistry();

        Assert.Equal(reflected.Components.Count, generated.Components.Count);
        Assert.Equal(reflected.CountsByCategory, generated.CountsByCategory);

        var expectedByType = reflected.Components
            .OrderBy(static descriptor => descriptor.ComponentType, StringComparer.Ordinal)
            .ToDictionary(static descriptor => descriptor.ComponentType, StringComparer.Ordinal);

        var actualByType = generated.Components
            .OrderBy(static descriptor => descriptor.ComponentType, StringComparer.Ordinal)
            .ToDictionary(static descriptor => descriptor.ComponentType, StringComparer.Ordinal);

        Assert.Equal(expectedByType.Keys, actualByType.Keys);

        foreach (var componentType in expectedByType.Keys)
        {
            AssertDescriptorEquivalent(expectedByType[componentType], actualByType[componentType]);
        }
    }

    private static void AssertDescriptorEquivalent(DesignerComponentDescriptor expected, DesignerComponentDescriptor actual)
    {
        Assert.Equal(expected.ComponentType, actual.ComponentType);
        Assert.Equal(expected.ClrType, actual.ClrType);
        Assert.Equal(expected.DisplayName, actual.DisplayName);
        Assert.Equal(expected.Category, actual.Category);
        Assert.Equal(expected.Icon, actual.Icon);
        Assert.Equal(expected.AllowedInPalette, actual.AllowedInPalette);
        Assert.Equal(expected.IsWrapper, actual.IsWrapper);
        Assert.Equal(expected.Slots, actual.Slots);

        var expectedParameters = expected.Parameters
            .Select(static parameter => (
                parameter.Name,
                parameter.TypeName,
                parameter.ParameterType,
                parameter.DefaultValue,
                parameter.Description,
                parameter.IsEditorRequired,
                parameter.IsEventCallback,
                parameter.IsRenderFragment,
                parameter.IsTemplatedRenderFragment))
            .ToArray();

        var actualParameters = actual.Parameters
            .Select(static parameter => (
                parameter.Name,
                parameter.TypeName,
                parameter.ParameterType,
                parameter.DefaultValue,
                parameter.Description,
                parameter.IsEditorRequired,
                parameter.IsEventCallback,
                parameter.IsRenderFragment,
                parameter.IsTemplatedRenderFragment))
            .ToArray();

        Assert.Equal(expectedParameters, actualParameters);
    }

    private static DesignerComponentRegistry BuildReflectionRegistry()
    {
        var categories = LoadInventoryCategories();
        var descriptors = new List<DesignerComponentDescriptor>();

        descriptors.AddRange(DiscoverComponents(typeof(AgtCard).Assembly, isWrapper: true, categories));
        descriptors.AddRange(DiscoverComponents(typeof(RadzenComponent).Assembly, isWrapper: false, categories));

        return new DesignerComponentRegistry(descriptors);
    }

    private static IEnumerable<DesignerComponentDescriptor> DiscoverComponents(
        Assembly assembly,
        bool isWrapper,
        IReadOnlyDictionary<string, string> categories)
    {
        var componentBaseType = typeof(IComponent);

        foreach (var type in assembly.GetExportedTypes()
                     .Where(type => !type.IsAbstract && componentBaseType.IsAssignableFrom(type)))
        {
            var key = GetComponentKey(type);
            if (isWrapper && !key.StartsWith("Agt", StringComparison.Ordinal))
            {
                continue;
            }

            if (!isWrapper && !key.StartsWith("Radzen", StringComparison.Ordinal))
            {
                continue;
            }

            var renderableType = CloseRenderableType(type);
            var parameters = ComponentParameterIntrospector.Describe(renderableType);
            var descriptorParameters = parameters
                .Select(parameter => new Agterhuis.Ui.Designer.Registry.ComponentParameterDescriptor(
                    parameter.Name,
                    parameter.TypeName,
                    parameter.ParameterType,
                    parameter.DefaultValue,
                    parameter.Description,
                    parameter.IsEditorRequired,
                    parameter.IsEventCallback,
                    parameter.IsRenderFragment,
                    parameter.IsTemplatedRenderFragment))
                .ToArray();

            yield return new DesignerComponentDescriptor(
                key,
                renderableType,
                ToDisplayName(key),
                ResolveCategory(type, key, isWrapper, categories),
                ResolveIcon(key, isWrapper),
                isWrapper || PaletteRawComponents.Contains(key),
                isWrapper,
                descriptorParameters.Where(static parameter => parameter.IsRenderFragment)
                    .Select(static parameter => parameter.Name)
                    .OrderBy(static slot => slot, StringComparer.Ordinal)
                    .ToArray(),
                descriptorParameters);
        }
    }

    private static Type CloseRenderableType(Type type)
    {
        if (!type.ContainsGenericParameters)
        {
            return type;
        }

        var genericArguments = Enumerable.Repeat(typeof(object), type.GetGenericArguments().Length).ToArray();
        return type.MakeGenericType(genericArguments);
    }

    private static string GetComponentKey(Type type)
        => type.Name.Split('`')[0];

    private static string ToDisplayName(string componentType)
    {
        var strippedPrefix = componentType.StartsWith("Agt", StringComparison.Ordinal)
            ? componentType[3..]
            : componentType.StartsWith("Radzen", StringComparison.Ordinal)
                ? componentType[6..]
                : componentType;

        if (string.IsNullOrWhiteSpace(strippedPrefix))
        {
            return componentType;
        }

        var spaced = Regex.Replace(strippedPrefix, "([a-z0-9])([A-Z])", "$1 $2", RegexOptions.CultureInvariant);
        return char.ToUpperInvariant(spaced[0]) + spaced[1..];
    }

    private static string ResolveCategory(Type type, string componentType, bool isWrapper, IReadOnlyDictionary<string, string> categories)
    {
        if (categories.TryGetValue(componentType, out var category))
        {
            return category;
        }

        if (isWrapper)
        {
            return type.Namespace switch
            {
                var namespaceName when namespaceName?.Contains("Buttons", StringComparison.Ordinal) == true => "Navigation & Actions",
                var namespaceName when namespaceName?.Contains("Forms", StringComparison.Ordinal) == true => "Forms & Inputs",
                var namespaceName when namespaceName?.Contains("Data", StringComparison.Ordinal) == true => "Data & Scheduling",
                var namespaceName when namespaceName?.Contains("Feedback", StringComparison.Ordinal) == true => "Feedback & Overlays",
                var namespaceName when namespaceName?.Contains("Layout", StringComparison.Ordinal) == true => "Layout & Display",
                _ => "Misc"
            };
        }

        return componentType switch
        {
            var name when Regex.IsMatch(name, "Button|Fab|SplitButton|ProfileMenu|Menu|BreadCrumb|Tabs|Accordion|Steps|Link|ContextMenu|PanelMenu", RegexOptions.CultureInvariant) => "Navigation & Actions",
            var name when Regex.IsMatch(name, "Text|Numeric|Password|Mask|AutoComplete|SecurityCode|FileInput|Upload|DropDown|ListBox|DatePicker|TimeSpanPicker|ColorPicker|Rating|CheckBox|Radio|Switch|Slider|Chip|PickList|DropZone", RegexOptions.CultureInvariant) => "Forms & Inputs",
            var name when Regex.IsMatch(name, "DataGrid|PivotDataGrid|DataList|Table|Tree|Pager|DataFilter|Gantt|Scheduler", RegexOptions.CultureInvariant) => "Data & Scheduling",
            var name when Regex.IsMatch(name, "Chart|Series|Gauge|Sparkline|Sankey|Timeline", RegexOptions.CultureInvariant) => "Data Visualization",
            var name when Regex.IsMatch(name, "Dialog|Popup|Tooltip|Alert|Notification|Progress|Skeleton|Login|Chat|AIChat", RegexOptions.CultureInvariant) => "Feedback & Overlays",
            var name when Regex.IsMatch(name, "Card|Panel|Fieldset|Row|Column|Stack|Splitter|Carousel|QRCode|Barcode|Image|Icon|HtmlEditor|Markdown|Gravatar|RadzenBody|RadzenHeader|RadzenFooter|RadzenSidebar|RadzenLayout", RegexOptions.CultureInvariant) => "Layout & Display",
            _ => "Misc"
        };
    }

    private static string ResolveIcon(string componentType, bool isWrapper)
    {
        if (isWrapper)
        {
            return "extension";
        }

        return componentType switch
        {
            var name when name.Contains("Grid", StringComparison.Ordinal) || name.Contains("Table", StringComparison.Ordinal) => "table_rows",
            var name when name.Contains("Chart", StringComparison.Ordinal) || name.Contains("Gauge", StringComparison.Ordinal) || name.Contains("Series", StringComparison.Ordinal) => "monitoring",
            var name when name.Contains("Dialog", StringComparison.Ordinal) || name.Contains("Popup", StringComparison.Ordinal) || name.Contains("Alert", StringComparison.Ordinal) => "notification_important",
            var name when name.Contains("Row", StringComparison.Ordinal) || name.Contains("Column", StringComparison.Ordinal) || name.Contains("Card", StringComparison.Ordinal) || name.Contains("Stack", StringComparison.Ordinal) => "dashboard",
            _ => "widgets"
        };
    }

    private static IReadOnlyDictionary<string, string> LoadInventoryCategories()
    {
        var repoRoot = FindRepositoryRoot();
        var path = Path.Combine(repoRoot, "docs", "RADZEN-COMPONENT-INVENTORY.md");
        if (!File.Exists(path))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var lines = File.ReadAllLines(path);

        var inComponentTable = false;
        var componentIndex = -1;
        var categoryIndex = -1;

        foreach (var line in lines)
        {
            if (!line.StartsWith('|'))
            {
                inComponentTable = false;
                continue;
            }

            var cells = line.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (cells.Length == 0)
            {
                continue;
            }

            if (cells.Contains("Component", StringComparer.Ordinal) && cells.Contains("Category", StringComparer.Ordinal))
            {
                componentIndex = Array.IndexOf(cells, "Component");
                categoryIndex = Array.IndexOf(cells, "Category");
                inComponentTable = componentIndex >= 0 && categoryIndex >= 0;
                continue;
            }

            if (!inComponentTable || cells.All(static cell => cell.StartsWith("---", StringComparison.Ordinal)))
            {
                continue;
            }

            if (componentIndex >= cells.Length || categoryIndex >= cells.Length)
            {
                continue;
            }

            var component = cells[componentIndex].Split('`')[0].Trim();
            var category = cells[categoryIndex].Trim();
            if (!string.IsNullOrWhiteSpace(component) && !string.IsNullOrWhiteSpace(category))
            {
                result[component] = category;
            }
        }

        return result;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Agterhuis.Ui.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root not found.");
    }
}
