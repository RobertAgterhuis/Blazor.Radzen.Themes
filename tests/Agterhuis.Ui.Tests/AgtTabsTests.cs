using Agterhuis.Ui.Components.Layout;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Tests;

public sealed class AgtTabsTests
{
    [Fact]
    public void RendersTabsShellClass()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtTabs>();
        Assert.Contains("agt-tabs", cut.Markup);
    }

    [Fact]
    public void LazyRenderHidesInactiveContent()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<CascadingValue<bool>>(parameters => parameters
            .Add(p => p.Name, "AgtTabsLazy")
            .Add(p => p.Value, true)
            .Add(p => p.ChildContent, (RenderFragment)(outerBuilder =>
            {
                outerBuilder.OpenComponent<CascadingValue<int>>(0);
                outerBuilder.AddAttribute(1, "Name", "AgtTabsSelectedIndex");
                outerBuilder.AddAttribute(2, "Value", 0);
                outerBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(innerBuilder =>
                {
                    innerBuilder.OpenComponent<AgtTabItem>(4);
                    innerBuilder.AddAttribute(5, nameof(AgtTabItem.Index), 1);
                    innerBuilder.AddAttribute(6, nameof(AgtTabItem.Text), "Tab B");
                    innerBuilder.AddAttribute(7, nameof(AgtTabItem.ChildContent), (RenderFragment)(b => b.AddContent(8, "Content B")));
                    innerBuilder.CloseComponent();
                }));
                outerBuilder.CloseComponent();
            })));

        Assert.DoesNotContain("Content B", cut.Markup);
    }
}
