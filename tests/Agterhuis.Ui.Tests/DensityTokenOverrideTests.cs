using System.Text.RegularExpressions;

namespace Agterhuis.Ui.Tests;

public sealed class DensityTokenOverrideTests
{
    private static readonly string[] CompactOverrideTokens =
    [
        "--agt-font-size-xs",
        "--agt-font-size-sm",
        "--agt-font-size-md",
        "--agt-font-size-lg",
        "--agt-font-size-xl",
        "--agt-spacing-1",
        "--agt-spacing-2",
        "--agt-spacing-3",
        "--agt-spacing-4",
        "--agt-spacing-6",
        "--agt-spacing-8",
        "--agt-control-height-xs",
        "--agt-control-height-sm",
        "--agt-control-height-md",
        "--agt-control-height-lg",
        "--agt-control-padding-inline",
        "--agt-card-padding",
        "--agt-grid-cell-padding-block",
        "--agt-grid-cell-padding-inline",
        "--agt-heading-size-xl",
        "--agt-heading-size-lg"
    ];

    [Fact]
    public void CompactDensityOverridesExpectedStructuralTokenSet()
    {
        var cssPath = GetWorkspaceFilePath(Path.Combine("src", "Agterhuis.Ui", "wwwroot", "css", "agt-tokens.css"));
        var css = File.ReadAllText(cssPath);
        var match = Regex.Match(css, "html\\[data-agt-density=\"compact\"\\]\\s*\\{(?<body>[\\s\\S]*?)\\}", RegexOptions.Multiline);

        Assert.True(match.Success, "Compact density override block was not found.");

        var declaredTokens = Regex.Matches(match.Groups["body"].Value, "--agt-[a-z0-9-]+(?=\\s*:)", RegexOptions.IgnoreCase)
            .Select(value => value.Value)
            .ToArray();

        Assert.Equal(CompactOverrideTokens, declaredTokens);
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