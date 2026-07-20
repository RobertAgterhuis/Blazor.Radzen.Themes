using System.Text.Json;
using System.Text.Json.Nodes;
using TokenAudit;

namespace TokenExport;

public static class TokenExportEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static TokenExportReport Export(string repoRoot, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        var report = new TokenExportReport();
        foreach (var parsedTheme in TokenThemeParser.ParseThemeFiles(repoRoot))
        {
            var designTokens = BuildDesignTokensDocument(parsedTheme);
            var styleDictionary = BuildStyleDictionaryDocument(parsedTheme);

            var designPath = Path.Combine(outputDir, $"design-tokens.{parsedTheme.Family}.json");
            var stylePath = Path.Combine(outputDir, $"style-dictionary.{parsedTheme.Family}.json");

            File.WriteAllText(designPath, designTokens.ToJsonString(JsonOptions));
            File.WriteAllText(stylePath, styleDictionary.ToJsonString(JsonOptions));

            report.Artifacts.Add(designPath);
            report.Artifacts.Add(stylePath);
            report.Families.Add(parsedTheme.Family);
        }

        return report;
    }

    private static JsonObject BuildDesignTokensDocument(ParsedThemeFile parsedTheme)
    {
        var document = new JsonObject
        {
            ["family"] = parsedTheme.Family,
            ["modes"] = new JsonArray(parsedTheme.Modes.Keys.Select(mode => (JsonNode?)mode).ToArray()),
            ["tokens"] = BuildTokenTree(parsedTheme)
        };

        return document;
    }

    private static JsonObject BuildStyleDictionaryDocument(ParsedThemeFile parsedTheme)
    {
        var document = new JsonObject();
        foreach (var token in GetTokenEntries(parsedTheme))
        {
            var entry = new JsonObject();
            foreach (var (mode, value) in token.Values)
            {
                entry[mode] = value;
            }

            document[token.Path] = entry;
        }

        return document;
    }

    private static JsonObject BuildTokenTree(ParsedThemeFile parsedTheme)
    {
        var root = new JsonObject();
        foreach (var token in GetTokenEntries(parsedTheme))
        {
            var current = root;
            for (var index = 0; index < token.PathSegments.Length - 1; index++)
            {
                var segment = token.PathSegments[index];
                if (current[segment] is not JsonObject child)
                {
                    child = CreateChild(current, segment);
                }

                current = child;
            }

            var leaf = new JsonObject
            {
                ["$type"] = token.Type,
                ["$value"] = BuildModeValueObject(token.Values)
            };

            current[token.PathSegments[^1]] = leaf;
        }

        return root;
    }

    private static JsonObject CreateChild(JsonObject parent, string segment)
    {
        var child = new JsonObject();
        parent[segment] = child;
        return child;
    }

    private static JsonObject BuildModeValueObject(IReadOnlyDictionary<string, string> values)
    {
        var result = new JsonObject();
        foreach (var (mode, value) in values)
        {
            result[mode] = value;
        }

        return result;
    }

    private static IReadOnlyList<TokenEntry> GetTokenEntries(ParsedThemeFile parsedTheme)
    {
        var tokens = new Dictionary<string, TokenEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var (mode, modeTokens) in parsedTheme.Modes)
        {
            foreach (var (name, value) in modeTokens)
            {
                var path = NormalizeTokenPath(name);
                if (!tokens.TryGetValue(path, out var entry))
                {
                    entry = new TokenEntry(path, path.Split('/').ToArray(), GetTokenType(name, value), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
                    tokens[path] = entry;
                }

                entry.Values[mode] = value;
            }
        }

        return tokens.Values.OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string NormalizeTokenPath(string tokenName)
    {
        return tokenName.StartsWith("--agt-", StringComparison.OrdinalIgnoreCase)
            ? tokenName[6..].Replace('-', '/')
            : tokenName.TrimStart('-').Replace('-', '/');
    }

    private static string GetTokenType(string tokenName, string value)
    {
        var normalizedName = tokenName.Replace("--agt-", string.Empty, StringComparison.OrdinalIgnoreCase);
        if (LooksLikeColor(tokenName, value)) return "color";
        if (normalizedName.StartsWith("font-", StringComparison.OrdinalIgnoreCase) || normalizedName.StartsWith("heading-", StringComparison.OrdinalIgnoreCase) || normalizedName.Contains("tracking", StringComparison.OrdinalIgnoreCase)) return "typography";
        if (normalizedName.Contains("spacing", StringComparison.OrdinalIgnoreCase) || normalizedName.Contains("padding", StringComparison.OrdinalIgnoreCase) || normalizedName.Contains("height", StringComparison.OrdinalIgnoreCase)) return "dimension";
        if (normalizedName.Contains("radius", StringComparison.OrdinalIgnoreCase)) return "dimension";
        if (normalizedName.Contains("shadow", StringComparison.OrdinalIgnoreCase)) return "shadow";
        if (normalizedName.Contains("transition", StringComparison.OrdinalIgnoreCase) || normalizedName.Contains("duration", StringComparison.OrdinalIgnoreCase)) return "duration";
        return "string";
    }

    private static bool LooksLikeColor(string tokenName, string value)
    {
        if (value.StartsWith("#", StringComparison.OrdinalIgnoreCase)
            || value.Contains("rgb(", StringComparison.OrdinalIgnoreCase)
            || value.Contains("rgba(", StringComparison.OrdinalIgnoreCase)
            || value.Contains("hsl(", StringComparison.OrdinalIgnoreCase)
            || value.Contains("oklch(", StringComparison.OrdinalIgnoreCase)
            || value.Contains("gradient(", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return tokenName.Contains("color", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("surface", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("text", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("link", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("nav", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("topbar", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("grid", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("input", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("btn", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("chart", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("alpha", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("on-", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("glass", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("glow", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("hover", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("selection", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("focus", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("canvas", StringComparison.OrdinalIgnoreCase)
            || tokenName.Contains("gradient", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record TokenEntry(string Path, string[] PathSegments, string Type, Dictionary<string, string> Values);
}

public sealed class TokenExportReport
{
    public List<string> Families { get; } = [];
    public List<string> Artifacts { get; } = [];
    public int FamiliesExported => Families.Count;
    public int ArtifactsWritten => Artifacts.Count;
}