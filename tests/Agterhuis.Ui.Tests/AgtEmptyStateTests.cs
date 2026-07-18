using Agterhuis.Ui.Components.Feedback;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtEmptyStateTests
{
    [Fact]
    public void RendersTitleAndDescription()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtEmptyState>(p =>
            p.Add(x => x.Title, "Geen resultaten")
             .Add(x => x.Description, "Pas filters aan"));

        Assert.Contains("Geen resultaten", cut.Markup);
        Assert.Contains("Pas filters aan", cut.Markup);
    }

    [Fact]
    public void RendersActionSlot()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtEmptyState>(p =>
            p.Add(x => x.Action, builder => builder.AddContent(0, "Actie")));

        Assert.Contains("Actie", cut.Markup);
    }
}
