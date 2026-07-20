namespace ExampleAudit;

public sealed record ExampleEntry(string Title, string SourcePath, string SourceCode, string NormalizedSource);

public sealed record PageAuditResult(
    string PagePath,
    string Category,
    int ExampleCount,
    IReadOnlyList<SimilarityViolation> SimilarityViolations,
    IReadOnlyList<TitleMismatchViolation> TitleMismatches,
    int EmptyExamples,
    int ErrorExamples,
    string? SingleCapabilityReason);

public sealed record SimilarityViolation(
    string SourcePathA,
    string SourcePathB,
    double Similarity,
    bool IsAllowlisted,
    string? AllowlistReason);

public sealed record TitleMismatchViolation(
    string Title,
    string SourcePath,
    string ExpectedKeyword,
    bool IsAllowlisted,
    string? AllowlistReason);

public sealed record ExampleAuditResult(
    IReadOnlyList<PageAuditResult> Pages,
    int UnallowlistedSimilarityViolations,
    int UnallowlistedTitleMismatches,
    int UnallowlistedEmptyViolations,
    int UnallowlistedErrorViolations,
    bool ScanReportLoaded,
    IReadOnlyList<CategoryAuditSummary> Categories)
{
    public bool IsSuccess => UnallowlistedSimilarityViolations == 0
        && UnallowlistedTitleMismatches == 0
        && UnallowlistedEmptyViolations == 0
        && UnallowlistedErrorViolations == 0;
}

public sealed record CategoryAuditSummary(
    string Category,
    int TotalPages,
    int FixedPages,
    int SimilarityViolations,
    int TitleMismatches,
    int EmptyExamples,
    int ErrorExamples);

public sealed record ScanPageResult(int EmptyExamples, int ErrorExamples);

public sealed class ExampleAuditAllowlist
{
    public Dictionary<string, string> SingleCapabilityPages { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<AllowlistedPair> SimilarityPairs { get; init; } = [];
    public List<AllowlistedTitleMismatch> TitleMismatches { get; init; } = [];
}

public sealed record AllowlistedPair(string PagePath, string SourcePathA, string SourcePathB, string Reason);

public sealed record AllowlistedTitleMismatch(string PagePath, string Title, string ExpectedKeyword, string Reason);
