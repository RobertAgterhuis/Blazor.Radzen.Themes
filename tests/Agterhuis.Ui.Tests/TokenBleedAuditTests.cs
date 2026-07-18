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