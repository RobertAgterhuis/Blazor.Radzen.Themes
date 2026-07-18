using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtDropdownTests
{
    [Fact]
    public void RendersPlaceholder()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtDropdown<int>>(p =>
            p.Add(x => x.AriaLabel, "Testkeuze")
             .Add(x => x.Placeholder, "Kies")
             .Add(x => x.Data, new[] { 1, 2, 3 }.Cast<object>()));

        Assert.Contains("agt-dropdown", cut.Markup);
    }

    [Fact]
    public void RendersDisabledState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtDropdown<int>>(p =>
            p.Add(x => x.AriaLabel, "Testkeuze")
             .Add(x => x.Disabled, true)
             .Add(x => x.Data, new[] { 1, 2, 3 }.Cast<object>()));

        Assert.Contains("rz-state-disabled", cut.Markup);
    }
}
