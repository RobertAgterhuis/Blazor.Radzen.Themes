using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtTextFieldTests
{
    [Fact]
    public void RendersPlaceholder()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtTextField>(p => p
            .Add(x => x.AriaLabel, "Testveld")
            .Add(x => x.Placeholder, "Typ hier"));

        Assert.Contains("Typ hier", cut.Markup);
    }

    [Fact]
    public void RendersDisabledInput()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtTextField>(p => p
            .Add(x => x.AriaLabel, "Testveld")
            .Add(x => x.Disabled, true));

        Assert.Contains("disabled", cut.Markup);
    }
}
