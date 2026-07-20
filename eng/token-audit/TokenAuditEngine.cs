using System.Text;
using System.Text.RegularExpressions;

namespace TokenAudit;

public static class TokenAuditEngine
{
    private static readonly Regex CustomPropertyRegex = new(@"(?<name>--[a-z0-9-]+)\s*:\s*(?<value>[^;{}]+);", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HexColorRegex = new(@"(?<![A-Za-z0-9_])#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\b", RegexOptions.Compiled);
    private static readonly Regex FunctionalColorRegex = new(@"\b(?:rgb|rgba|hsl|hsla|oklch|oklab)\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex GradientRegex = new(@"\b(?:linear|radial|conic)-gradient\s*\((?<value>[^)]*)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BoxShadowRegex = new(@"\bbox-shadow\s*:\s*(?<value>[^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StringLiteralRegex = new("@\"(?<value>(?:[^\"]|\"\")*)\"|\"(?<value>(?:[^\"\\\\]|\\\\.)*)\"|'(?<value>(?:[^'\\\\]|\\\\.)*)'", RegexOptions.Compiled);
    private static readonly Regex RazorColorAttributeRegex = new("\\b(?:style|fill|stroke|color)\\s*=\\s*(?:\"(?<value>[^\"\\r\\n]*)\"|'(?<value>[^'\\r\\n]*)')", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AllowedLiteralRegex = new(@"\b(?:transparent|currentColor|inherit|none)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex NamedColorRegex = BuildNamedColorRegex();

    private static readonly string[] ThemeFileNames =
    [
        "agt-theme.plum.css",
        "agt-theme.ocean.css",
        "agt-theme.dagobah.css",
        "agt-theme.dathomir.css",
        "agt-theme.hoth.css",
        "agt-theme.tatooine.css",
        "agt-theme.imperial.css",
        "agt-theme.azure.css",
        "agt-theme.ms365.css",
        "agt-theme.volt.css",
        "agt-theme.autotaalglas.css",
        "agt-theme.autotaalglas-contrast.css",
        "agt-theme.autotaalglas-portal.css",
        "agt-theme.autotaalglas-mono.css"
    ];

    public static TokenAuditReport Generate(string repoRoot)
    {
        var report = new TokenAuditReport();
        var literalAllowlist = TokenAuditAllowlist.Load(repoRoot);

        foreach (var file in EnumerateAuditFiles(repoRoot))
        {
            var text = File.ReadAllText(file);

            if (file.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                ScanScopeAudit(report, file, text);
            }

            ScanLiteralAudit(report, file, text, literalAllowlist);
        }

        ScanParityAudit(report, repoRoot);
        return report;
    }

    private static Regex BuildNamedColorRegex()
    {
        const string colorNames =
            "aliceblue|antiquewhite|aqua|aquamarine|azure|beige|bisque|black|blanchedalmond|blue|blueviolet|brown|burlywood|cadetblue|chartreuse|chocolate|coral|cornflowerblue|cornsilk|crimson|cyan|darkblue|darkcyan|darkgoldenrod|darkgray|darkgreen|darkgrey|darkkhaki|darkmagenta|darkolivegreen|darkorange|darkorchid|darkred|darksalmon|darkseagreen|darkslateblue|darkslategray|darkslategrey|darkturquoise|darkviolet|deeppink|deepskyblue|dimgray|dimgrey|dodgerblue|firebrick|floralwhite|forestgreen|fuchsia|gainsboro|ghostwhite|gold|goldenrod|gray|green|greenyellow|grey|honeydew|hotpink|indianred|indigo|ivory|khaki|lavender|lavenderblush|lawngreen|lemonchiffon|lightblue|lightcoral|lightcyan|lightgoldenrodyellow|lightgray|lightgreen|lightgrey|lightpink|lightsalmon|lightseagreen|lightskyblue|lightslategray|lightslategrey|lightsteelblue|lightyellow|lime|limegreen|linen|magenta|maroon|mediumaquamarine|mediumblue|mediumorchid|mediumpurple|mediumseagreen|mediumslateblue|mediumspringgreen|mediumturquoise|mediumvioletred|midnightblue|mintcream|mistyrose|moccasin|navajowhite|navy|oldlace|olive|olivedrab|orange|orangered|orchid|palegoldenrod|palegreen|paleturquoise|palevioletred|papayawhip|peachpuff|peru|pink|plum|powderblue|purple|rebeccapurple|red|rosybrown|royalblue|saddlebrown|salmon|sandybrown|seagreen|seashell|sienna|silver|skyblue|slateblue|slategray|slategrey|snow|springgreen|steelblue|tan|teal|thistle|tomato|turquoise|violet|wheat|white|whitesmoke|yellow|yellowgreen";
        return new Regex($@"(?<![A-Za-z0-9_-])(?:{colorNames})\b(?!-)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    private static IEnumerable<string> EnumerateAuditFiles(string repoRoot)
    {
        var roots = new[]
        {
            Path.Combine(repoRoot, "src", "Agterhuis.Ui"),
            Path.Combine(repoRoot, "samples", "Agterhuis.Ui.Demo")
        };

        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                if (ShouldSkip(file))
                {
                    continue;
                }

                var extension = Path.GetExtension(file);
                if (extension is ".css" or ".razor" or ".cs" or ".js")
                {
                    yield return file;
                }
            }
        }
    }

    private static bool ShouldSkip(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/wwwroot/lib/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/TestResults/", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".styles.css", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase);
    }

    private static void ScanScopeAudit(TokenAuditReport report, string filePath, string css)
    {
        foreach (var block in CssBlockScanner.FindBlocks(css))
        {
            if (!block.IsThemeScoped)
            {
                foreach (Match match in CustomPropertyRegex.Matches(block.Body))
                {
                    if (!IsColorBearingValue(match.Groups["value"].Value))
                    {
                        continue;
                    }

                    report.ScopeViolations.Add(new TokenFinding(
                        "scope",
                        filePath,
                        block.StartLine,
                        block.Selector.Trim(),
                        $"Color-bearing custom property {match.Groups["name"].Value} is defined outside a data-agt-theme scope.",
                        match.Value.Trim()));
                }
            }
        }
    }

    private static void ScanLiteralAudit(TokenAuditReport report, string filePath, string content, TokenAuditAllowlist allowlist)
    {
        var normalized = filePath.Replace('\\', '/');
        if (normalized.Contains("/src/Agterhuis.Ui/wwwroot/css/agt-tokens.css", StringComparison.OrdinalIgnoreCase)
            || ThemeFileNames.Any(themeFile => normalized.EndsWith($"/wwwroot/css/themes/{themeFile}", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var lines = content.Split('\n');
        var extension = Path.GetExtension(filePath);
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            foreach (var candidate in GetAuditCandidates(extension, line))
            {
                if (!ContainsColorLiteral(candidate.Text, candidate.AllowNamedColors))
                {
                    continue;
                }

                if (allowlist.IsAllowed(normalized, line, candidate.Text))
                {
                    report.AllowlistedLiterals.Add(new TokenFinding("allowlist", filePath, index + 1, string.Empty, "Allowed literal.", candidate.Text.Trim()));
                    continue;
                }

                report.LiteralViolations.Add(new TokenFinding(
                    "literal",
                    filePath,
                    index + 1,
                    string.Empty,
                    "Raw color literal found outside theme token files.",
                    candidate.Text.Trim()));
            }
        }
    }

    private static IEnumerable<AuditCandidate> GetAuditCandidates(string extension, string line)
    {
        if (extension.Equals(".css", StringComparison.OrdinalIgnoreCase))
        {
            yield return new AuditCandidate(line, AllowNamedColors: true);
            yield break;
        }

        if (extension.Equals(".razor", StringComparison.OrdinalIgnoreCase))
        {
            foreach (Match attribute in RazorColorAttributeRegex.Matches(line))
            {
                var value = attribute.Groups["value"].Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return new AuditCandidate(value, AllowNamedColors: true);
                }
            }
        }

        foreach (Match literal in StringLiteralRegex.Matches(line))
        {
            var value = literal.Groups["value"].Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                yield return new AuditCandidate(value, AllowNamedColors: false);
            }
        }
    }

    private static void ScanParityAudit(TokenAuditReport report, string repoRoot)
    {
        var themeDir = Path.Combine(repoRoot, "src", "Agterhuis.Ui", "wwwroot", "css", "themes");
        var baselineFile = Path.Combine(themeDir, "agt-theme.plum.css");
        var baselineTokens = GetDeclaredTokens(File.ReadAllText(baselineFile));

        foreach (var fileName in ThemeFileNames)
        {
            var filePath = Path.Combine(themeDir, fileName);
            var tokens = GetDeclaredTokens(File.ReadAllText(filePath));
            var missing = baselineTokens.Except(tokens, StringComparer.OrdinalIgnoreCase).ToArray();
            var extra = tokens.Except(baselineTokens, StringComparer.OrdinalIgnoreCase).ToArray();

            if (missing.Length == 0 && extra.Length == 0)
            {
                report.ParityScans.Add(new TokenFinding("parity", filePath, 0, fileName, "Token set matches baseline.", string.Empty));
                continue;
            }

            report.ParityViolations.Add(new TokenFinding(
                "parity",
                filePath,
                0,
                fileName,
                $"Token parity mismatch. Missing: {string.Join(", ", missing)}. Extra: {string.Join(", ", extra)}.",
                string.Empty));
        }
    }

    private static HashSet<string> GetDeclaredTokens(string css)
    {
        return CustomPropertyRegex.Matches(css)
            .Select(match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool ContainsColorLiteral(string line, bool allowNamedColors)
    {
        var normalized = StripTokenReferences(line);
        return ContainsDirectColorValue(normalized, allowNamedColors)
            || ContainsGradientLiteral(normalized, allowNamedColors)
            || ContainsBoxShadowLiteral(normalized, allowNamedColors);
    }

    private static bool IsColorBearingValue(string value)
    {
        var normalized = StripTokenReferences(value);
        return ContainsDirectColorValue(normalized, allowNamedColors: true)
            || ContainsGradientLiteral(normalized, allowNamedColors: true)
            || ContainsBoxShadowLiteral(normalized, allowNamedColors: true);
    }

    private static bool ContainsDirectColorValue(string text, bool allowNamedColors)
    {
        if (HexColorRegex.IsMatch(text) || FunctionalColorRegex.IsMatch(text))
        {
            return true;
        }

        if (!allowNamedColors)
        {
            return false;
        }

        var withoutAllowedLiterals = AllowedLiteralRegex.Replace(text, string.Empty);
        return ContainsNamedColorLiteral(withoutAllowedLiterals);
    }

    private static bool ContainsNamedColorLiteral(string text)
    {
        foreach (Match match in NamedColorRegex.Matches(text))
        {
            var before = match.Index > 0 ? text[match.Index - 1] : '\0';
            var afterIndex = match.Index + match.Length;
            var after = afterIndex < text.Length ? text[afterIndex] : '\0';

            if (before is '.' or '/' or '_' || after is '.' or '/' or '_')
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool ContainsGradientLiteral(string text, bool allowNamedColors)
    {
        foreach (Match gradient in GradientRegex.Matches(text))
        {
            if (ContainsDirectColorValue(gradient.Value, allowNamedColors))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsBoxShadowLiteral(string text, bool allowNamedColors)
    {
        foreach (Match boxShadow in BoxShadowRegex.Matches(text))
        {
            if (ContainsDirectColorValue(boxShadow.Groups["value"].Value, allowNamedColors))
            {
                return true;
            }
        }

        return false;
    }

    private static string StripTokenReferences(string text)
    {
        return Regex.Replace(text, @"var\(--[a-z0-9-]+\)", "var()", RegexOptions.IgnoreCase);
    }

    private readonly record struct AuditCandidate(string Text, bool AllowNamedColors);
}

public sealed class TokenAuditReport
{
    public List<TokenFinding> ScopeViolations { get; } = [];
    public List<TokenFinding> LiteralViolations { get; } = [];
    public List<TokenFinding> AllowlistedLiterals { get; } = [];
    public List<TokenFinding> ParityViolations { get; } = [];
    public List<TokenFinding> ParityScans { get; } = [];

    public bool HasFailures => ScopeViolations.Count > 0 || LiteralViolations.Count > 0 || ParityViolations.Count > 0;

    public string SummaryLine() => $"scope={ScopeViolations.Count}, literals={LiteralViolations.Count}, parity={ParityViolations.Count}, allowlisted={AllowlistedLiterals.Count}";

    public string ToMarkdown(string repoRoot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Token Audit");
        builder.AppendLine();
        builder.AppendLine($"- Scope violations: {ScopeViolations.Count}");
        builder.AppendLine($"- Literal violations: {LiteralViolations.Count}");
        builder.AppendLine($"- Allowlisted literals: {AllowlistedLiterals.Count}");
        builder.AppendLine($"- Parity violations: {ParityViolations.Count}");
        builder.AppendLine();

        AppendSection(builder, repoRoot, "Scope Violations", ScopeViolations);
        AppendSection(builder, repoRoot, "Literal Violations", LiteralViolations);
        AppendSection(builder, repoRoot, "Allowlisted Literals", AllowlistedLiterals);
        AppendSection(builder, repoRoot, "Parity Violations", ParityViolations);

        return builder.ToString();
    }

    private static void AppendSection(StringBuilder builder, string repoRoot, string title, IReadOnlyCollection<TokenFinding> findings)
    {
        builder.AppendLine($"## {title}");
        if (findings.Count == 0)
        {
            builder.AppendLine("- None");
            builder.AppendLine();
            return;
        }

        foreach (var finding in findings)
        {
            builder.AppendLine($"- {Path.GetRelativePath(repoRoot, finding.FilePath)}:{finding.Line} - {finding.Message}");
        }

        builder.AppendLine();
    }
}

public sealed record TokenFinding(string Category, string FilePath, int Line, string Selector, string Message, string Snippet);

internal sealed class TokenAuditAllowlist
{
    private readonly List<TokenAuditAllowlistEntry> _entries;

    private TokenAuditAllowlist(List<TokenAuditAllowlistEntry> entries)
    {
        _entries = entries;
    }

    public static TokenAuditAllowlist Load(string repoRoot)
    {
        var allowlistPath = Path.Combine(repoRoot, "eng", "token-audit", "allowlist.txt");
        if (!File.Exists(allowlistPath))
        {
            return new TokenAuditAllowlist([]);
        }

        var entries = File.ReadAllLines(allowlistPath)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .Select(TokenAuditAllowlistEntry.Parse)
            .ToList();

        return new TokenAuditAllowlist(entries);
    }

    public bool IsAllowed(string normalizedFilePath, string lineText, string candidate)
    {
        return _entries.Any(entry => entry.Matches(normalizedFilePath, lineText, candidate));
    }
}

internal sealed record TokenAuditAllowlistEntry(string FilePattern, string Pattern, string Reason)
{
    public static TokenAuditAllowlistEntry Parse(string line)
    {
        var parts = line.Split('|', 3);
        if (parts.Length != 3)
        {
            throw new InvalidOperationException($"Allowlist entry must have 3 pipe-delimited parts (path|pattern|reason): {line}");
        }

        return new TokenAuditAllowlistEntry(parts[0], parts[1], parts[2]);
    }

    public bool Matches(string normalizedFilePath, string lineText, string candidate)
    {
        var filePattern = FilePattern.Replace('\\', '/');
        var fileMatch = normalizedFilePath.Contains(filePattern, StringComparison.OrdinalIgnoreCase);
        if (!fileMatch)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Pattern))
        {
            return true;
        }

        return Regex.IsMatch(candidate, Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
            || Regex.IsMatch(lineText, Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

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