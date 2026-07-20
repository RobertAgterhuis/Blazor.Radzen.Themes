using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using Agterhuis.Ui.Designer.Introspection;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Designer.RegistryGenerator;

public static class Program
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

    public static int Main(string[] args)
    {
        var outputPath = GetOption(args, "--output") ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Agterhuis.Ui.Designer", "Registry", "DesignerComponentRegistry.g.cs"));
        var runtimeAssemblyPath = GetOption(args, "--assembly") ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Agterhuis.Ui.Designer", "bin", "Debug", "net10.0", "Agterhuis.Ui.Designer.dll"));

        var repoRoot = FindRepositoryRoot(outputPath, runtimeAssemblyPath);
        var descriptors = BuildRegistry(repoRoot, runtimeAssemblyPath);
        var source = BuildSource(descriptors);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, source, Encoding.UTF8);

        Console.WriteLine(outputPath);
        Console.WriteLine($"Descriptors written: {descriptors.Count}");
        return 0;
    }

    private static string? GetOption(IReadOnlyList<string> args, string name)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(args[index + 1]);
            }
        }

        return null;
    }

    private static string FindRepositoryRoot(string outputPath, string assemblyPath)
    {
        var start = new DirectoryInfo(Path.GetDirectoryName(outputPath) ?? Path.GetDirectoryName(assemblyPath) ?? AppContext.BaseDirectory);
        while (start is not null)
        {
            if (File.Exists(Path.Combine(start.FullName, "Agterhuis.Ui.sln")))
            {
                return start.FullName;
            }

            start = start.Parent;
        }

        throw new InvalidOperationException("Repository root not found.");
    }

    private static IReadOnlyList<DescriptorSnapshot> BuildRegistry(string repoRoot, string runtimeAssemblyPath)
    {
        var categories = LoadInventoryCategories(repoRoot);
        var wrapperAssemblyPath = FindAssemblyPath(repoRoot,
            Path.Combine("src", "Agterhuis.Ui", "bin", "Release", "net10.0", "Agterhuis.Ui.dll"),
            Path.Combine("src", "Agterhuis.Ui", "bin", "Debug", "net10.0", "Agterhuis.Ui.dll"));
        var wrapperAssembly = LoadAssembly(wrapperAssemblyPath);
        var componentBaseType = typeof(IComponent);
        var descriptors = new List<DescriptorSnapshot>();

        descriptors.AddRange(DiscoverComponents(wrapperAssembly, isWrapper: true, categories, componentBaseType));

        var radzenAssembly = LoadRadzenAssembly(wrapperAssembly);
        descriptors.AddRange(DiscoverComponents(radzenAssembly, isWrapper: false, categories, componentBaseType));

        return descriptors
            .OrderBy(static descriptor => descriptor.ComponentType, StringComparer.Ordinal)
            .ToArray();
    }

    private static string FindAssemblyPath(string repoRoot, params string[] candidatePaths)
    {
        foreach (var relativePath in candidatePaths)
        {
            var candidate = Path.Combine(repoRoot, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Unable to locate assembly. Tried: {string.Join(", ", candidatePaths)}");
    }

    private static Assembly LoadAssembly(string assemblyPath)
    {
        var directory = Path.GetDirectoryName(assemblyPath) ?? AppContext.BaseDirectory;
        foreach (var dependency in Directory.EnumerateFiles(directory, "*.dll"))
        {
            try
            {
                Assembly.LoadFrom(dependency);
            }
            catch
            {
                // Ignore dependencies that are already loaded or unavailable in this context.
            }
        }

        return Assembly.LoadFrom(assemblyPath);
    }

    private static Assembly LoadRadzenAssembly(Assembly wrapperAssembly)
    {
        var reference = wrapperAssembly.GetReferencedAssemblies()
            .FirstOrDefault(name => string.Equals(name.Name, "Radzen.Blazor", StringComparison.Ordinal));

        if (reference is not null)
        {
            return Assembly.Load(reference);
        }

        throw new InvalidOperationException("Unable to locate Radzen.Blazor.dll for registry generation.");
    }

    private static IEnumerable<DescriptorSnapshot> DiscoverComponents(
        Assembly assembly,
        bool isWrapper,
        IReadOnlyDictionary<string, string> categories,
        Type componentBaseType)
    {
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

            yield return new DescriptorSnapshot(
                key,
                renderableType,
                ToDisplayName(key),
                ResolveCategory(type, key, isWrapper, categories),
                ResolveIcon(key, isWrapper),
                isWrapper || PaletteRawComponents.Contains(key),
                isWrapper,
                parameters.Where(static parameter => parameter.IsRenderFragment)
                    .Select(static parameter => parameter.Name)
                    .OrderBy(static slot => slot, StringComparer.Ordinal)
                    .ToArray(),
                parameters.Select(parameter => new ParameterSnapshot(
                    parameter.Name,
                    parameter.TypeName,
                    parameter.ParameterType,
                    parameter.DefaultValue,
                    parameter.Description,
                    parameter.IsEditorRequired,
                    parameter.IsEventCallback,
                    parameter.IsRenderFragment,
                    parameter.IsTemplatedRenderFragment)).ToArray());
        }
    }

    private static Type CloseRenderableType(Type type)
    {
        if (!type.ContainsGenericParameters)
        {
            return type;
        }

        var arguments = Enumerable.Repeat(typeof(object), type.GetGenericArguments().Length).ToArray();
        return type.MakeGenericType(arguments);
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

    private static IReadOnlyDictionary<string, string> LoadInventoryCategories(string repoRoot)
    {
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

    private static string BuildSource(IReadOnlyList<DescriptorSnapshot> descriptors)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using Agterhuis.Ui.Components.Layout;");
        builder.AppendLine("using Microsoft.AspNetCore.Components;");
        builder.AppendLine("using Radzen;");
        builder.AppendLine();
        builder.AppendLine("namespace Agterhuis.Ui.Designer.Registry;");
        builder.AppendLine();
        builder.AppendLine("public sealed partial class DesignerComponentRegistry");
        builder.AppendLine("{");
        builder.AppendLine("    // Generated metadata roots all component types so WASM trimming keeps the registry surface alive.");
        builder.AppendLine("    private static partial DesignerComponentRegistry BuildDefault()");
        builder.AppendLine("    {");
        builder.AppendLine("        return new DesignerComponentRegistry(new[]");
        builder.AppendLine("        {");

        foreach (var descriptor in descriptors)
        {
            AppendDescriptor(builder, descriptor);
        }

        builder.AppendLine("        });");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static DesignerComponentDescriptor CreateDescriptor(");
        builder.AppendLine("        string componentType,");
        builder.AppendLine("        Type clrType,");
        builder.AppendLine("        string displayName,");
        builder.AppendLine("        string category,");
        builder.AppendLine("        string icon,");
        builder.AppendLine("        bool allowedInPalette,");
        builder.AppendLine("        bool isWrapper,");
        builder.AppendLine("        IReadOnlyList<string> slots,");
        builder.AppendLine("        IReadOnlyList<ComponentParameterDescriptor> parameters)");
        builder.AppendLine("        => new(componentType, clrType, displayName, category, icon, allowedInPalette, isWrapper, slots, parameters);");
        builder.AppendLine();
        builder.AppendLine("    private static ComponentParameterDescriptor CreateParameter(");
        builder.AppendLine("        string name,");
        builder.AppendLine("        string typeName,");
        builder.AppendLine("        Type parameterType,");
        builder.AppendLine("        string defaultValue,");
        builder.AppendLine("        string description,");
        builder.AppendLine("        bool isEditorRequired,");
        builder.AppendLine("        bool isEventCallback,");
        builder.AppendLine("        bool isRenderFragment,");
        builder.AppendLine("        bool isTemplatedRenderFragment)");
        builder.AppendLine("        => new(name, typeName, parameterType, defaultValue, description, isEditorRequired, isEventCallback, isRenderFragment, isTemplatedRenderFragment);");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static void AppendDescriptor(StringBuilder builder, DescriptorSnapshot descriptor)
    {
        builder.AppendLine("            CreateDescriptor(");
        builder.AppendLine($"                {ToLiteral(descriptor.ComponentType)},");
        builder.AppendLine($"                {GetTypeExpression(descriptor.ClrType)},");
        builder.AppendLine($"                {ToLiteral(descriptor.DisplayName)},");
        builder.AppendLine($"                {ToLiteral(descriptor.Category)},");
        builder.AppendLine($"                {ToLiteral(descriptor.Icon)},");
        builder.AppendLine($"                {descriptor.AllowedInPalette.ToString().ToLowerInvariant()},");
        builder.AppendLine($"                {descriptor.IsWrapper.ToString().ToLowerInvariant()},");
        builder.AppendLine("                new string[]");
        builder.AppendLine("                {");

        foreach (var slot in descriptor.Slots)
        {
            builder.AppendLine($"                    {ToLiteral(slot)},");
        }

        builder.AppendLine("                },");
        builder.AppendLine("                new ComponentParameterDescriptor[]");
        builder.AppendLine("                {");

        foreach (var parameter in descriptor.Parameters)
        {
            AppendParameter(builder, parameter);
        }

        builder.AppendLine("                }),");
    }

    private static void AppendParameter(StringBuilder builder, ParameterSnapshot parameter)
    {
        builder.AppendLine("                    CreateParameter(");
        builder.AppendLine($"                        {ToLiteral(parameter.Name)},");
        builder.AppendLine($"                        {ToLiteral(parameter.TypeName)},");
        builder.AppendLine($"                        {GetTypeExpression(parameter.ParameterType)},");
        builder.AppendLine($"                        {ToLiteral(parameter.DefaultValue)},");
        builder.AppendLine($"                        {ToLiteral(parameter.Description)},");
        builder.AppendLine($"                        {parameter.IsEditorRequired.ToString().ToLowerInvariant()},");
        builder.AppendLine($"                        {parameter.IsEventCallback.ToString().ToLowerInvariant()},");
        builder.AppendLine($"                        {parameter.IsRenderFragment.ToString().ToLowerInvariant()},");
        builder.AppendLine($"                        {parameter.IsTemplatedRenderFragment.ToString().ToLowerInvariant()}),");
    }

    private static string ToLiteral(string? value)
        => value is null ? "null" : $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")}\"";

    private static string GetTypeExpression(Type type)
    {
        if (type == typeof(void))
        {
            return "typeof(void)";
        }

        if (type.IsByRef)
        {
            return GetTypeExpression(type.GetElementType()!) + "&";
        }

        return $"typeof({GetTypeSyntax(type)})";
    }

    private static string GetTypeSyntax(Type type)
    {
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsArray)
        {
            return $"{GetTypeSyntax(type.GetElementType()!)}[]";
        }

        if (type.IsGenericType)
        {
            var genericDefinition = type.GetGenericTypeDefinition();
            var genericName = GetNonGenericTypeName(genericDefinition);
            var arguments = string.Join(", ", type.GetGenericArguments().Select(GetTypeSyntax));
            return $"global::{genericName}<{arguments}>";
        }

        if (type.IsNested)
        {
            return $"{GetTypeSyntax(type.DeclaringType!)}.{type.Name.Split('`')[0]}";
        }

        var name = string.IsNullOrWhiteSpace(type.Namespace)
            ? type.Name
            : $"{type.Namespace}.{type.Name}";

        return $"global::{name.Split('`')[0]}";
    }

    private static string GetNonGenericTypeName(Type type)
    {
        if (type.IsNested)
        {
            return $"{GetNonGenericTypeName(type.DeclaringType!)}.{type.Name.Split('`')[0]}";
        }

        return string.IsNullOrWhiteSpace(type.Namespace)
            ? type.Name.Split('`')[0]
            : $"{type.Namespace}.{type.Name.Split('`')[0]}";
    }

    private sealed record DescriptorSnapshot(
        string ComponentType,
        Type ClrType,
        string DisplayName,
        string Category,
        string Icon,
        bool AllowedInPalette,
        bool IsWrapper,
        IReadOnlyList<string> Slots,
        IReadOnlyList<ParameterSnapshot> Parameters);

    private sealed record ParameterSnapshot(
        string Name,
        string TypeName,
        Type ParameterType,
        string DefaultValue,
        string Description,
        bool IsEditorRequired,
        bool IsEventCallback,
        bool IsRenderFragment,
        bool IsTemplatedRenderFragment);
}
