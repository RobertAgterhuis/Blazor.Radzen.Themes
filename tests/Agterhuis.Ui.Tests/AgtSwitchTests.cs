using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtSwitchTests
{
    [Fact]
    public void RendersDefaultOnOffLabels()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtSwitch>(p => p
            .Add(x => x.Label, "Meldingen")
            .Add(x => x.Value, true));

        Assert.Contains("Aan", cut.Markup);
    }

    [Fact]
    public void RendersCustomOnOffLabels()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtSwitch>(p => p
            .Add(x => x.Label, "Meldingen")
            .Add(x => x.OnText, "Actief")
            .Add(x => x.OffText, "Inactief")
            .Add(x => x.Value, false));

        Assert.Contains("Inactief", cut.Markup);
    }
}
