using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtTextAreaTests
{
    [Fact]
    public void DefaultsToThreeRows()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtTextArea>(p => p
            .Add(x => x.Label, "Toelichting"));

        Assert.Contains("rows=\"3\"", cut.Markup);
    }

    [Fact]
    public void ShowsDefaultCharacterCounterWhenMaxLengthIsSet()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtTextArea>(p => p
            .Add(x => x.Label, "Toelichting")
            .Add(x => x.Value, "abc")
            .Add(x => x.MaxLength, 10L));

        Assert.Contains("3/10 tekens", cut.Markup);
    }
}
