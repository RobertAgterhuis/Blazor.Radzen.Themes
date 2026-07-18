using Agterhuis.Ui.Components.Data;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtDataGridTests
{
    [Fact]
    public void UsesDefaultGridSettings()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtDataGrid<Row>>(p => p.Add(x => x.Data, Array.Empty<Row>()));

        Assert.True(cut.Instance.AllowPaging);
        Assert.True(cut.Instance.AllowSorting);
        Assert.Equal(20, cut.Instance.PageSize);
    }

    [Fact]
    public void RendersCustomEmptyTemplate()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtDataGrid<Row>>(p =>
            p.Add(x => x.Data, Array.Empty<Row>())
             .Add(x => x.EmptyTemplate, builder => builder.AddContent(0, "Geen items test")));

        Assert.Contains("agt-data-grid", cut.Markup);
    }

    [Fact]
    public void AppliesStickyGridClassesWhenEnabled()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtDataGrid<Row>>(p => p
            .Add(x => x.Data, [new Row("Alpha")])
            .Add(x => x.StickySummaryFooter, true)
            .Add(x => x.StickyFirstColumn, true));

        Assert.Contains("agt-data-grid--sticky-summary", cut.Markup);
        Assert.Contains("agt-data-grid--sticky-first-column", cut.Markup);
    }

    [Fact]
    public void ExposesSettingsBinding()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var settings = new DataGridSettings();

        var cut = ctx.Render<AgtDataGrid<Row>>(p => p
            .Add(x => x.Data, [new Row("Alpha")])
            .Add(x => x.Settings, settings));

        Assert.Same(settings, cut.Instance.Settings);
    }

    private sealed record Row(string Name);
}
