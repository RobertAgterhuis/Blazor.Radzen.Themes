using Agterhuis.Ui.Components.Feedback;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtDeltaTests
{
    [Fact]
    public void PositiveDelta_RendersIncreaseLabelAndValue()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<AgtDelta>(0);
            builder.AddAttribute(1, nameof(AgtDelta.Value), 0.12m);
            builder.AddAttribute(2, nameof(AgtDelta.Format), "P0");
            builder.CloseComponent();
        });

        Assert.Contains("▲", cut.Markup);
        Assert.Contains("Stijging", cut.Markup);
        Assert.Contains("12", cut.Markup);
    }

    [Fact]
    public void NeutralDelta_RendersNeutralIndicator()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<AgtDelta>(0);
            builder.AddAttribute(1, nameof(AgtDelta.Value), 0m);
            builder.CloseComponent();
        });

        Assert.Contains("—", cut.Markup);
        Assert.Contains("Gelijk", cut.Markup);
    }
}