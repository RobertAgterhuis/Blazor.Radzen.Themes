using System.Text;
using System.Text.RegularExpressions;

namespace ExampleAudit;

public static class ExampleAuditEngine
{
    private const double SimilarityThreshold = 0.85;

    private static readonly Regex DemoExampleRegex = new("<DemoExample(?<attrs>[\\s\\S]*?)/>", RegexOptions.Compiled);
    private static readonly Regex AttributeRegex = new("(?<name>[A-Za-z0-9_]+)\\s*=\\s*\"(?<value>[^\"]*)\"", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new("\\s+", RegexOptions.Compiled);
    private static readonly Regex SingleLineCommentRegex = new("//.*?$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex MultiLineCommentRegex = new("/\\*[\\s\\S]*?\\*/", RegexOptions.Compiled);

    private static readonly (string Trigger, string KeywordRegex)[] TitleHeuristics =
    [
        ("validatie", "validator|validation|editcontext|messages|required"),
        ("disabled", "disabled\\s*=|isdisabled|enabled\\s*=\\s*\"false\"|enabled=\\\"false\\\""),
        ("template", "template"),
        ("sort", "sort|allowsorting"),
        ("filter", "filter|filtermode"),
        ("event", "on[a-z]+|change|click|select|open|close"),
        ("binding", "@bind|value\\s*=")
    ];

    private static readonly Regex SampleDataIdentifierRegex =
        new("\\b(?:items|item|records|record|rows|row|data|dataset|model|models|employees|orders|products|customers|values|series)\\w*\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ExampleAuditResult Run(string repoRoot)
    {
        var allowlist = ExampleAuditAllowlistStore.Load(repoRoot);
        var pagesRoot = Path.Combine(repoRoot, "samples", "Agterhuis.Ui.Demo", "Components", "Pages");
        var allPages = Directory.EnumerateFiles(pagesRoot, "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
                && !path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase));

        var pageResults = new List<PageAuditResult>();

        foreach (var page in allPages)
        {
            var text = File.ReadAllText(page);
            if (!text.Contains("<DemoExample", StringComparison.Ordinal))
            {
                continue;
            }

            var entries = ParseExamples(repoRoot, text);
            var relativePagePath = Path.GetRelativePath(repoRoot, page).Replace('\\', '/');
            var similarityViolations = BuildSimilarityViolations(relativePagePath, entries, allowlist);
            var titleMismatches = BuildTitleMismatches(relativePagePath, entries, allowlist);
            allowlist.SingleCapabilityPages.TryGetValue(relativePagePath, out var singleReason);

            pageResults.Add(new PageAuditResult(relativePagePath, entries.Count, similarityViolations, titleMismatches, singleReason));
        }

        pageResults.Sort((left, right) => string.CompareOrdinal(left.PagePath, right.PagePath));

        var unallowlistedSimilarity = pageResults
            .SelectMany(page => page.SimilarityViolations)
            .Count(violation => !violation.IsAllowlisted);

        var unallowlistedTitle = pageResults
            .SelectMany(page => page.TitleMismatches)
            .Count(violation => !violation.IsAllowlisted);

        return new ExampleAuditResult(pageResults, unallowlistedSimilarity, unallowlistedTitle);
    }

    public static string BuildMarkdownReport(ExampleAuditResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Example Audit");
        builder.AppendLine();
        builder.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        builder.AppendLine();
        builder.AppendLine($"Pages audited: {result.Pages.Count}");
        builder.AppendLine($"Unallowlisted similarity violations: {result.UnallowlistedSimilarityViolations}");
        builder.AppendLine($"Unallowlisted title/code mismatches: {result.UnallowlistedTitleMismatches}");
        builder.AppendLine();
        builder.AppendLine("| Pagina | Voorbeelden (n) | Gelijkenis-paren | Oordeel |");
        builder.AppendLine("|---|---:|---|---|");

        foreach (var page in result.Pages)
        {
            var pairCount = page.SimilarityViolations.Count;
            var hasBlocking = page.SimilarityViolations.Any(v => !v.IsAllowlisted) || page.TitleMismatches.Any(v => !v.IsAllowlisted);
            var judgement = hasBlocking
                ? "violatie"
                : page.SingleCapabilityReason is not null
                    ? $"single-capability: {EscapePipe(page.SingleCapabilityReason)}"
                    : "ok";

            builder.AppendLine($"| {EscapePipe(page.PagePath)} | {page.ExampleCount} | {pairCount} | {judgement} |");

            foreach (var pair in page.SimilarityViolations.OrderByDescending(v => v.Similarity))
            {
                var allowlistNote = pair.IsAllowlisted ? $" (allowlist: {EscapePipe(pair.AllowlistReason ?? "reason missing")})" : string.Empty;
                builder.AppendLine($"|  |  | {EscapePipe(pair.SourcePathA)} ~ {EscapePipe(pair.SourcePathB)} ({pair.Similarity:P1}){allowlistNote} |  |");
            }

            foreach (var mismatch in page.TitleMismatches)
            {
                var allowlistNote = mismatch.IsAllowlisted ? $" (allowlist: {EscapePipe(mismatch.AllowlistReason ?? "reason missing")})" : string.Empty;
                builder.AppendLine($"|  |  | title '{EscapePipe(mismatch.Title)}' mist keyword '{EscapePipe(mismatch.ExpectedKeyword)}' in {EscapePipe(mismatch.SourcePath)}{allowlistNote} |  |");
            }
        }

        return builder.ToString();
    }

    private static List<ExampleEntry> ParseExamples(string repoRoot, string pageContent)
    {
        var entries = new List<ExampleEntry>();
        var samplesRoot = Path.GetFullPath(Path.Combine(repoRoot, "samples", "Agterhuis.Ui.Demo"));

        foreach (Match match in DemoExampleRegex.Matches(pageContent))
        {
            var attrs = match.Groups["attrs"].Value;
            var title = GetAttribute(attrs, "Title") ?? "(untitled)";
            var sourcePath = GetAttribute(attrs, "SourcePath") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                continue;
            }

            var candidatePath = Path.Combine(samplesRoot, sourcePath.Replace('/', Path.DirectorySeparatorChar));
            var fullSourcePath = Path.GetFullPath(candidatePath);
            if (!fullSourcePath.StartsWith(samplesRoot, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var sourceCode = File.Exists(fullSourcePath)
                ? File.ReadAllText(fullSourcePath)
                : string.Empty;

            entries.Add(new ExampleEntry(title, sourcePath, sourceCode, NormalizeSource(sourceCode)));
        }

        return entries;
    }

    private static List<SimilarityViolation> BuildSimilarityViolations(string pagePath, IReadOnlyList<ExampleEntry> entries, ExampleAuditAllowlist allowlist)
    {
        var violations = new List<SimilarityViolation>();

        for (var leftIndex = 0; leftIndex < entries.Count; leftIndex++)
        {
            for (var rightIndex = leftIndex + 1; rightIndex < entries.Count; rightIndex++)
            {
                var left = entries[leftIndex];
                var right = entries[rightIndex];

                var similarity = CalculateSimilarity(left.NormalizedSource, right.NormalizedSource);
                if (similarity <= SimilarityThreshold)
                {
                    continue;
                }

                var allowlisted = TryGetAllowlistedPairReason(allowlist, pagePath, left.SourcePath, right.SourcePath, out var reason);
                violations.Add(new SimilarityViolation(left.SourcePath, right.SourcePath, similarity, allowlisted, reason));
            }
        }

        return violations;
    }

    private static List<TitleMismatchViolation> BuildTitleMismatches(string pagePath, IReadOnlyList<ExampleEntry> entries, ExampleAuditAllowlist allowlist)
    {
        var mismatches = new List<TitleMismatchViolation>();

        foreach (var entry in entries)
        {
            var lowerTitle = entry.Title.ToLowerInvariant();
            foreach (var heuristic in TitleHeuristics)
            {
                if (!lowerTitle.Contains(heuristic.Trigger, StringComparison.Ordinal))
                {
                    continue;
                }

                if (Regex.IsMatch(entry.SourceCode, heuristic.KeywordRegex, RegexOptions.IgnoreCase))
                {
                    continue;
                }

                var allowlisted = TryGetAllowlistedTitleMismatchReason(allowlist, pagePath, entry.Title, heuristic.Trigger, out var reason);
                mismatches.Add(new TitleMismatchViolation(entry.Title, entry.SourcePath, heuristic.Trigger, allowlisted, reason));
            }
        }

        return mismatches;
    }

    private static bool TryGetAllowlistedPairReason(ExampleAuditAllowlist allowlist, string pagePath, string sourcePathA, string sourcePathB, out string? reason)
    {
        foreach (var pair in allowlist.SimilarityPairs)
        {
            if (!string.Equals(pair.PagePath, pagePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var directMatch = string.Equals(pair.SourcePathA, sourcePathA, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pair.SourcePathB, sourcePathB, StringComparison.OrdinalIgnoreCase);
            var reverseMatch = string.Equals(pair.SourcePathA, sourcePathB, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pair.SourcePathB, sourcePathA, StringComparison.OrdinalIgnoreCase);

            if (!directMatch && !reverseMatch)
            {
                continue;
            }

            reason = pair.Reason;
            return true;
        }

        reason = null;
        return false;
    }

    private static bool TryGetAllowlistedTitleMismatchReason(ExampleAuditAllowlist allowlist, string pagePath, string title, string expectedKeyword, out string? reason)
    {
        foreach (var mismatch in allowlist.TitleMismatches)
        {
            if (string.Equals(mismatch.PagePath, pagePath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(mismatch.Title, title, StringComparison.OrdinalIgnoreCase)
                && string.Equals(mismatch.ExpectedKeyword, expectedKeyword, StringComparison.OrdinalIgnoreCase))
            {
                reason = mismatch.Reason;
                return true;
            }
        }

        reason = null;
        return false;
    }

    private static double CalculateSimilarity(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
        {
            return 1;
        }

        var leftLines = ExtractComparableLines(left);
        var rightLines = ExtractComparableLines(right);
        if (leftLines.Count == 0 || rightLines.Count == 0)
        {
            return 0;
        }

        var leftCounts = BuildTokenCounts(leftLines);
        var rightCounts = BuildTokenCounts(rightLines);

        var dotProduct = 0d;
        foreach (var (token, leftCount) in leftCounts)
        {
            if (rightCounts.TryGetValue(token, out var rightCount))
            {
                dotProduct += leftCount * rightCount;
            }
        }

        var leftNorm = Math.Sqrt(leftCounts.Values.Sum(value => value * value));
        var rightNorm = Math.Sqrt(rightCounts.Values.Sum(value => value * value));
        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0;
        }

        return dotProduct / (leftNorm * rightNorm);
    }

    private static Dictionary<string, int> BuildTokenCounts(IEnumerable<string> tokens)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in tokens)
        {
            counts[token] = counts.TryGetValue(token, out var count) ? count + 1 : 1;
        }

        return counts;
    }

    private static List<string> ExtractComparableLines(string source)
    {
        var lines = source
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => WhitespaceRegex.Replace(line.Trim(), " "))
            .Where(line => line.Length > 0)
            .ToList();

        return lines;
    }

    private static string NormalizeSource(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        var normalized = source.ToLowerInvariant();
        normalized = SingleLineCommentRegex.Replace(normalized, string.Empty);
        normalized = MultiLineCommentRegex.Replace(normalized, string.Empty);
        normalized = SampleDataIdentifierRegex.Replace(normalized, "sampledata");
        normalized = WhitespaceRegex.Replace(normalized, " ");
        return normalized.Trim();
    }

    private static string? GetAttribute(string attrs, string name)
    {
        foreach (Match match in AttributeRegex.Matches(attrs))
        {
            if (string.Equals(match.Groups["name"].Value, name, StringComparison.OrdinalIgnoreCase))
            {
                return match.Groups["value"].Value;
            }
        }

        return null;
    }

    private static string EscapePipe(string value)
    {
        return value.Replace("|", "\\|");
    }
}
