using Agterhuis.Ui.Demo.Components.Layout;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
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
        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();

        var cut = ctx.Render<NavMenu>();

        cut.Find("#demo-nav-filter").Input("AutoComplete");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("AutoComplete", cut.Markup);
            Assert.DoesNotContain("Voorbeeldapplicatie", cut.Markup);
        });
    }

    [Fact]
    public void CatalogCategories_RenderWithoutRadzenSuffix_AndNoChildPages()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();

        ctx.Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost/", "https://localhost/catalog"));

        var cut = ctx.Render<NavMenu>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Radzen catalogus · QA", cut.Markup);
            Assert.Contains("DataGrid", cut.Markup);
            Assert.Contains("AutoComplete", cut.Markup);
            Assert.DoesNotContain("(Radzen)", cut.Markup);
            Assert.DoesNotContain("~O__TH@der", cut.Markup);
            Assert.DoesNotContain("/catalog/body", cut.Markup);
            Assert.DoesNotContain("/catalog/splitter-pane", cut.Markup);
            Assert.DoesNotContain("/catalog/data-filter-item", cut.Markup);
        });
    }

    [Fact]
    public void FilterInput_OpensMatchingCatalogCategoryAndHidesOthers()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();
        ctx.Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost/", "https://localhost/catalog"));

        var cut = ctx.Render<NavMenu>();
        cut.Find("#demo-nav-filter").Input("AutoComplete");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Forms", cut.Markup);
            Assert.Contains("AutoComplete", cut.Markup);
            Assert.DoesNotContain("Scheduler", cut.Markup);
        });
    }

    [Fact]
    public void CatalogCategories_RestoreExpandStateFromStorage()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();
        ctx.Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost/", "https://localhost/catalog"));

        _ = ctx.Render<NavMenu>();

        Assert.Contains(ctx.JSInterop.Invocations, invocation =>
            invocation.Identifier == "agtTheme.getStoredValue"
            && invocation.Arguments.Count >= 2
            && string.Equals(invocation.Arguments[0]?.ToString(), "agt-ui-catalog-category-forms", StringComparison.Ordinal)
            && string.Equals(invocation.Arguments[1]?.ToString(), "collapsed", StringComparison.Ordinal));
    }

    [Fact]
    public void CatalogItemLabels_MatchValidNamePattern()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();
        ctx.Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost/", "https://localhost/catalog"));

        var cut = ctx.Render<NavMenu>();
        var labelRegex = new Regex(@"^[\p{L}\p{Nd}][\p{L}\p{Nd}\s\-\&\+\./]*$", RegexOptions.CultureInvariant);

        var labels = cut
            .FindAll(".demo-panel-menu__catalog-category .rz-navigation-item-text")
            .Select(node => node.TextContent.Trim())
            .Where(label => !string.IsNullOrWhiteSpace(label));

        Assert.All(labels, label => Assert.Matches(labelRegex, label));
    }

    [Fact]
    public void Footer_RendersPackageVersion()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("collapsed");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", _ => true).SetResult("collapsed");
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();

        var cut = ctx.Render<NavMenu>();

        var versionText = cut.Find(".demo-panel-nav__version").TextContent;
        Assert.StartsWith("Agterhuis.Ui v", versionText, StringComparison.Ordinal);
        Assert.DoesNotContain("@", versionText, StringComparison.Ordinal);

        var semverRegex = new Regex(@"^Agterhuis\.Ui v\d+\.\d+\.\d+(?:-[0-9A-Za-z\.-]+)?$", RegexOptions.CultureInvariant);
        Assert.Matches(semverRegex, versionText);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string baseUri, string uri)
        {
            Initialize(baseUri, uri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            var absoluteUri = ToAbsoluteUri(uri).ToString();
            Uri = absoluteUri;
            NotifyLocationChanged(false);
        }
    }
}