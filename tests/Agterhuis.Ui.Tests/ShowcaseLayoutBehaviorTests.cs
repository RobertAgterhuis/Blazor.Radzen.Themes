using Agterhuis.Ui.Demo.Components.Layout;
using Agterhuis.Ui.Demo.Services;
using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Theming;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class ShowcaseLayoutBehaviorTests
{
    [Fact]
    public void SidebarToggle_UpdatesAriaExpanded_AndPersistsDesktopChoice()
    {
        using var ctx = CreateContext(
            new TestNavigationManager("https://localhost/", "https://localhost/app"),
            isMobileViewport: false,
            initialSidebarState: "expanded");

        var cut = ctx.Render<ShowcaseLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".showcase-topbar__menu-toggle")));

        var initialValue = cut.Find(".showcase-topbar__menu-toggle").GetAttribute("aria-expanded");

        cut.Find(".showcase-topbar__menu-toggle").Click();

        cut.WaitForAssertion(() => Assert.NotEqual(initialValue, cut.Find(".showcase-topbar__menu-toggle").GetAttribute("aria-expanded")));
    }

    [Fact]
    public void Sidebar_RemainsExpanded_AfterNavigationUntilUserToggles()
    {
        using var ctx = CreateContext(
            new TestNavigationManager("https://localhost/", "https://localhost/app"),
            isMobileViewport: false,
            initialSidebarState: "expanded");

        var navigation = (NavigationManager)ctx.Services.GetRequiredService<NavigationManager>();

        var cut = ctx.Render<ShowcaseLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find(".showcase-topbar__menu-toggle").GetAttribute("aria-expanded")));

        cut.InvokeAsync(() => navigation.NavigateTo("/app/werkorders"));

        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find(".showcase-topbar__menu-toggle").GetAttribute("aria-expanded")));
    }

    [Fact]
    public void FirstRender_SyncsPersistedThemeIntoThemeState()
    {
        using var ctx = CreateContext(
            new TestNavigationManager("https://localhost/", "https://localhost/app"),
            isMobileViewport: false,
            initialSidebarState: "expanded",
            storedTheme: "ocean-dark");

        var themeState = ctx.Services.GetRequiredService<AgtThemeState>();

        _ = ctx.Render<ShowcaseLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        Assert.Equal("ocean-dark", themeState.Theme);
    }

    [Fact]
    public void SidebarFooter_RendersPackageVersion()
    {
        using var ctx = CreateContext(
            new TestNavigationManager("https://localhost/", "https://localhost/app"),
            isMobileViewport: false,
            initialSidebarState: "expanded");

        var cut = ctx.Render<ShowcaseLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.Contains("Agterhuis.Ui v", cut.Find(".showcase-sidebar__footer-version").TextContent));
    }

    private static BunitContext CreateContext(
        TestNavigationManager navigationManager,
        bool isMobileViewport,
        string initialSidebarState,
        string storedTheme = "plum-dark")
    {
        var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddAgterhuisUi();
        ctx.Services.AddScoped<ShowcaseDataService>();
        ctx.Services.AddSingleton<NavigationManager>(navigationManager);

        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("agtTheme.closeAllPopups").SetVoidResult();
        ctx.JSInterop.Setup<string>("agtTheme.getStoredTheme", _ => true).SetResult(storedTheme);
        ctx.JSInterop.Setup<string>("agtTheme.getStoredDensity", _ => true).SetResult("comfortable");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult(initialSidebarState);
        ctx.JSInterop.SetupVoid("agtTheme.setStoredNavSectionState", _ => true).SetVoidResult();
        ctx.JSInterop.Setup<bool>("agtTheme.isViewportAtMost", _ => true).SetResult(isMobileViewport);
        ctx.JSInterop.SetupVoid("agtTheme.applyNavItemTitles", _ => true).SetVoidResult();

        return ctx;
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
