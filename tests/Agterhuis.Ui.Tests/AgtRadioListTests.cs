using Agterhuis.Ui.Components.Forms;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtRadioListTests
{
    [Fact]
    public void RendersLegendAndChoices()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var data = new[] { new { Text = "A", Value = 1 }, new { Text = "B", Value = 2 } }.Cast<object>();
        var cut = ctx.Render<AgtRadioList<int>>(p => p
            .Add(x => x.Label, "Keuze")
            .Add(x => x.Data, data)
            .Add(x => x.TextProperty, "Text")
            .Add(x => x.ValueProperty, "Value"));

        Assert.Contains("Keuze", cut.Markup);
        Assert.Contains("A", cut.Markup);
    }

    [Fact]
    public void DefaultsToVerticalOrientation()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var data = new[] { new { Text = "A", Value = 1 } }.Cast<object>();
        var cut = ctx.Render<AgtRadioList<int>>(p => p
            .Add(x => x.Label, "Keuze")
            .Add(x => x.Data, data)
            .Add(x => x.TextProperty, "Text")
            .Add(x => x.ValueProperty, "Value"));

        Assert.Contains("vertical", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}
