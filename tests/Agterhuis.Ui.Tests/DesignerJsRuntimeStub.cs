using Microsoft.JSInterop;

namespace Agterhuis.Ui.Tests;

internal sealed class DesignerJsRuntimeStub : IJSRuntime
{
    private readonly Dictionary<string, object?> _results = new(StringComparer.Ordinal);

    public void SetResult<TValue>(string identifier, TValue value)
        => _results[identifier] = value;

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        if (_results.TryGetValue(identifier, out var value) && value is TValue typedValue)
        {
            return ValueTask.FromResult(typedValue);
        }

        if (typeof(TValue) == typeof(string))
        {
            return ValueTask.FromResult((TValue)(object?)string.Empty!);
        }

        return ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);
}