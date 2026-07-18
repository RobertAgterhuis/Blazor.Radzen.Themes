using Agterhuis.Ui.Demo.Components.Layout;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class NavMenuTests
{
    [Fact]
    public void SectionHeaders_RenderAsPlainNonFocusableElements()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");

        var cut = ctx.Render<NavMenu>();

        cut.WaitForAssertion(() => Assert.Contains("Knoppen", cut.Markup));

        Assert.Empty(cut.FindAll(".rz-navigation-item.demo-panel-menu__section, .rz-navigation-item.demo-panel-menu__subtitle"));
        Assert.NotEmpty(cut.FindAll(".demo-panel-menu__section-title"));
        Assert.NotEmpty(cut.FindAll(".demo-panel-menu__subtitle"));
        Assert.Empty(cut.FindAll(".demo-panel-menu__section-title a, .demo-panel-menu__section-title button, .demo-panel-menu__subtitle a, .demo-panel-menu__subtitle button"));
        Assert.All(cut.FindAll(".demo-panel-menu__section-title, .demo-panel-menu__subtitle"), element =>
        {
            Assert.False(element.HasAttribute("href"));
            Assert.False(element.HasAttribute("tabindex"));
        });
    }
}