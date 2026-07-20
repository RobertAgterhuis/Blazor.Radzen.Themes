using System.Text;
using System.Text.RegularExpressions;

namespace ExampleAudit;

public static class ExampleAuditEngine
{
    private const double SimilarityThreshold = 0.85;

    private static readonly Regex DemoExampleRegex = new("<DemoExample(?<attrs>[\\s\\S]*?)/>", RegexOptions.Compiled);
    private static readonly Regex RouteDirectiveRegex = new("^@page\\s+\"(?<route>[^\"]+)\"", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex AttributeRegex = new("(?<name>[A-Za-z0-9_]+)\\s*=\\s*\"(?<value>[^\"]*)\"", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new("\\s+", RegexOptions.Compiled);
    private static readonly Regex SingleLineCommentRegex = new("//.*?$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex MultiLineCommentRegex = new("/\\*[\\s\\S]*?\\*/", RegexOptions.Compiled);
    private static readonly Regex ScanRowRegex = new(
        "^\\|\\s*(?<route>/[^|]*)\\s*\\|\\s*(?<file>[^|]+)\\|\\s*(?<index>\\d+)\\s*\\|\\s*(?<title>[^|]*)\\|\\s*(?<status>OK|LEEG|ERROR)\\s*\\|",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly string[] CategoryOrder =
    [
        "Forms",
        "Data",
        "Navigation",
        "Overlays",
        "Feedback",
        "Data Visualization",
        "Display",
        "Other"
    ];

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

    private static readonly string[] DataVisualizationKeywords =
    [
        "chart", "series", "gauge", "sparkline", "sankey", "treemap", "timeline", "waterfall", "radial", "arc-gauge", "linear-gauge"
    ];

    private static readonly string[] DataKeywords =
    [
        "data-grid", "grid", "data-list", "pager", "table", "scheduler", "tree", "pick-list", "data-filter", "gantt", "spreadsheet"
    ];

    private static readonly string[] FormsKeywords =
    [
        "form", "validator", "text", "textbox", "textarea", "autocomplete", "dropdown", "datepicker", "numeric", "password", "checkbox", "radio", "switch", "slider", "mask", "upload", "file-input", "select", "template-form"
    ];

    private static readonly string[] NavigationKeywords =
    [
        "menu", "tabs", "steps", "breadcrumb", "sidebar", "toc", "splitter", "panel-menu", "panel", "layout", "navigation"
    ];

    private static readonly string[] OverlayKeywords =
    [
        "dialog", "popup", "context-menu", "tooltip", "overlay", "drawer"
    ];

    private static readonly string[] FeedbackKeywords =
    [
        "alert", "notification", "progress", "skeleton", "message"
    ];

    private static readonly string[] DisplayKeywords =
    [
        "icon", "image", "badge", "chip", "link", "card", "avatar", "gravatar", "markdown", "login", "profile", "carousel", "timeline"
    ];

    public static ExampleAuditResult Run(string repoRoot)
    {
        var resolvedRepoRoot = Path.GetFullPath(repoRoot);
        if (!Directory.Exists(resolvedRepoRoot))
        {
            throw new DirectoryNotFoundException($"Repository root not found: {resolvedRepoRoot}");
        }

        var allowlist = ExampleAuditAllowlistStore.Load(resolvedRepoRoot);
        var scanByPage = LoadScanResults(resolvedRepoRoot, out var scanReportLoaded);
        var pagesRoot = Path.Combine(resolvedRepoRoot, "samples", "Agterhuis.Ui.Demo", "Components", "Pages");
        var allPages = Directory.EnumerateFiles(pagesRoot, "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
                && !path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase));

        var pageResults = new List<PageAuditResult>();

        foreach (var page in allPages)
        {
            var pageResult = BuildPageAuditResult(resolvedRepoRoot, page, allowlist, scanByPage);
            if (pageResult is not null)
            {
                pageResults.Add(pageResult);
            }
        }

        pageResults.Sort((left, right) => string.CompareOrdinal(left.PagePath, right.PagePath));

        var unallowlistedSimilarity = CountUnallowlistedSimilarity(pageResults);
        var unallowlistedTitle = CountUnallowlistedTitleMismatches(pageResults);

        var unallowlistedEmpty = pageResults.Sum(page => page.EmptyExamples);
        var unallowlistedError = pageResults.Sum(page => page.ErrorExamples);

        var categories = BuildCategorySummaries(pageResults);

        return new ExampleAuditResult(
            pageResults,
            unallowlistedSimilarity,
            unallowlistedTitle,
            unallowlistedEmpty,
            unallowlistedError,
            scanReportLoaded,
            categories);
    }

    private static PageAuditResult? BuildPageAuditResult(
        string repoRoot,
        string pagePath,
        ExampleAuditAllowlist allowlist,
        IReadOnlyDictionary<string, ScanPageResult> scanByPage)
    {
        var text = File.ReadAllText(pagePath);
        if (!text.Contains("<DemoExample", StringComparison.Ordinal))
        {
            return null;
        }

        var entries = ParseExamples(repoRoot, text);
        var relativePagePath = Path.GetRelativePath(repoRoot, pagePath).Replace('\\', '/');
        var route = GetPrimaryRoute(text);
        var category = ClassifyCategory(relativePagePath, route);
        var similarityViolations = BuildSimilarityViolations(relativePagePath, entries, allowlist);
        var titleMismatches = BuildTitleMismatches(relativePagePath, entries, allowlist);
        allowlist.SingleCapabilityPages.TryGetValue(relativePagePath, out var singleReason);
        scanByPage.TryGetValue(relativePagePath, out var scanPage);

        var emptyExamples = scanPage?.EmptyExamples ?? 0;
        var errorExamples = scanPage?.ErrorExamples ?? 0;

        return new PageAuditResult(
            relativePagePath,
            category,
            entries.Count,
            similarityViolations,
            titleMismatches,
            emptyExamples,
            errorExamples,
            singleReason);
    }

    private static int CountUnallowlistedSimilarity(IEnumerable<PageAuditResult> pages)
    {
        return pages
            .SelectMany(page => page.SimilarityViolations)
            .Count(violation => !violation.IsAllowlisted);
    }

    private static int CountUnallowlistedTitleMismatches(IEnumerable<PageAuditResult> pages)
    {
        return pages
            .SelectMany(page => page.TitleMismatches)
            .Count(violation => !violation.IsAllowlisted);
    }

    public static string BuildMarkdownReport(ExampleAuditResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Example Audit");
        builder.AppendLine();
        builder.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        builder.AppendLine();
        builder.AppendLine($"Pages audited: {result.Pages.Count}");
        builder.AppendLine($"Scan report loaded: {(result.ScanReportLoaded ? "yes" : "no")}");
        builder.AppendLine($"Unallowlisted similarity violations: {result.UnallowlistedSimilarityViolations}");
        builder.AppendLine($"Unallowlisted empty examples: {result.UnallowlistedEmptyViolations}");
        builder.AppendLine($"Unallowlisted error examples: {result.UnallowlistedErrorViolations}");
        builder.AppendLine($"Unallowlisted title/code mismatches: {result.UnallowlistedTitleMismatches}");
        builder.AppendLine();
        builder.AppendLine("| Pagina | Categorie | Voorbeelden (n) | Gelijkenis-paren | Leeg/error | Titel-mismatch | Oordeel |");
        builder.AppendLine("|---|---|---:|---:|---:|---:|---|");

        foreach (var page in result.Pages)
        {
            var pairCount = page.SimilarityViolations.Count;
            var titleMismatchCount = page.TitleMismatches.Count;
            var hasBlocking = page.SimilarityViolations.Any(v => !v.IsAllowlisted)
                || page.TitleMismatches.Any(v => !v.IsAllowlisted)
                || page.EmptyExamples > 0
                || page.ErrorExamples > 0;
            var judgement = hasBlocking
                ? "violatie"
                : page.SingleCapabilityReason is not null
                    ? $"single-capability: {EscapePipe(page.SingleCapabilityReason)}"
                    : "ok";

            builder.AppendLine($"| {EscapePipe(page.PagePath)} | {EscapePipe(page.Category)} | {page.ExampleCount} | {pairCount} | {page.EmptyExamples + page.ErrorExamples} | {titleMismatchCount} | {judgement} |");

            foreach (var pair in page.SimilarityViolations.OrderByDescending(v => v.Similarity))
            {
                var allowlistNote = pair.IsAllowlisted ? $" (allowlist: {EscapePipe(pair.AllowlistReason ?? "reason missing")})" : string.Empty;
                builder.AppendLine($"|  |  |  | {EscapePipe(pair.SourcePathA)} ~ {EscapePipe(pair.SourcePathB)} ({pair.Similarity:P1}){allowlistNote} |  |  |  |");
            }

            foreach (var mismatch in page.TitleMismatches)
            {
                var allowlistNote = mismatch.IsAllowlisted ? $" (allowlist: {EscapePipe(mismatch.AllowlistReason ?? "reason missing")})" : string.Empty;
                builder.AppendLine($"|  |  |  |  |  | title '{EscapePipe(mismatch.Title)}' mist keyword '{EscapePipe(mismatch.ExpectedKeyword)}' in {EscapePipe(mismatch.SourcePath)}{allowlistNote} |  |");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Category Summary");
        builder.AppendLine();
        builder.AppendLine("| Categorie | Pagina's | Gefixt/totaal | Gelijkenis | Leeg | Error | Titel-mismatch | Voortgang |");
        builder.AppendLine("|---|---:|---|---:|---:|---:|---:|---:|");

        foreach (var category in result.Categories)
        {
            var progress = category.TotalPages == 0
                ? "0%"
                : $"{(int)Math.Round(category.FixedPages * 100d / category.TotalPages, MidpointRounding.AwayFromZero)}%";
            builder.AppendLine($"| {EscapePipe(category.Category)} | {category.TotalPages} | {category.FixedPages}/{category.TotalPages} | {category.SimilarityViolations} | {category.EmptyExamples} | {category.ErrorExamples} | {category.TitleMismatches} | {progress} |");
        }

        return builder.ToString();
    }

    private static Dictionary<string, ScanPageResult> LoadScanResults(string repoRoot, out bool loaded)
    {
        var reportPath = Path.Combine(repoRoot, "docs", "EXAMPLE-SCAN.md");
        if (!File.Exists(reportPath))
        {
            loaded = false;
            return new Dictionary<string, ScanPageResult>(StringComparer.OrdinalIgnoreCase);
        }

        var text = File.ReadAllText(reportPath);
        var totals = new Dictionary<string, (int Empty, int Error)>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in ScanRowRegex.Matches(text))
        {
            var file = match.Groups["file"].Value.Trim().Replace('\\', '/');
            var status = match.Groups["status"].Value.Trim();
            if (!totals.TryGetValue(file, out var bucket))
            {
                bucket = (0, 0);
            }

            if (string.Equals(status, "LEEG", StringComparison.OrdinalIgnoreCase))
            {
                bucket.Empty++;
            }
            else if (string.Equals(status, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                bucket.Error++;
            }

            totals[file] = bucket;
        }

        loaded = totals.Count > 0;

        var result = new Dictionary<string, ScanPageResult>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, bucket) in totals)
        {
            result[path] = new ScanPageResult(bucket.Empty, bucket.Error);
        }

        return result;
    }

    private static string? GetPrimaryRoute(string pageContent)
    {
        foreach (Match match in RouteDirectiveRegex.Matches(pageContent))
        {
            var route = match.Groups["route"].Value.Trim();
            if (route.StartsWith("/catalog", StringComparison.OrdinalIgnoreCase)
                || route.StartsWith("/components", StringComparison.OrdinalIgnoreCase))
            {
                return route;
            }
        }

        return null;
    }

    private static string ClassifyCategory(string pagePath, string? route)
    {
        var target = route is null
            ? pagePath.ToLowerInvariant()
            : route.ToLowerInvariant();

        if (ContainsAny(target, DataVisualizationKeywords))
        {
            return "Data Visualization";
        }

        if (ContainsAny(target, DataKeywords))
        {
            return "Data";
        }

        if (ContainsAny(target, FormsKeywords))
        {
            return "Forms";
        }

        if (ContainsAny(target, NavigationKeywords))
        {
            return "Navigation";
        }

        if (ContainsAny(target, OverlayKeywords))
        {
            return "Overlays";
        }

        if (ContainsAny(target, FeedbackKeywords))
        {
            return "Feedback";
        }

        if (ContainsAny(target, DisplayKeywords))
        {
            return "Display";
        }

        return "Other";
    }

    private static bool ContainsAny(string target, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            if (target.Contains(key, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<CategoryAuditSummary> BuildCategorySummaries(IReadOnlyList<PageAuditResult> pages)
    {
        var categoryMap = new Dictionary<string, List<PageAuditResult>>(StringComparer.OrdinalIgnoreCase);
        foreach (var page in pages)
        {
            if (!categoryMap.TryGetValue(page.Category, out var list))
            {
                list = [];
                categoryMap[page.Category] = list;
            }

            list.Add(page);
        }

        var summaries = new List<CategoryAuditSummary>();
        foreach (var category in CategoryOrder)
        {
            categoryMap.TryGetValue(category, out var pagesInCategory);
            pagesInCategory ??= [];
            summaries.Add(CreateCategorySummary(category, pagesInCategory));
        }

        return summaries;
    }

    private static CategoryAuditSummary CreateCategorySummary(string category, IReadOnlyList<PageAuditResult> pages)
    {
        var similarity = pages
            .SelectMany(page => page.SimilarityViolations)
            .Count(violation => !violation.IsAllowlisted);

        var title = pages
            .SelectMany(page => page.TitleMismatches)
            .Count(violation => !violation.IsAllowlisted);

        var empty = pages.Sum(page => page.EmptyExamples);
        var error = pages.Sum(page => page.ErrorExamples);

        var fixedPages = pages.Count(page =>
            !page.SimilarityViolations.Any(violation => !violation.IsAllowlisted)
            && !page.TitleMismatches.Any(violation => !violation.IsAllowlisted)
            && page.EmptyExamples == 0
            && page.ErrorExamples == 0);

        return new CategoryAuditSummary(
            category,
            pages.Count,
            fixedPages,
            similarity,
            title,
            empty,
            error);
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
