using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Designer.Introspection;

public static class ComponentParameterIntrospector
{
    private static readonly Dictionary<Assembly, IReadOnlyDictionary<string, string>> SummaryCache = [];
    private static readonly Lock CacheLock = new();

    public static IReadOnlyList<ComponentParameterDescriptor> Describe(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var summaries = GetSummaries(componentType.Assembly);
        var rows = new List<ComponentParameterDescriptor>();

        object? instance = null;
        if (componentType.GetConstructor(Type.EmptyTypes) is not null)
        {
            try
            {
                instance = Activator.CreateInstance(componentType);
            }
            catch
            {
                instance = null;
            }
        }

        foreach (var property in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(static prop => prop.GetCustomAttribute<ParameterAttribute>() is not null)
                     .OrderBy(static prop => prop.Name, StringComparer.Ordinal))
        {
            var parameterType = property.PropertyType;
            rows.Add(new ComponentParameterDescriptor(
                property.Name,
                ToFriendlyTypeName(parameterType),
                parameterType,
                GetDefaultValue(property, instance),
                GetSummary(summaries, componentType, property.Name),
                IsBindable(property.Name, parameterType),
                property.GetCustomAttribute<EditorRequiredAttribute>() is not null,
                IsEventCallback(parameterType),
                IsRenderFragment(parameterType),
                IsTemplatedRenderFragment(parameterType)));
        }

        return rows;
    }

    private static IReadOnlyDictionary<string, string> GetSummaries(Assembly assembly)
    {
        lock (CacheLock)
        {
            if (SummaryCache.TryGetValue(assembly, out var cached))
            {
                return cached;
            }

            var loaded = LoadSummaries(assembly);
            SummaryCache[assembly] = loaded;
            return loaded;
        }
    }

    private static IReadOnlyDictionary<string, string> LoadSummaries(Assembly assembly)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");

        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            return result;
        }

        try
        {
            var document = XDocument.Load(xmlPath);
            var members = document.Root?.Element("members")?.Elements("member") ?? [];

            foreach (var member in members)
            {
                var name = member.Attribute("name")?.Value;
                var summary = member.Element("summary")?.Value?.Trim();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(summary))
                {
                    continue;
                }

                result[name] = string.Join(' ', summary.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return result;
    }

    private static string ToFriendlyTypeName(Type type)
    {
        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(bool))
        {
            return "bool";
        }

        if (type == typeof(decimal))
        {
            return "decimal";
        }

        if (type == typeof(double))
        {
            return "double";
        }

        if (type == typeof(float))
        {
            return "float";
        }

        if (Nullable.GetUnderlyingType(type) is { } nullableType)
        {
            return $"{ToFriendlyTypeName(nullableType)}?";
        }

        if (type.IsGenericType)
        {
            var genericName = type.Name.Split('`')[0];
            var arguments = string.Join(", ", type.GetGenericArguments().Select(ToFriendlyTypeName));
            return $"{genericName}<{arguments}>";
        }

        return type.Name;
    }

    private static string GetDefaultValue(PropertyInfo property, object? instance)
    {
        if (instance is null)
        {
            return "-";
        }

        object? value;
        try
        {
            value = property.GetValue(instance);
        }
        catch
        {
            return "-";
        }

        if (value is null)
        {
            return "null";
        }

        try
        {
            return value switch
            {
                string text when string.IsNullOrEmpty(text) => "\"\"",
                string text => text,
                bool boolean => boolean ? "true" : "false",
                _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "-"
            };
        }
        catch
        {
            return "-";
        }
    }

    private static string GetSummary(IReadOnlyDictionary<string, string> summaries, Type componentType, string propertyName)
    {
        var memberName = $"P:{componentType.FullName}.{propertyName}";
        return summaries.TryGetValue(memberName, out var summary)
            ? summary
            : "-";
    }

    private static bool IsEventCallback(Type type) =>
        type == typeof(EventCallback) ||
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EventCallback<>);

    private static bool IsRenderFragment(Type type) =>
        type == typeof(RenderFragment) || IsTemplatedRenderFragment(type);

    private static bool IsTemplatedRenderFragment(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(RenderFragment<>);

    private static bool IsBindable(string propertyName, Type propertyType)
    {
        if (IsEventCallback(propertyType) || IsRenderFragment(propertyType))
        {
            return false;
        }

        return propertyName switch
        {
            "Data" or "Items" or "Value" or "TextProperty" or "ValueProperty" or "Count" or "Query" or "Filter" or "SearchText" => true,
            _ => false
        };
    }
}
