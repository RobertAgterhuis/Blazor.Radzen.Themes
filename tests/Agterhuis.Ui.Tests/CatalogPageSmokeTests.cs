using Agterhuis.Ui.Demo.Components.Pages.Catalog;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class CatalogPageSmokeTests
{
    [Fact]
    public void CatalogIndex_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogIndex>();
        Assert.Contains("Radzen catalogus (Radzen)", cut.Markup);
    }

    [Fact]
    public void ButtonsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ButtonsCatalog>();
        Assert.Contains("Buttons", cut.Markup);
    }

    [Fact]
    public void TextInputsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<TextInputsCatalog>();
        Assert.Contains("Text Inputs", cut.Markup);
    }

    [Fact]
    public void SelectionInputsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<SelectionInputsCatalog>();
        Assert.Contains("Selection Inputs", cut.Markup);
    }

    [Fact]
    public void PickersCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<PickersCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Pickers", cut.Markup);
    }

    [Fact]
    public void FormsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<FormsCatalog>();
        Assert.Contains("Forms", cut.Markup);
    }

    [Fact]
    public void ValidatorsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ValidatorsCatalog>();
        Assert.Contains("Validators", cut.Markup);
    }

    [Fact]
    public void DataCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<DataCatalog>();
        Assert.Contains("Data", cut.Markup);
    }

    [Fact]
    public void SchedulingCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<SchedulingCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Scheduling", cut.Markup);
    }

    [Fact]
    public void NavigationCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<NavigationCatalog>();
        Assert.Contains("Navigation", cut.Markup);
    }

    [Fact]
    public void OverlaysCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<OverlaysCatalog>();
        Assert.Contains("Overlays", cut.Markup);
    }

    [Fact]
    public void LayoutCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<LayoutCatalog>();
        Assert.Contains("Layout", cut.Markup);
    }

    [Fact]
    public void FeedbackCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<FeedbackCatalog>();
        Assert.Contains("Feedback", cut.Markup);
    }

    [Fact]
    public void ChartsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ChartsCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Charts", cut.Markup);
    }

    [Fact]
    public void GaugesCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<GaugesCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Gauges", cut.Markup);
    }

    [Fact]
    public void DisplayCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<DisplayCatalog>();
        Assert.Contains("Display", cut.Markup);
    }

    [Fact]
    public void EmbedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<EmbedCatalog>();
        Assert.Contains("Embed", cut.Markup);
    }

    [Fact]
    public void FormsAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<FormsAdvancedCatalog>();
        Assert.Contains("Forms Advanced", cut.Markup);
    }

    [Fact]
    public void DataAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<DataAdvancedCatalog>();
        Assert.Contains("Data Advanced", cut.Markup);
    }

    [Fact]
    public void OverlaysAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<OverlaysAdvancedCatalog>();
        Assert.Contains("Overlays Advanced", cut.Markup);
    }

    [Fact]
    public void LayoutAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<LayoutAdvancedCatalog>();
        Assert.Contains("Layout Advanced", cut.Markup);
    }

    [Fact]
    public void ChartsAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ChartsAdvancedCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Charts Advanced", cut.Markup);
    }

    [Fact]
    public void AllComponentsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<AllComponentsCatalog>();
        Assert.Contains("All Components", cut.Markup);
    }

    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        return ctx;
    }
}
