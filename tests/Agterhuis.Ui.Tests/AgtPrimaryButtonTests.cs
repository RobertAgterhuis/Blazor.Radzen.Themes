using Agterhuis.Ui.Components.Buttons;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtPrimaryButtonTests
{
    [Fact]
    public void RendersText()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtPrimaryButton>(parameters =>
            parameters.Add(p => p.Text, "Opslaan"));

        Assert.Contains("Opslaan", cut.Markup);
    }

    [Fact]
    public void IsDisabledWhileBusy()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtPrimaryButton>(parameters =>
            parameters.Add(p => p.Text, "Opslaan")
                      .Add(p => p.IsBusy, true));

        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void ClickCallbackFiresWhenEnabled()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var clicked = 0;

        var cut = ctx.Render<AgtPrimaryButton>(parameters =>
            parameters.Add(p => p.Text, "Opslaan")
                      .Add(p => p.Click, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicked++)));

        cut.Find("button").Click();

        Assert.Equal(1, clicked);
    }

    [Fact]
    public void NoClickWhileDisabled()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var clicked = 0;

        var cut = ctx.Render<AgtPrimaryButton>(parameters =>
            parameters.Add(p => p.Text, "Opslaan")
                      .Add(p => p.Disabled, true)
                      .Add(p => p.Click, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicked++)));

        cut.Find("button").Click();

        Assert.Equal(0, clicked);
    }
}
