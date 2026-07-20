using TokenAudit;

namespace Agterhuis.Ui.Tests;

public sealed class TokenBleedAuditTests
{
    [Fact]
    public void ScopeAuditHasNoUnscopedColorTokens()
    {
        var report = TokenAuditEngine.Generate(GetRepositoryRoot());

        Assert.Empty(report.ScopeViolations);
    }

    [Fact]
    public void LiteralAuditHasNoUnallowlistedColorLiterals()
    {
        var report = TokenAuditEngine.Generate(GetRepositoryRoot());

        Assert.Empty(report.LiteralViolations);
    }

    [Fact]
    public void ThemeParityAuditFindsNoMismatchedScopes()
    {
        var report = TokenAuditEngine.Generate(GetRepositoryRoot());

        Assert.Empty(report.ParityViolations);
    }

    [Fact]
    public void LiteralAuditDetectsTemporaryFixtureWithHexLiteral()
    {
        var repoRoot = GetRepositoryRoot();
        var fixtureFileName = $"TokenAuditFixture_{Guid.NewGuid():N}.razor.css";
        var fixturePath = Path.Combine(repoRoot, "samples", "Agterhuis.Ui.Demo", "Components", "Pages", fixtureFileName);

        try
        {
            File.WriteAllText(fixturePath, ".token-audit-fixture { border-color: #abc; }");

            var report = TokenAuditEngine.Generate(repoRoot);

            Assert.Contains(report.LiteralViolations, finding =>
                string.Equals(Path.GetFullPath(finding.FilePath), Path.GetFullPath(fixturePath), StringComparison.OrdinalIgnoreCase)
                && finding.Snippet.Contains("#abc", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(fixturePath))
            {
                File.Delete(fixturePath);
            }
        }
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Agterhuis.Ui.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException($"Could not find repository root from '{AppContext.BaseDirectory}'.");
    }
}