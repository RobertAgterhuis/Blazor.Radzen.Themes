using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Microsoft.Extensions.Options;

namespace Agterhuis.Ui.Tests;

public sealed class AgtDensityStateTests
{
    [Fact]
    public void DefaultDensity_IsComfortable()
    {
        var state = CreateState(AgtDensityState.Comfortable);

        Assert.Equal(AgtDensityState.Comfortable, state.Density);
        Assert.False(state.IsCompact);
    }

    [Theory]
    [InlineData("compact", "compact")]
    [InlineData("Compact", "compact")]
    [InlineData("comfortable", "comfortable")]
    [InlineData("", "comfortable")]
    public void NormalizesDensityValues(string input, string expected)
    {
        var state = CreateState(input);

        Assert.Equal(expected, state.Density);
    }

    [Fact]
    public void ToggleDensity_SwitchesModes()
    {
        var state = CreateState(AgtDensityState.Comfortable);

        state.ToggleDensity();
        Assert.Equal(AgtDensityState.Compact, state.Density);

        state.ToggleDensity();
        Assert.Equal(AgtDensityState.Comfortable, state.Density);
    }

    private static AgtDensityState CreateState(string defaultDensity)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultDensity = defaultDensity
        });

        return new AgtDensityState(options);
    }
}