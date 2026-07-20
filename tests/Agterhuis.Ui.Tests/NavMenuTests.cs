using Agterhuis.Ui.Demo.Components.Layout;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using System.Text.RegularExpressions;

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

    [Fact]
    public void FilterInput_FiltersVisibleNavigationItems()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();

        var cut = ctx.Render<NavMenu>();

        cut.Find("#demo-nav-filter").Input("Buttons");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Buttons", cut.Markup);
            Assert.DoesNotContain("Voorbeeldapplicatie", cut.Markup);
        });
    }

    [Fact]
    public void Footer_RendersPackageVersion()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();

        var cut = ctx.Render<NavMenu>();

        var versionText = cut.Find(".demo-panel-nav__version").TextContent;
        Assert.StartsWith("Agterhuis.Ui v", versionText, StringComparison.Ordinal);
        Assert.DoesNotContain("@", versionText, StringComparison.Ordinal);

        var semverRegex = new Regex(@"^Agterhuis\.Ui v\d+\.\d+\.\d+(?:-[0-9A-Za-z\.-]+)?$", RegexOptions.CultureInvariant);
        Assert.Matches(semverRegex, versionText);
    }
}