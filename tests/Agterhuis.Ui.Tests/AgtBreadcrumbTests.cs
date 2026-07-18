using Agterhuis.Ui.Components.Layout;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtBreadcrumbTests
{
    [Fact]
    public void RendersNavigationWithAriaLabel()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var items = new List<(string Text, string? Href)> { ("Home", "/"), ("Detail", null) };
        var cut = ctx.Render<AgtBreadcrumb>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.AriaLabel, "Padnavigatie"));

        Assert.Contains("aria-label=\"Padnavigatie\"", cut.Markup);
    }

    [Fact]
    public void RendersAllCrumbs()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var items = new List<(string Text, string? Href)> { ("Home", "/"), ("Catalog", "/catalog"), ("Nu", null) };
        var cut = ctx.Render<AgtBreadcrumb>(p => p.Add(x => x.Items, items));

        Assert.Contains("Home", cut.Markup);
        Assert.Contains("Catalog", cut.Markup);
        Assert.Contains("Nu", cut.Markup);
    }
}
