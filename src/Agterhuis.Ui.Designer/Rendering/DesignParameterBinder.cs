using System.Reflection;
using System.Text.Json;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Designer.Rendering;

internal static class DesignParameterBinder
{
    private static readonly MethodInfo GenericNoOpEventCallbackMethod = typeof(DesignParameterBinder)
        .GetMethod(nameof(CreateNoOpEventCallbackCore), BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("No-op EventCallback binder method not found.");

    public static object? Bind(DesignParameterValue value, Type targetType, ComponentBase receiver)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(receiver);

        if (targetType == typeof(EventCallback))
        {
            return EventCallback.Factory.Create(receiver, static () => { });
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(EventCallback<>))
        {
            return GenericNoOpEventCallbackMethod.MakeGenericMethod(targetType.GetGenericArguments()[0])
                .Invoke(null, [receiver]);
        }

        if (!string.IsNullOrWhiteSpace(value.Expression) && value.Literal is null)
        {
            return CreateExpressionPlaceholder(targetType, value.Expression!);
        }

        if (value.Literal is null)
        {
            return CreateDefaultValue(targetType);
        }

        var resolved = value.Literal.Deserialize(targetType, DesignJsonOptions.Default);
        return resolved ?? CreateDefaultValue(targetType);
    }

    private static object CreateNoOpEventCallbackCore<T>(ComponentBase receiver)
        => EventCallback.Factory.Create<T>(receiver, static _ => { });

    private static object? CreateExpressionPlaceholder(Type targetType, string expression)
    {
        if (targetType == typeof(string))
        {
            return $"{{{{{expression}}}}}";
        }

        return CreateDefaultValue(targetType);
    }

    private static object? CreateDefaultValue(Type targetType)
    {
        if (Nullable.GetUnderlyingType(targetType) is not null || !targetType.IsValueType)
        {
            return null;
        }

        return Activator.CreateInstance(targetType);
    }
}