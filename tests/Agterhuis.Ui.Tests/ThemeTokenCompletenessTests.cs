using System.Text.RegularExpressions;

namespace Agterhuis.Ui.Tests;

public sealed class ThemeTokenCompletenessTests
{
    private static readonly string[] RequiredThemeTokens =
    [
        "--agt-color-primary-50",
        "--agt-color-primary-100",
        "--agt-color-primary-200",
        "--agt-color-primary-300",
        "--agt-color-primary-400",
        "--agt-color-primary-500",
        "--agt-color-primary-600",
        "--agt-color-primary-700",
        "--agt-color-primary-800",
        "--agt-color-primary-900",
        "--agt-color-primary-950",
        "--agt-color-accent-300",
        "--agt-color-accent-400",
        "--agt-color-accent-500",
        "--agt-color-accent-600",
        "--agt-color-accent-700",
        "--agt-color-success",
        "--agt-color-warning",
        "--agt-color-danger",
        "--agt-color-info",
        "--agt-color-white",
        "--agt-color-gray-50",
        "--agt-color-gray-100",
        "--agt-color-gray-200",
        "--agt-color-gray-500",
        "--agt-color-gray-700",
        "--agt-color-gray-900",
        "--agt-surface-0",
        "--agt-surface-1",
        "--agt-surface-2",
        "--agt-surface-3",
        "--agt-glass-bg",
        "--agt-glass-border-color",
        "--agt-glass-border",
        "--agt-glow-primary",
        "--agt-glow-accent",
        "--agt-alpha-primary-4",
        "--agt-alpha-primary-5",
        "--agt-alpha-primary-10",
        "--agt-alpha-primary-20",
        "--agt-alpha-primary-40",
        "--agt-alpha-accent-5",
        "--agt-alpha-accent-10",
        "--agt-alpha-accent-20",
        "--agt-alpha-accent-40",
        "--agt-gradient-hero",
        "--agt-gradient-accent",
        "--agt-gradient-surface",
        "--agt-shadow-sm",
        "--agt-shadow-md",
        "--agt-font-display",
        "--agt-font-body",
        "--agt-heading-weight",
        "--agt-heading-tracking",
        "--agt-radius-scale",
        "--agt-canvas-backdrop",
        "--agt-hover-tint",
        "--agt-selection-tint",
        "--agt-focus-ring-style",
        "--agt-glow-strength",
        "--agt-press-scale",
        "--agt-lift-distance",
        "--agt-indicator-transition",
        "--agt-text-heading",
        "--agt-text-body",
        "--agt-text-muted",
        "--agt-link-color",
        "--agt-link-hover-color",
        "--agt-on-accent",
        "--agt-color-accent-text",
        "--agt-focus-ring",
        "--agt-text-disabled",
        "--agt-control-disabled-bg",
        "--agt-color-danger-text",
        "--agt-nav-group-text",
        "--agt-nav-item-text",
        "--agt-nav-item-icon",
        "--agt-nav-item-hover-bg",
        "--agt-nav-item-hover-text",
        "--agt-nav-item-active-bg",
        "--agt-nav-item-active-text",
        "--agt-nav-item-active-icon",
        "--agt-nav-focus-outline",
        "--agt-topbar-brand-color",
        "--agt-topbar-brand-accent",
        "--agt-topbar-icon-color",
        "--agt-topbar-icon-hover-color",
        "--agt-grid-header-bg",
        "--agt-grid-header-text",
        "--agt-grid-header-border",
        "--agt-grid-row-hover-bg",
        "--agt-grid-row-selected-bg",
        "--agt-grid-row-selected-text",
        "--agt-input-border",
        "--agt-input-focus-border",
        "--agt-input-focus-ring",
        "--agt-on-primary",
        "--agt-on-secondary",
        "--agt-on-base",
        "--agt-on-light",
        "--agt-on-dark",
        "--agt-on-info",
        "--agt-on-success",
        "--agt-on-warning",
        "--agt-on-danger",
        "--agt-btn-primary-fill",
        "--agt-btn-secondary-fill",
        "--agt-btn-base-fill",
        "--agt-btn-light-fill",
        "--agt-btn-dark-fill",
        "--agt-btn-info-fill",
        "--agt-btn-success-fill",
        "--agt-btn-warning-fill",
        "--agt-btn-danger-fill",
        "--agt-btn-light-ink",
        "--agt-btn-dark-ink",
        "--agt-btn-secondary-border",
        "--agt-btn-secondary-text",
        "--agt-btn-secondary-hover-bg",
        "--agt-btn-secondary-hover-text",
        "--agt-tooltip-text",
        "--agt-chart-gridline",
        "--agt-chart-axis-text",
        "--agt-chart-series-1",
        "--agt-chart-series-2",
        "--agt-chart-series-3",
        "--agt-chart-series-4",
        "--agt-chart-series-5",
        "--agt-chart-series-6",
        "--agt-chart-series-7",
        "--agt-chart-series-8"
    ];

