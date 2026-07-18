namespace Agterhuis.Ui.Tests;

public sealed class ShowcaseThemeProbeTests
{
    [Fact]
    public void RuntimeProbeSelectorsExistInShowcaseFiles()
    {
        var root = GetRepositoryRoot();
        var layoutMarkup = File.ReadAllText(Path.Combine(root, "samples", "Agterhuis.Ui.Demo", "Components", "Layout", "ShowcaseLayout.razor"));
        var planningMarkup = File.ReadAllText(Path.Combine(root, "samples", "Agterhuis.Ui.Demo", "Components", "Pages", "App", "ShowcasePlanning.razor"));
        var appCss = File.ReadAllText(Path.Combine(root, "samples", "Agterhuis.Ui.Demo", "wwwroot", "app.css"));

        Assert.Contains("showcase-topbar", layoutMarkup, StringComparison.Ordinal);
        Assert.Contains("showcase-planning__scheduler-surface", planningMarkup, StringComparison.Ordinal);
        Assert.Contains("showcase-notifications__panel", layoutMarkup, StringComparison.Ordinal);

        Assert.Contains(".showcase-topbar", appCss, StringComparison.Ordinal);
        Assert.Contains(".showcase-planning__scheduler-surface", appCss, StringComparison.Ordinal);
        Assert.Contains(".showcase-notifications__panel", appCss, StringComparison.Ordinal);
    }

    [Fact]
    public void ShowcaseLayoutDoesNotSetThemeOnInnerContainer()
    {
        var root = GetRepositoryRoot();
        var layoutMarkup = File.ReadAllText(Path.Combine(root, "samples", "Agterhuis.Ui.Demo", "Components", "Layout", "ShowcaseLayout.razor"));

        Assert.DoesNotContain("showcase-theme-root\" data-agt-theme=", layoutMarkup, StringComparison.OrdinalIgnoreCase);
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
