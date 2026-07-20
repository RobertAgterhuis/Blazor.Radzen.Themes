using Agterhuis.Ui.Components.Layout;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Bunit;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class AgtThemeSwitcherTests
{
    [Fact]
    public void SwitcherRendersThemeControls()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var themeState = CreateThemeState("plum-dark");

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<AgtThemeSwitcher>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Contains("Thema", cut.Markup);
        Assert.Contains("agt-theme-family", cut.Markup);
        Assert.Contains("agt-theme-value__separator", cut.Markup);
        Assert.Contains("·", cut.Markup);
        Assert.Contains("MS365", cut.Markup);
        Assert.Contains("AAA / hoge contrast", cut.Markup);
    }

    [Fact]
    public void ToggleFlipsLightDarkWithinCurrentFamily()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var themeState = CreateThemeState("ocean-dark");

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<AgtThemeToggle>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        cut.Find("button").Click();

        Assert.Equal("ocean-light", themeState.Theme);
    }

    [Fact]
    public async Task SelectingFamilyClosesPopupsBeforeThemeChange()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var closeInvocation = ctx.JSInterop.SetupVoid("agtTheme.closeAllPopups");
        closeInvocation.SetVoidResult();

        var themeState = CreateThemeState("plum-dark");
        var themeChangedCount = 0;
        themeState.ThemeChanged += () => themeChangedCount++;

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<AgtThemeSwitcher>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var dropdown = cut.FindComponent<Radzen.Blazor.RadzenDropDown<string>>();
        await cut.InvokeAsync(() => dropdown.Instance.Change.InvokeAsync("dagobah"));

        Assert.Equal("dagobah-dark", themeState.Theme);
        Assert.Equal(1, themeChangedCount);
        Assert.Single(closeInvocation.Invocations);
    }

    [Fact]
    public async Task SelectingFamilyKeepsLightVariant()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("agtTheme.closeAllPopups").SetVoidResult();

        var themeState = CreateThemeState("hoth-light");

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<AgtThemeSwitcher>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var dropdown = cut.FindComponent<Radzen.Blazor.RadzenDropDown<string>>();
        await cut.InvokeAsync(() => dropdown.Instance.Change.InvokeAsync("tatooine"));

        Assert.Equal("tatooine-light", themeState.Theme);
    }

    [Fact]
    public void SwitcherUpdatesDisplayedTheme_WhenThemeStateChangesExternally()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var themeState = CreateThemeState("plum-dark");

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<CascadingValue<AgtThemeState>>(0);
            builder.AddAttribute(1, "Value", themeState);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
            {
                child.OpenComponent<AgtThemeSwitcher>(0);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        });

        cut.WaitForAssertion(() => Assert.Contains("Plum Ink", cut.Markup));

        cut.InvokeAsync(() => themeState.SetTheme("ocean-dark"));

        cut.WaitForAssertion(() => Assert.Contains("Ocean", cut.Markup));
    }

    private static AgtThemeState CreateThemeState(string defaultTheme)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = defaultTheme,
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Azure, AgtTheme.Ms365, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        });

        return new AgtThemeState(options);
    }
}
