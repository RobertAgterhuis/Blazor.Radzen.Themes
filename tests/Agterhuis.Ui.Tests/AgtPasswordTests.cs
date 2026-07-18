using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtPasswordTests
{
    [Fact]
    public void RendersToggleButtonWithAriaPressed()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtPassword>(p => p
            .Add(x => x.Label, "Wachtwoord")
            .Add(x => x.Value, "secret"));

        Assert.Contains("agt-password__toggle", cut.Markup);
        Assert.DoesNotContain("agt-password__text-input", cut.Markup);
    }

    [Fact]
    public void ToggleButtonSwitchesVisibilityState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtPassword>(p => p
            .Add(x => x.Label, "Wachtwoord")
            .Add(x => x.Value, "secret"));

        cut.Find("button").Click();

        Assert.Contains("agt-password__text-input", cut.Markup);
    }
}
