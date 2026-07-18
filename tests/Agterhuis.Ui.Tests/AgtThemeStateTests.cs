using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Microsoft.Extensions.Options;

namespace Agterhuis.Ui.Tests;

public sealed class AgtThemeStateTests
{
    [Fact]
    public void DefaultTheme_IsPlumDark()
    {
        var state = CreateState("plum-dark");

        Assert.Equal("plum-dark", state.Theme);
        Assert.True(state.IsDark);
        Assert.Equal("plum", state.ActiveTheme.Name);
    }

    [Theory]
    [InlineData("dark", "plum-dark")]
    [InlineData("light", "plum-light")]
    [InlineData("ocean", "ocean-dark")]
    [InlineData("ocean-light", "ocean-light")]
    [InlineData("dagobah", "dagobah-dark")]
    [InlineData("dagobah-light", "dagobah-light")]
    [InlineData("dathomir", "dathomir-dark")]
    [InlineData("dathomir-light", "dathomir-light")]
    [InlineData("hoth", "hoth-dark")]
    [InlineData("hoth-light", "hoth-light")]
    [InlineData("tatooine", "tatooine-dark")]
    [InlineData("tatooine-light", "tatooine-light")]
    [InlineData("autotaalglas", "autotaalglas-light")]
    [InlineData("autotaalglas-light", "autotaalglas-light")]
    [InlineData("autotaalglas-contrast", "autotaalglas-contrast-light")]
    [InlineData("autotaalglas-portal", "autotaalglas-portal-light")]
    [InlineData("autotaalglas-mono", "autotaalglas-mono-light")]
    public void NormalizesLegacyAndFamilyInputs(string input, string expected)
    {
        var state = CreateState(input);

        Assert.Equal(expected, state.Theme);
    }

    [Fact]
    public void ToggleTheme_SwitchesWithinActiveFamily()
    {
        var state = CreateState("ocean-dark");

        state.ToggleTheme();
        Assert.Equal("ocean-light", state.Theme);

        state.ToggleTheme();
        Assert.Equal("ocean-dark", state.Theme);
    }

    [Fact]
    public void SetThemeFamily_PreservesDarkLightMode()
    {
        var state = CreateState("plum-light");

        state.SetThemeFamily("ocean");

        Assert.Equal("ocean-light", state.Theme);
        Assert.False(state.IsDark);
    }

    private static AgtThemeState CreateState(string defaultTheme)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = defaultTheme,
            AvailableThemes = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono]
        });

        return new AgtThemeState(options);
    }
}
