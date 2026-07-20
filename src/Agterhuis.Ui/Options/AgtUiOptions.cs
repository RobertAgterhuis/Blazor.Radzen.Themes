using Agterhuis.Ui.Theming;
using System.Collections.Generic;

namespace Agterhuis.Ui.Options;

public sealed class AgtUiOptions
{
    public string ApplicationName { get; set; } = "Agterhuis";

    public bool EnableAnimations { get; set; } = true;

    public bool EnableAmbientEffects { get; set; } = true;

    public string DefaultTheme { get; set; } = "plum-dark";

    public string DefaultDensity { get; set; } = AgtDensityState.Comfortable;

    public IList<AgtTheme> AvailableThemes { get; set; } = [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Azure, AgtTheme.Ms365, AgtTheme.Volt, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono];

    public string DefaultCulture { get; set; } = "nl-NL";
}
