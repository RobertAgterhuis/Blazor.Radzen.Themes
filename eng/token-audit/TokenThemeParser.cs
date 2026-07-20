using System.Text.RegularExpressions;

namespace TokenAudit;

public static class TokenThemeParser
{
    private static readonly Regex ThemeSelectorRegex = new("data-agt-theme=\"(?<variant>[a-z0-9-]+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CustomPropertyRegex = new("(?<name>--[a-z0-9-]+)\\s*:\\s*(?<value>[^;{}]+);", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<ParsedThemeFile> ParseThemeFiles(string repoRoot)
    {
        var themeDir = Path.Combine(repoRoot, "src", "Agterhuis.Ui", "wwwroot", "css", "themes");
        var files = Directory.EnumerateFiles(themeDir, "agt-theme.*.css", SearchOption.TopDirectoryOnly)
            .Where(file => !file.EndsWith("agt-theme.plum.css", StringComparison.OrdinalIgnoreCase) || File.Exists(file))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var parsed = new List<ParsedThemeFile>();
        foreach (var file in files)
        {
            var themeFile = ParseThemeFile(file);
            if (themeFile is not null)
            {
                parsed.Add(themeFile);
            }
        }

        return parsed;
    }

    public static ParsedThemeFile? ParseThemeFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var family = fileName.StartsWith("agt-theme.", StringComparison.OrdinalIgnoreCase)
            ? fileName["agt-theme.".Length..]
            : fileName;

        var css = File.ReadAllText(filePath);
        var modes = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in CssBlockScanner.FindBlocks(css))
        {
            var match = ThemeSelectorRegex.Match(block.Selector);
            if (!match.Success)
            {
                continue;
            }

            var variant = match.Groups["variant"].Value;
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match property in CustomPropertyRegex.Matches(block.Body))
            {
                tokens[property.Groups["name"].Value] = property.Groups["value"].Value.Trim();
            }

            if (tokens.Count > 0)
            {
                modes[variant] = tokens;
            }
        }

        return new ParsedThemeFile(filePath, family, modes);
    }
}

public sealed record ParsedThemeFile(string FilePath, string Family, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Modes);

internal static class CssBlockScanner
{
    public static IReadOnlyList<CssBlock> FindBlocks(string css)
    {
        var blocks = new List<CssBlock>();
        var line = 1;
        Scan(css, 0, css.Length, false, string.Empty, ref line, blocks);
        return blocks;
    }

    private static void Scan(string css, int start, int end, bool inheritedThemeScope, string inheritedSelector, ref int line, List<CssBlock> blocks)
    {
        var index = start;
        while (index < end)
        {
            var selectorStart = index;
            while (index < end && css[index] != '{')
            {
                if (css[index] == '\n')
                {
                    line++;
                }

                index++;
            }

            if (index >= end)
            {
                return;
            }

            var selector = css.Substring(selectorStart, index - selectorStart).Trim();
            var openLine = line;
            index++;
            var bodyStart = index;
            var depth = 1;
            while (index < end && depth > 0)
            {
                if (css[index] == '{')
                {
                    depth++;
                }
                else if (css[index] == '}')
                {
                    depth--;
                }

                if (css[index] == '\n')
                {
                    line++;
                }

                index++;
            }

            var body = css.Substring(bodyStart, Math.Max(0, index - bodyStart - 1));
            var selectorPath = string.IsNullOrWhiteSpace(inheritedSelector) ? selector : $"{inheritedSelector} {selector}";
            var isThemeScope = inheritedThemeScope || selector.Contains("data-agt-theme", StringComparison.OrdinalIgnoreCase);
            blocks.Add(new CssBlock(selectorPath, body, openLine, isThemeScope));

            if (selector.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                var nestedLine = openLine;
                Scan(body, 0, body.Length, isThemeScope, selectorPath, ref nestedLine, blocks);
            }
        }
    }
}

internal sealed record CssBlock(string Selector, string Body, int StartLine, bool IsThemeScoped);