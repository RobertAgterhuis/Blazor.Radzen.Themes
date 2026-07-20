using Agterhuis.Ui.Demo.Components.Layout;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Theming;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class MainLayoutSignatureTests
{
    [Fact]
    public void SetsReducedMotionHook_WhenPrefersReducedMotionIsTrue()
    {
        using var ctx = CreateContext(new TestNavigationManager("https://localhost/", "https://localhost/components/buttons"), false, true);

        var cut = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.Contains("data-agt-motion=\"reduced\"", cut.Markup));
        Assert.DoesNotContain("demo-shell-ambient", cut.Markup);
    }

    [Fact]
    public void RemovesAmbientOnDataDenseRoutes()
    {
        using var ctx = CreateContext(new TestNavigationManager("https://localhost/", "https://localhost/components/data/grid"), true, false);

        var cut = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.Contains("data-agt-ambient=\"calm\"", cut.Markup));
        Assert.DoesNotContain("demo-shell-ambient", cut.Markup);
    }

    [Fact]
    public void ShowsWrapperToCatalogCrossLink_OnAgtButtonsPage()
    {
        using var ctx = CreateContext(new TestNavigationManager("https://localhost/", "https://localhost/components/buttons"), true, false);

        var cut = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.Contains("/catalog/buttons", cut.Markup));
        Assert.Contains("Bekijk de rauwe Radzen-variant in het theme", cut.Markup);
    }

    [Fact]
    public void ShowsCatalogToWrapperCrossLink_OnRadzenLayoutPage()
    {
        using var ctx = CreateContext(new TestNavigationManager("https://localhost/", "https://localhost/catalog/layout"), true, false);

        var cut = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.Contains("/components/layout", cut.Markup));
        Assert.Contains("Voor gebruik in applicaties: zie de Agt-wrappers", cut.Markup);
    }

    [Fact]
    public void SidebarToggle_UpdatesAriaExpanded()
    {
        using var ctx = CreateContext(
            new TestNavigationManager("https://localhost/", "https://localhost/components/buttons"),
            enableAmbientEffects: true,
            prefersReducedMotion: false,
            isMobileViewport: false,
            initialSidebarState: "collapsed");

        var cut = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".demo-topbar__menu-toggle")));

        var initialValue = cut.Find(".demo-topbar__menu-toggle").GetAttribute("aria-expanded");

        cut.Find(".demo-topbar__menu-toggle").Click();

        cut.WaitForAssertion(() => Assert.NotEqual(initialValue, cut.Find(".demo-topbar__menu-toggle").GetAttribute("aria-expanded")));
    }

    [Fact]
    public void Sidebar_RemainsExpanded_AfterNavigationUntilUserToggles()
    {
        using var ctx = CreateContext(
            new TestNavigationManager("https://localhost/", "https://localhost/components/buttons"),
            enableAmbientEffects: true,
            prefersReducedMotion: false,
            isMobileViewport: false,
            initialSidebarState: "expanded");

        var navigation = (NavigationManager)ctx.Services.GetRequiredService<NavigationManager>();

        var cut = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find(".demo-topbar__menu-toggle").GetAttribute("aria-expanded")));

        cut.InvokeAsync(() => navigation.NavigateTo("/components/forms/checkbox"));

        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find(".demo-topbar__menu-toggle").GetAttribute("aria-expanded")));
    }

    [Fact]
    public void FirstRender_SyncsPersistedThemeIntoThemeState()
    {
        using var ctx = CreateContext(
            new TestNavigationManager("https://localhost/", "https://localhost/components/forms/radio-list"),
            enableAmbientEffects: true,
            prefersReducedMotion: false,
            isMobileViewport: false,
            initialSidebarState: "expanded",
            storedTheme: "ocean-dark");

        var themeState = ctx.Services.GetRequiredService<AgtThemeState>();

        _ = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, body => body.AddMarkupContent(0, "<p>Body</p>")));

        Assert.Equal("ocean-dark", themeState.Theme);
    }

    private static BunitContext CreateContext(
        TestNavigationManager navigationManager,
        bool enableAmbientEffects,
        bool prefersReducedMotion,
        bool isMobileViewport = false,
        string initialSidebarState = "expanded",
        string storedTheme = "plum-dark")
    {
        var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("agtTheme.closeAllPopups").SetVoidResult();
        ctx.JSInterop.Setup<string>("agtTheme.getStoredTheme", _ => true).SetResult(storedTheme);
        ctx.JSInterop.Setup<string>("agtTheme.getStoredDensity", _ => true).SetResult("comfortable");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult(initialSidebarState);
        ctx.JSInterop.SetupVoid("agtTheme.setStoredNavSectionState", _ => true).SetVoidResult();
        ctx.JSInterop.Setup<bool>("agtTheme.isViewportAtMost", _ => true).SetResult(isMobileViewport);
        ctx.JSInterop.Setup<bool>("agtTheme.prefersReducedMotion").SetResult(prefersReducedMotion);

        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = "plum-dark",
            DefaultDensity = "comfortable",
            EnableAmbientEffects = enableAmbientEffects,
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Azure, AgtTheme.Ms365, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        });

        var themeState = new AgtThemeState(options);
        var densityState = new AgtDensityState(options);

        ctx.Services.AddSingleton<IOptions<AgtUiOptions>>(options);
        ctx.Services.AddSingleton<IAgtCommandRegistry, AgtCommandRegistry>();
        ctx.Services.AddSingleton(densityState);
        ctx.Services.AddSingleton(themeState);
        ctx.Services.AddSingleton<NavigationManager>(navigationManager);

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
