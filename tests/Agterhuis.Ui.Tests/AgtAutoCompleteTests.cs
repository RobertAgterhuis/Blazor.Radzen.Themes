using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtAutoCompleteTests
{
    [Fact]
    public void UsesDefaultFilterDelay()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtAutoComplete<string>>(p => p
            .Add(x => x.Label, "Zoeken")
            .Add(x => x.Data, new[] { "Anna", "Bram" }));

        Assert.Equal(300, cut.Instance.FilterDelay);
    }

    [Fact]
    public void RendersEmptyTemplateText()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtAutoComplete<string>>(p => p
            .Add(x => x.Label, "Zoeken")
            .Add(x => x.EmptyText, "Geen resultaten")
            .Add(x => x.Data, new[] { "Anna", "Bram" }));

        Assert.Equal("Geen resultaten", cut.Instance.EmptyText);
    }
}
