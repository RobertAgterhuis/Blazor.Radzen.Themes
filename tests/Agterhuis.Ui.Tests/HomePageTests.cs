using Agterhuis.Ui.Demo.Components.Pages;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Agterhuis.Ui.Tests;

public sealed class HomePageTests
{
    [Fact]
    public void ReducesMotionAndAmbient_WhenKnobIsOffAndPrefersReducedMotionIsTrue()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<bool>("agtTheme.prefersReducedMotion").SetResult(true);

        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            EnableAnimations = true,
            EnableAmbientEffects = false,
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Azure, AgtTheme.Ms365, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        });

        ctx.Services.AddSingleton<IOptions<AgtUiOptions>>(options);
        var themeState = new AgtThemeState(options);

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<Home>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var metricValues = cut.FindAll(".agt-home-metric__value").Select(element => element.TextContent.Trim()).ToArray();

        cut.WaitForAssertion(() => Assert.DoesNotContain("agt-home-hero__ambient", cut.Markup));
        Assert.Equal(["19", "12", "86", "25"], metricValues);
    }

    [Theory]
    [InlineData("hoth-dark", "Hoth")]
    [InlineData("tatooine-dark", "Tatooine")]
    [InlineData("ms365-dark", "MS365")]
    public void HeroShowsTheActiveThemeFamily(string initialTheme, string expectedFamily)
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<bool>("agtTheme.prefersReducedMotion").SetResult(true);

        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = initialTheme,
            EnableAnimations = false,
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Azure, AgtTheme.Ms365, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        });

        ctx.Services.AddSingleton<IOptions<AgtUiOptions>>(options);
        var themeState = new AgtThemeState(options);

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<Home>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Contains(expectedFamily, cut.Markup);
        Assert.DoesNotContain("Dagobah", cut.Markup);
    }

    [Fact]
    public void HeroRerendersWhenThemeStateChanges()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<bool>("agtTheme.prefersReducedMotion").SetResult(true);

        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = "dagobah-dark",
            EnableAnimations = false,
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Azure, AgtTheme.Ms365, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        });

        ctx.Services.AddSingleton<IOptions<AgtUiOptions>>(options);
        var themeState = new AgtThemeState(options);

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<Home>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        themeState.SetTheme("hoth-dark");

        cut.WaitForAssertion(() => Assert.Contains("Hoth", cut.Markup));
        Assert.DoesNotContain("Dagobah", cut.Markup);
    }
}
