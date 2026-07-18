using System.Text.RegularExpressions;

namespace Agterhuis.Ui.Tests;

public sealed class ThemeTokenParityTests
{
    public static IEnumerable<object[]> ThemeScopes =>
    [
        ["agt-theme.plum.css"],
        ["agt-theme.ocean.css"],
        ["agt-theme.dagobah.css"],
        ["agt-theme.dathomir.css"],
        ["agt-theme.hoth.css"],
        ["agt-theme.tatooine.css"],
        ["agt-theme.imperial.css"],
        ["agt-theme.autotaalglas.css"],
        ["agt-theme.autotaalglas-contrast.css"],
        ["agt-theme.autotaalglas-portal.css"],
        ["agt-theme.autotaalglas-mono.css"]
    ];

    [Theory]
    [MemberData(nameof(ThemeScopes))]
    public void ThemeFileMatchesPlumTokenSet(string fileName)
    {
        var themeDir = Path.Combine("src", "Agterhuis.Ui", "wwwroot", "css", "themes");
        var referenceTokens = GetTokens(GetWorkspaceFilePath(Path.Combine(themeDir, "agt-theme.plum.css")));
        var themeTokens = GetTokens(GetWorkspaceFilePath(Path.Combine(themeDir, fileName)));

        var missing = referenceTokens
            .Where(token => !themeTokens.Contains(token))
            .ToArray();

        Assert.True(missing.Length == 0, $"Missing tokens in {fileName}: {string.Join(", ", missing)}");
    }

    [Fact]
    public void SourceThemeVariablesKeepIdlePanelMenuTransparentAndDropdownSelectionsReadable()
    {
        var variablesPath = GetWorkspaceFilePath(Path.Combine("src", "Agterhuis.Ui", "wwwroot", "css", "theme", "_variables.css"));
        var css = File.ReadAllText(variablesPath);

        Assert.Contains("--rz-panel-menu-item-background-color: transparent;", css);
        Assert.Contains("--rz-dropdown-item-selected-color: var(--agt-on-accent);", css);
        Assert.Contains("--rz-dropdown-open-background-color: var(--agt-surface-1);", css);
    }

    [Theory]
    [InlineData("agt-theme.plum.css", "html[data-agt-theme=\"plum-light\"]")]
    [InlineData("agt-theme.ocean.css", "html[data-agt-theme=\"ocean-light\"]")]
    [InlineData("agt-theme.dagobah.css", "html[data-agt-theme=\"dagobah-light\"]")]
    [InlineData("agt-theme.dathomir.css", "html[data-agt-theme=\"dathomir-light\"]")]
    [InlineData("agt-theme.hoth.css", "html[data-agt-theme=\"hoth-light\"]")]
    [InlineData("agt-theme.tatooine.css", "html[data-agt-theme=\"tatooine-light\"]")]
    [InlineData("agt-theme.imperial.css", "html[data-agt-theme=\"imperial-light\"]")]
    [InlineData("agt-theme.autotaalglas.css", "html[data-agt-theme=\"autotaalglas-light\"]")]
    [InlineData("agt-theme.autotaalglas-contrast.css", "html[data-agt-theme=\"autotaalglas-contrast-light\"]")]
    [InlineData("agt-theme.autotaalglas-portal.css", "html[data-agt-theme=\"autotaalglas-portal-light\"]")]
    [InlineData("agt-theme.autotaalglas-mono.css", "html[data-agt-theme=\"autotaalglas-mono-light\"]")]
    public void ThemeAssetsIncludeHeaderSurfaceToken(string fileName, string selectorPrefix)
    {
        var themeDir = Path.Combine("src", "Agterhuis.Ui", "wwwroot", "css", "themes");
        var css = File.ReadAllText(GetWorkspaceFilePath(Path.Combine(themeDir, fileName)));

        Assert.Contains(selectorPrefix, css, StringComparison.Ordinal);
        Assert.Contains("--agt-gradient-surface:", css, StringComparison.Ordinal);
    }

    private static HashSet<string> GetTokens(string filePath)
    {
        var css = File.ReadAllText(filePath);

        return Regex.Matches(css, "--agt-[a-z0-9-]+(?=\\s*:)", RegexOptions.IgnoreCase)
            .Select(match => match.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string GetWorkspaceFilePath(string relativePath)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Could not find '{relativePath}' from '{AppContext.BaseDirectory}'.");
    }
}