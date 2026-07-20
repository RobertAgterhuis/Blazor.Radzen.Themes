using System.Collections.Concurrent;
using System.Reflection;

namespace Agterhuis.Ui.Demo.Services;

public sealed class DemoSourceProvider
{
    private readonly Assembly _assembly = typeof(Program).Assembly;
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public string GetSource(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return "";
        }

        return _cache.GetOrAdd(sourcePath, ResolveSource);
    }

    private string ResolveSource(string sourcePath)
    {
        var normalized = sourcePath
            .Replace('\\', '/')
            .TrimStart('/');

        var assemblyName = _assembly.GetName().Name ?? "Agterhuis.Ui.Demo";
        var candidate = $"{assemblyName}.{normalized.Replace('/', '.')}";
        var resourceName = _assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => string.Equals(name, candidate, StringComparison.Ordinal))
            ?? _assembly
                .GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith($".{normalized.Replace('/', '.')}", StringComparison.Ordinal));

        if (resourceName is null)
        {
            return $"// Bron niet gevonden: {sourcePath}";
        }

        using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return $"// Bron niet leesbaar: {sourcePath}";
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
