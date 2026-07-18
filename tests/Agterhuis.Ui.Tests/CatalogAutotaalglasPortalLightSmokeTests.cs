using Agterhuis.Ui.Demo.Components.Pages.Catalog;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class CatalogAutotaalglasPortalLightSmokeTests
{
    [Fact]
    public void KeyCatalogSlices_RenderUnderAutotaalglasPortalLightTheme()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var themeState = new AgtThemeState(Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = "autotaalglas-portal-light",
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        }));

        Assert.Equal("autotaalglas-portal-light", themeState.Theme);

        _ = ctx.Render<ButtonsCatalog>();
        _ = ctx.Render<FormsCatalog>();
        _ = ctx.Render<DataCatalog>();
        _ = ctx.Render<NavigationCatalog>();
        _ = ctx.Render<FeedbackCatalog>();
        _ = ctx.Render<OverlaysCatalog>();
    }
}
