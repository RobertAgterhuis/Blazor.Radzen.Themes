using Agterhuis.Ui.Components.Layout;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Tests;

public sealed class AgtDensityToggleTests
{
    [Fact]
    public void ToggleFlipsDensityMode()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var densityState = new AgtDensityState(Microsoft.Extensions.Options.Options.Create(new AgtUiOptions()));

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtDensityState>>(0);
            builder.AddAttribute(1, "Value", densityState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<AgtDensityToggle>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        cut.Find("button").Click();

        Assert.Equal(AgtDensityState.Compact, densityState.Density);
    }
}