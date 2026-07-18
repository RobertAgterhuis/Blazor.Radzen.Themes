namespace Agterhuis.Ui.Theming;

public sealed record AgtTheme(string Name, string DisplayName, string LightVariantId, string DarkVariantId)
{
    public static readonly AgtTheme Plum = new("plum", "Plum Ink", "plum-light", "plum-dark");
    public static readonly AgtTheme Ocean = new("ocean", "Ocean", "ocean-light", "ocean-dark");
    public static readonly AgtTheme Dagobah = new("dagobah", "Dagobah", "dagobah-light", "dagobah-dark");
    public static readonly AgtTheme Dathomir = new("dathomir", "Dathomir", "dathomir-light", "dathomir-dark");
    public static readonly AgtTheme Hoth = new("hoth", "Hoth", "hoth-light", "hoth-dark");
    public static readonly AgtTheme Tatooine = new("tatooine", "Tatooine", "tatooine-light", "tatooine-dark");
    public static readonly AgtTheme Imperial = new("imperial", "Imperial", "imperial-light", "imperial-dark");
    public static readonly AgtTheme Autotaalglas = new("autotaalglas", "Autotaalglas", "autotaalglas-light", "autotaalglas-dark");
    public static readonly AgtTheme AutotaalglasContrast = new("autotaalglas-contrast", "Autotaalglas Contrast", "autotaalglas-contrast-light", "autotaalglas-contrast-dark");
    public static readonly AgtTheme AutotaalglasPortal = new("autotaalglas-portal", "Autotaalglas Portal", "autotaalglas-portal-light", "autotaalglas-portal-dark");
    public static readonly AgtTheme AutotaalglasMono = new("autotaalglas-mono", "Autotaalglas Mono", "autotaalglas-mono-light", "autotaalglas-mono-dark");

    public bool ContainsVariant(string variantId)
    {
        return string.Equals(LightVariantId, variantId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(DarkVariantId, variantId, StringComparison.OrdinalIgnoreCase);
    }
}