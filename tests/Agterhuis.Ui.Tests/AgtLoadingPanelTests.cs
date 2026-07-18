using Agterhuis.Ui.Components.Feedback;
using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtLoadingPanelTests
{
    [Fact]
    public void ShowsOverlayWhenLoading()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtLoadingPanel>(p => p.Add(x => x.IsLoading, true));

        Assert.Contains("agt-loading-panel__overlay", cut.Markup);
    }

    [Fact]
    public void HidesOverlayWhenNotLoading()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtLoadingPanel>(p => p.Add(x => x.IsLoading, false));

        Assert.DoesNotContain("agt-loading-panel__overlay", cut.Markup);
    }

    [Fact]
    public void RendersSkeletonRowsWhenConfigured()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtLoadingPanel>(p => p
            .Add(x => x.IsLoading, true)
            .Add(x => x.Variant, AgtLoadingPanelVariant.Skeleton)
            .Add(x => x.SkeletonPreset, AgtLoadingPanelSkeletonPreset.GridRows)
            .Add(x => x.SkeletonItems, 3));

        Assert.Contains("agt-loading-panel__overlay--skeleton", cut.Markup);
        Assert.Equal(3, cut.FindAll(".agt-loading-panel__skeleton-row").Count);
    }
}
