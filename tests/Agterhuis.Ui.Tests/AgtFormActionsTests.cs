using Agterhuis.Ui.Components.Forms;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtFormActionsTests
{
    [Fact]
    public void RendersDefaultButtons()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtFormActions>();

        Assert.Contains("Opslaan", cut.Markup);
        Assert.Contains("Annuleren", cut.Markup);
    }

    [Fact]
    public void InvokesSaveCallback()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var calls = 0;
        var cut = ctx.Render<AgtFormActions>(p =>
            p.Add(x => x.Save, EventCallback.Factory.Create<MouseEventArgs>(this, _ => calls++)));

        cut.FindAll("button")[1].Click();
        Assert.Equal(1, calls);
    }
}
