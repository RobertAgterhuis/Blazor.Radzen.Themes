using Agterhuis.Ui.Components.Data;
using Agterhuis.Ui.Demo.Components.Pages;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public void DataGridDemo_HeaderOrderMatchesFirstRowCellOrder()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<Agterhuis.Ui.Demo.Services.DemoSourceProvider>();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<DataGridDemo>();

        var headers = cut.FindAll("thead th")
            .Select(header => header.TextContent.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Take(2)
            .ToArray();

        Assert.Equal(["Naam", "Score"], headers);

        var firstRowCells = cut.FindAll("tbody tr:first-child td")
            .Select(cell => cell.TextContent.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Take(2)
            .ToArray();

        Assert.Equal(["Alfa", "95"], firstRowCells);
    }

    private sealed record Row(string Name);
}
