using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtDatePickerTests
{
    [Fact]
    public void RendersDateValue()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var value = new DateTime(2026, 1, 15);
        var cut = ctx.Render<AgtDatePicker>(p => p
            .Add(x => x.AriaLabel, "Testdatum")
            .Add(x => x.Value, value));

        Assert.Contains("agt-date-picker", cut.Markup);
    }

    [Fact]
    public void RendersDisabledState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtDatePicker>(p => p
            .Add(x => x.AriaLabel, "Testdatum")
            .Add(x => x.Disabled, true));

        Assert.Contains("disabled", cut.Markup);
    }
}
