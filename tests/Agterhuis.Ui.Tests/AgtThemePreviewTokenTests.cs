using Agterhuis.Ui.Theming;
using System.Text.RegularExpressions;

namespace Agterhuis.Ui.Tests;

public sealed class AgtThemePreviewTokenTests
{
    private static readonly Regex HexColorRegex = new("^#(?:[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);

    [Fact]
    public void EveryThemeDeclaresPreviewPalette()
    {
        foreach (var theme in AgtTheme.All)
        {
            Assert.Matches(HexColorRegex, theme.PreviewCanvas);
            Assert.Matches(HexColorRegex, theme.PreviewPrimary);
            Assert.Matches(HexColorRegex, theme.PreviewAccent);
        }
    }

    [Fact]
    public void PreviewPaletteMatchesThemeLightTokens()
    {
        foreach (var theme in AgtTheme.All)
        {
            var cssPath = GetWorkspaceFilePath(Path.Combine("src", "Agterhuis.Ui", "wwwroot", "css", "themes", $"agt-theme.{theme.Name}.css"));
            var css = File.ReadAllText(cssPath);
            var selector = $"html[data-agt-theme=\"{theme.LightVariantId}\"]";

            var canvas = GetTokenValue(css, selector, "--agt-surface-0");
            var primary = GetTokenValue(css, selector, "--agt-color-primary-500");
            var accent = GetTokenValue(css, selector, "--agt-color-accent-400");

            Assert.Equal(canvas, theme.PreviewCanvas, ignoreCase: true);
            Assert.Equal(primary, theme.PreviewPrimary, ignoreCase: true);
            Assert.Equal(accent, theme.PreviewAccent, ignoreCase: true);
        }
    }

    private static string GetTokenValue(string css, string selector, string token)
    {
        var blockPattern = $"{Regex.Escape(selector)}\\s*\\{{(?<body>[\\s\\S]*?)\\n\\}}";
        var blockMatch = Regex.Match(css, blockPattern, RegexOptions.CultureInvariant);
        Assert.True(blockMatch.Success, $"Could not find selector '{selector}'.");

        var body = blockMatch.Groups["body"].Value;
        var tokenPattern = $"{Regex.Escape(token)}\\s*:\\s*(?<value>[^;]+);";
        var tokenMatch = Regex.Match(body, tokenPattern, RegexOptions.CultureInvariant);
        Assert.True(tokenMatch.Success, $"Could not find token '{token}' in '{selector}'.");

        return tokenMatch.Groups["value"].Value.Trim();
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