    [Theory]
    [InlineData("agt-theme.plum.css")]
    [InlineData("agt-theme.ocean.css")]
    [InlineData("agt-theme.dagobah.css")]
    [InlineData("agt-theme.dathomir.css")]
    [InlineData("agt-theme.hoth.css")]
    [InlineData("agt-theme.tatooine.css")]
    [InlineData("agt-theme.imperial.css")]
    [InlineData("agt-theme.azure.css")]
    [InlineData("agt-theme.ms365.css")]
    [InlineData("agt-theme.volt.css")]
    [InlineData("agt-theme.autotaalglas.css")]
    [InlineData("agt-theme.autotaalglas-contrast.css")]
    [InlineData("agt-theme.autotaalglas-portal.css")]
    [InlineData("agt-theme.autotaalglas-mono.css")]
    public void ThemeVariantDeclaresFullTokenSet(string fileName)
    {
        var filePath = GetWorkspaceFilePath(Path.Combine("src", "Agterhuis.Ui", "wwwroot", "css", "themes", fileName));
        var css = File.ReadAllText(filePath);

        var declaredTokens = Regex.Matches(css, "--agt-[a-z0-9-]+(?=\\s*:)", RegexOptions.IgnoreCase)
            .Select(match => match.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = RequiredThemeTokens
            .Where(token => !declaredTokens.Contains(token))
            .ToArray();

        Assert.True(missing.Length == 0, $"Missing tokens in {fileName}: {string.Join(", ", missing)}");
    }

    [Theory]
    [InlineData("agt-theme.plum.css", "html[data-agt-theme=\"plum-dark\"]")]
    [InlineData("agt-theme.ocean.css", "html[data-agt-theme=\"ocean-dark\"]")]
    [InlineData("agt-theme.dagobah.css", "html[data-agt-theme=\"dagobah-dark\"]")]
    [InlineData("agt-theme.dathomir.css", "html[data-agt-theme=\"dathomir-dark\"]")]
    [InlineData("agt-theme.hoth.css", "html[data-agt-theme=\"hoth-dark\"]")]
    [InlineData("agt-theme.tatooine.css", "html[data-agt-theme=\"tatooine-dark\"]")]
    [InlineData("agt-theme.imperial.css", "html[data-agt-theme=\"imperial-dark\"]")]
    [InlineData("agt-theme.azure.css", "html[data-agt-theme=\"azure-dark\"]")]
    [InlineData("agt-theme.ms365.css", "html[data-agt-theme=\"ms365-dark\"]")]
    [InlineData("agt-theme.volt.css", "html[data-agt-theme=\"volt-dark\"]")]
    [InlineData("agt-theme.autotaalglas.css", "html[data-agt-theme=\"autotaalglas-dark\"]")]
    [InlineData("agt-theme.autotaalglas-contrast.css", "html[data-agt-theme=\"autotaalglas-contrast-dark\"]")]
    [InlineData("agt-theme.autotaalglas-portal.css", "html[data-agt-theme=\"autotaalglas-portal-dark\"]")]
    [InlineData("agt-theme.autotaalglas-mono.css", "html[data-agt-theme=\"autotaalglas-mono-dark\"]")]
    public void DarkVariantSelectorExists(string fileName, string selectorPrefix)
    {
        var filePath = GetWorkspaceFilePath(Path.Combine("src", "Agterhuis.Ui", "wwwroot", "css", "themes", fileName));
        var css = File.ReadAllText(filePath);

        Assert.Contains(selectorPrefix, css, StringComparison.Ordinal);
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