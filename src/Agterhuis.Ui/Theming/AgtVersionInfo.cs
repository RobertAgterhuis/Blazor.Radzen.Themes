using System.Reflection;

namespace Agterhuis.Ui.Theming;

public static class AgtVersionInfo
{
    private static readonly Lazy<string> DisplayVersionLazy = new(ResolveDisplayVersion);

    public static string DisplayVersion => DisplayVersionLazy.Value;

    public static string ProductVersionLabel => $"Agterhuis.Ui v{DisplayVersion}";

    private static string ResolveDisplayVersion()
    {
        var assembly = typeof(AgtThemeState).Assembly;

        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        var assemblyVersion = assembly.GetName().Version?.ToString(3);

        return NormalizeDisplayVersion(informational)
            ?? NormalizeDisplayVersion(fileVersion)
            ?? NormalizeDisplayVersion(assemblyVersion)
            ?? "dev";
    }

    private static string? NormalizeDisplayVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        var trimmed = version.Trim();
        var plusIndex = trimmed.IndexOf('+', StringComparison.Ordinal);
        var withoutBuildMetadata = plusIndex >= 0 ? trimmed[..plusIndex] : trimmed;
        return string.IsNullOrWhiteSpace(withoutBuildMetadata) ? null : withoutBuildMetadata;
    }
}