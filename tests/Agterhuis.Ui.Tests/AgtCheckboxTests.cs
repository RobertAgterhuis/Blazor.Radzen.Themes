using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtCheckboxTests
{
    [Fact]
    public void RendersClickableLabel()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtCheckbox>(p => p
            .Add(x => x.Label, "Akkoord")
            .Add(x => x.Name, "agree"));

        Assert.Contains("for=\"agree\"", cut.Markup);
        Assert.Contains("Akkoord", cut.Markup);
    }

    [Fact]
    public void AppliesDisabledState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtCheckbox>(p => p
            .Add(x => x.Label, "Akkoord")
            .Add(x => x.Disabled, true));

        Assert.Contains("disabled", cut.Markup);
    }
}
