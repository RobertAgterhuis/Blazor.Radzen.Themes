using Agterhuis.Ui.Demo.Components.Pages.Catalog;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class CatalogAzureLightSmokeTests
{
    [Fact]
    public void KeyCatalogPages_RenderUnderAzureLightTheme()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var themeState = new AgtThemeState(Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = "azure-light",
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Azure, AgtTheme.Ms365, AgtTheme.Autotaalglas]
        }));

        Assert.Equal("azure-light", themeState.Theme);

        _ = ctx.Render<ButtonsCatalog>();
        _ = ctx.Render<TextInputsCatalog>();
        _ = ctx.Render<SelectionInputsCatalog>();
        _ = ctx.Render<PickersCatalog>(p => p.Add(x => x.PreviewOnly, true));
        _ = ctx.Render<FormsCatalog>();
        _ = ctx.Render<FormsAdvancedCatalog>();
        _ = ctx.Render<DataCatalog>();
        _ = ctx.Render<DataAdvancedCatalog>();
        _ = ctx.Render<LayoutCatalog>();
        _ = ctx.Render<LayoutAdvancedCatalog>();
        _ = ctx.Render<NavigationCatalog>();
        _ = ctx.Render<FeedbackCatalog>();
        _ = ctx.Render<ChartsCatalog>(p => p.Add(x => x.PreviewOnly, true));
        _ = ctx.Render<ChartsAdvancedCatalog>(p => p.Add(x => x.PreviewOnly, true));
        _ = ctx.Render<GaugesCatalog>(p => p.Add(x => x.PreviewOnly, true));
        _ = ctx.Render<SchedulingCatalog>(p => p.Add(x => x.PreviewOnly, true));
        _ = ctx.Render<OverlaysCatalog>();
        _ = ctx.Render<OverlaysAdvancedCatalog>();
        _ = ctx.Render<DisplayCatalog>();
        _ = ctx.Render<EmbedCatalog>();
        _ = ctx.Render<AllComponentsCatalog>();
    }
}