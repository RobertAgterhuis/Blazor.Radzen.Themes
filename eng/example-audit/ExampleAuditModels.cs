namespace ExampleAudit;

public sealed record ExampleEntry(string Title, string SourcePath, string SourceCode, string NormalizedSource);

public sealed record PageAuditResult(
    string PagePath,
    int ExampleCount,
    IReadOnlyList<SimilarityViolation> SimilarityViolations,
    IReadOnlyList<TitleMismatchViolation> TitleMismatches,
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
    int UnallowlistedTitleMismatches)
{
    public bool IsSuccess => UnallowlistedSimilarityViolations == 0 && UnallowlistedTitleMismatches == 0;
}

public sealed class ExampleAuditAllowlist
{
    public Dictionary<string, string> SingleCapabilityPages { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<AllowlistedPair> SimilarityPairs { get; init; } = [];
    public List<AllowlistedTitleMismatch> TitleMismatches { get; init; } = [];
}

public sealed record AllowlistedPair(string PagePath, string SourcePathA, string SourcePathB, string Reason);

public sealed record AllowlistedTitleMismatch(string PagePath, string Title, string ExpectedKeyword, string Reason);
