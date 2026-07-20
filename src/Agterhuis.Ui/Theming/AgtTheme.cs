namespace Agterhuis.Ui.Theming;

public sealed record AgtTheme(
    string Name,
    string DisplayName,
    string LightVariantId,
    string DarkVariantId,
    string PreviewCanvas,
    string PreviewPrimary,
    string PreviewAccent)
{
    public static readonly AgtTheme Plum = new("plum", "Plum Ink", "plum-light", "plum-dark", "#fbfafd", "#680898", "#f1ce05");
    public static readonly AgtTheme Ocean = new("ocean", "Ocean", "ocean-light", "ocean-dark", "#f6fbfb", "#0b6e6e", "#e8a13d");
    public static readonly AgtTheme Dagobah = new("dagobah", "Dagobah", "dagobah-light", "dagobah-dark", "#f7f9f3", "#6f9c4f", "#7cfc5a");
    public static readonly AgtTheme Dathomir = new("dathomir", "Dathomir", "dathomir-light", "dathomir-dark", "#faf6f5", "#a8211c", "#ff3b30");
    public static readonly AgtTheme Hoth = new("hoth", "Hoth", "hoth-light", "hoth-dark", "#f6f9fc", "#35678f", "#ff8c42");
    public static readonly AgtTheme Tatooine = new("tatooine", "Tatooine", "tatooine-light", "tatooine-dark", "#faf6ee", "#b0761d", "#e8622c");
    public static readonly AgtTheme Imperial = new("imperial", "Imperial", "imperial-light", "imperial-dark", "#f5f6f8", "#9aa7b5", "#e5231b");
    public static readonly AgtTheme Azure = new("azure", "Azure", "azure-light", "azure-dark", "#f5f5f5", "#0078d4", "#0078d4");
    public static readonly AgtTheme Ms365 = new("ms365", "MS365", "ms365-light", "ms365-dark", "#fafafa", "#0f6cbd", "#0f6cbd");
    public static readonly AgtTheme Volt = new("volt", "Volt", "volt-light", "volt-dark", "#faf8f2", "#e8e5d8", "#c8f542");
    public static readonly AgtTheme Autotaalglas = new("autotaalglas", "Autotaalglas", "autotaalglas-light", "autotaalglas-dark", "#f8fafe", "#002575", "#e4002b");
    public static readonly AgtTheme AutotaalglasContrast = new("autotaalglas-contrast", "Autotaalglas Contrast", "autotaalglas-contrast-light", "autotaalglas-contrast-dark", "#ffffff", "#002575", "#b3001f");
    public static readonly AgtTheme AutotaalglasPortal = new("autotaalglas-portal", "Autotaalglas Portal", "autotaalglas-portal-light", "autotaalglas-portal-dark", "#f7fbff", "#005fc5", "#e4002b");
    public static readonly AgtTheme AutotaalglasMono = new("autotaalglas-mono", "Autotaalglas Mono", "autotaalglas-mono-light", "autotaalglas-mono-dark", "#f8fafe", "#002575", "#003b87");

    public static IReadOnlyList<AgtTheme> All { get; } =
    [
        Plum,
        Ocean,
        Dagobah,
        Dathomir,
        Hoth,
        Tatooine,
        Imperial,
        Azure,
        Ms365,
        Volt,
        Autotaalglas,
        AutotaalglasContrast,
        AutotaalglasPortal,
        AutotaalglasMono
    ];

    public bool ContainsVariant(string variantId)
    {
        return string.Equals(LightVariantId, variantId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(DarkVariantId, variantId, StringComparison.OrdinalIgnoreCase);
    }
}