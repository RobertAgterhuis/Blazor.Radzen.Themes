using ExampleAudit;

namespace Agterhuis.Ui.Tests;

public sealed class ExampleAuditTests
{
    [Fact]
    public void ExampleAudit_HasNoUnallowlistedViolations_AndWritesReport()
    {
        var repoRoot = RepositoryLayout.FindRepositoryRoot(AppContext.BaseDirectory);
        var result = ExampleAuditEngine.Run(repoRoot);
        var markdown = ExampleAuditEngine.BuildMarkdownReport(result);

        var reportPath = Path.Combine(repoRoot, "docs", "EXAMPLE-AUDIT.md");
        File.WriteAllText(reportPath, markdown);

        Assert.True(File.Exists(reportPath));
        Assert.True(result.IsSuccess,
            $"Example audit failed: similarity={result.UnallowlistedSimilarityViolations}, title={result.UnallowlistedTitleMismatches}. See docs/EXAMPLE-AUDIT.md");
    }
}
