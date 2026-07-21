using Microsoft.JSInterop;

namespace Agterhuis.Ui.Tests;

internal sealed class DesignerJsRuntimeStub : IJSRuntime
{
    private readonly Dictionary<string, object?> _results = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _storage = new(StringComparer.Ordinal);

    public void SetResult<TValue>(string identifier, TValue value)
        => _results[identifier] = value;

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        args ??= [];

        if (identifier == "designerInterop.setJson" && args.Length >= 2)
        {
            var key = args[0]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(key))
            {
                var payload = args[1]?.ToString() ?? string.Empty;
                _storage[key] = payload;
            }

            return ValueTask.FromResult(default(TValue)!);
        }

        if (identifier == "designerInterop.removeItem" && args.Length >= 1)
        {
            var key = args[0]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(key))
            {
                _storage.Remove(key);
            }

            return ValueTask.FromResult(default(TValue)!);
        }

        if (identifier == "designerInterop.getText" && args.Length >= 1)
        {
            var key = args[0]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(key) && _storage.TryGetValue(key, out var storedValue))
            {
                return ValueTask.FromResult((TValue)(object)storedValue);
            }
        }

        if (identifier == "designerInterop.getJson" && args.Length >= 1)
        {
            var key = args[0]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(key) && _storage.TryGetValue(key, out var storedJson))
            {
                try
                {
                    var deserialized = System.Text.Json.JsonSerializer.Deserialize<TValue>(storedJson);
                    if (deserialized is not null)
                    {
                        return ValueTask.FromResult(deserialized);
                    }
                }
                catch
                {
                    // Fall through to default behavior.
                }
            }
        }

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