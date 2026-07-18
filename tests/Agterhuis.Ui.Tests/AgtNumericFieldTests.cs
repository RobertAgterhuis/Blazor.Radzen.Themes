using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtNumericFieldTests
{
    [Fact]
    public void RendersNumericValue()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtNumericField>(p => p
            .Add(x => x.AriaLabel, "Testgetal")
            .Add(x => x.Value, 12.5m));

        Assert.Contains("12.5", cut.Markup);
    }

    [Fact]
    public void AppliesDisabledState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtNumericField>(p => p
            .Add(x => x.AriaLabel, "Testgetal")
            .Add(x => x.Disabled, true));

        Assert.Contains("disabled", cut.Markup);
    }
}
