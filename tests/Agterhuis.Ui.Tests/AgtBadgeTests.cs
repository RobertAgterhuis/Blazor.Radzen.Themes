using Agterhuis.Ui.Components.Feedback;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtBadgeTests
{
    [Fact]
    public void RendersTextAndIcon()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtBadge>(p => p
            .Add(x => x.Text, "Nieuw")
            .Add(x => x.Icon, "label"));

        Assert.Contains("Nieuw", cut.Markup);
        Assert.Contains("label", cut.Markup);
    }

    [Fact]
    public void MapsDangerIntentToDangerStyle()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtBadge>(p => p
            .Add(x => x.Text, "Fout")
            .Add(x => x.Intent, AgtIntent.Danger));

        Assert.Contains("danger", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}
