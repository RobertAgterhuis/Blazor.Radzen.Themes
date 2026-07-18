using Agterhuis.Ui.Demo.Components.Layout;
using Agterhuis.Ui.Options;
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
        Assert.Contains("Dit is de theme-QA-weergave.", cut.Markup);
    }

    private static BunitContext CreateContext(TestNavigationManager navigationManager, bool enableAmbientEffects, bool prefersReducedMotion)
    {
        var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredTheme", _ => true).SetResult("plum-dark");
        ctx.JSInterop.Setup<bool>("agtTheme.prefersReducedMotion").SetResult(prefersReducedMotion);

        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = "plum-dark",
            EnableAmbientEffects = enableAmbientEffects,
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        });

        var themeState = new AgtThemeState(options);

        ctx.Services.AddSingleton<IOptions<AgtUiOptions>>(options);
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
            Initialize(BaseUri, uri);
        }
    }
}
