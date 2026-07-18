using Agterhuis.Ui.Components.Layout;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtSidebarLayoutTests
{
    [Fact]
    public void RendersLogoAndContent()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtSidebarLayout>(p =>
            p.Add(x => x.Logo, builder => builder.AddContent(0, "Logo"))
             .Add(x => x.ChildContent, builder => builder.AddContent(0, "Inhoud")));

        Assert.Contains("Logo", cut.Markup);
        Assert.Contains("Inhoud", cut.Markup);
    }

    [Fact]
    public void RendersSidebarSlot()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtSidebarLayout>(p =>
            p.Add(x => x.Sidebar, builder => builder.AddContent(0, "Menu item")));

        Assert.Contains("Menu item", cut.Markup);
    }
}
