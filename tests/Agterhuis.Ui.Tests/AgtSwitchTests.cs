using Agterhuis.Ui.Components.Forms;
using Bunit;
using Microsoft.AspNetCore.Components;

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

    [Fact]
    public void ClickOnSwitch_InvokesValueChanged()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var callbackInvoked = false;
        var lastValue = false;

        var cut = ctx.Render<AgtSwitch>(p => p
            .Add(x => x.Label, "Geavanceerd")
            .Add(x => x.Value, false)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool>(this, value =>
            {
                callbackInvoked = true;
                lastValue = value;
            })));

        cut.Find(".rz-switch").Click();

        Assert.True(callbackInvoked);
        Assert.True(lastValue);
    }
}
