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

    private sealed record Row(string Name);
}
