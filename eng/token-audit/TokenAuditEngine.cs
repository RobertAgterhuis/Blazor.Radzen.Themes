using System.Text;
using System.Text.RegularExpressions;

namespace TokenAudit;

public static class TokenAuditEngine
{
    private static readonly Regex CustomPropertyRegex = new(@"(?<name>--[a-z0-9-]+)\s*:\s*(?<value>[^;{}]+);", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HexColorRegex = new(@"(?<![A-Za-z0-9_])#(?:[0-9a-fA-F]{3,8})\b", RegexOptions.Compiled);
    private static readonly Regex RgbColorRegex = new(@"\b(?:rgb|rgba|hsl|hsla|oklch|oklab)\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex NamedColorRegex = new(@"(?<![A-Za-z0-9_-])(black|white|red|green|blue|yellow|orange|purple|pink|gray|grey|silver|maroon|navy|teal|olive|lime|aqua|fuchsia)\b(?!-)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AllowedLiteralRegex = new(@"transparent|currentColor|inherit|none", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] ThemeFileNames =
    [
        "agt-theme.plum.css",
        "agt-theme.ocean.css",
        "agt-theme.dagobah.css",
        "agt-theme.dathomir.css",
        "agt-theme.hoth.css",
        "agt-theme.tatooine.css",
        "agt-theme.imperial.css",
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
                if (extension is ".css" or ".razor" or ".cs")
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
            || normalized.EndsWith(".styles.css", StringComparison.OrdinalIgnoreCase);
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
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (!ContainsColorLiteral(line))
            {
                continue;
            }

            if (allowlist.IsAllowed(normalized, index + 1, line))
            {
                report.AllowlistedLiterals.Add(new TokenFinding("allowlist", filePath, index + 1, string.Empty, "Allowed literal.", line.Trim()));
                continue;
            }

            report.LiteralViolations.Add(new TokenFinding(
                "literal",
                filePath,
                index + 1,
                string.Empty,
                "Raw color literal found outside theme token files.",
                line.Trim()));
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

    private static bool ContainsColorLiteral(string line)
    {
        var normalized = StripTokenReferences(line);

        if (AllowedLiteralRegex.IsMatch(normalized) && !HexColorRegex.IsMatch(normalized) && !RgbColorRegex.IsMatch(normalized))
        {
            return false;
        }

        return HexColorRegex.IsMatch(normalized) || RgbColorRegex.IsMatch(normalized) || NamedColorRegex.IsMatch(normalized);
    }

    private static bool IsColorBearingValue(string value)
    {
        var normalized = StripTokenReferences(value);

        if (AllowedLiteralRegex.IsMatch(normalized) && !HexColorRegex.IsMatch(normalized) && !RgbColorRegex.IsMatch(normalized))
        {
            return false;
        }

        return HexColorRegex.IsMatch(normalized) || RgbColorRegex.IsMatch(normalized) || NamedColorRegex.IsMatch(normalized);
    }

    private static string StripTokenReferences(string text)
    {
        return Regex.Replace(text, @"var\(--(?:agt|rz)-[^)]+\)", "var()", RegexOptions.IgnoreCase);
    }
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

    public bool IsAllowed(string normalizedFilePath, int lineNumber, string lineText)
    {
        return _entries.Any(entry => entry.Matches(normalizedFilePath, lineNumber, lineText));
    }
}

internal sealed record TokenAuditAllowlistEntry(string FilePattern, int? LineNumber, string Snippet, string Reason)
{
    public static TokenAuditAllowlistEntry Parse(string line)
    {
        var parts = line.Split('|', 4);
        if (parts.Length != 4)
        {
            throw new InvalidOperationException($"Allowlist entry must have 4 pipe-delimited parts: {line}");
        }

        return new TokenAuditAllowlistEntry(parts[0], string.IsNullOrWhiteSpace(parts[1]) ? null : int.Parse(parts[1]), parts[2], parts[3]);
    }

    public bool Matches(string normalizedFilePath, int lineNumber, string lineText)
    {
        var filePattern = FilePattern.Replace('\\', '/');
        var fileMatch = normalizedFilePath.Contains(filePattern, StringComparison.OrdinalIgnoreCase);
        var lineMatch = LineNumber is null || LineNumber == lineNumber;
        var snippetMatch = string.IsNullOrWhiteSpace(Snippet) || lineText.Contains(Snippet, StringComparison.OrdinalIgnoreCase);
        return fileMatch && lineMatch && snippetMatch;
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