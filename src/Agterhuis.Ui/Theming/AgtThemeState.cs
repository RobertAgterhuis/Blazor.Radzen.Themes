using Agterhuis.Ui.Options;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Agterhuis.Ui.Theming;

public sealed class AgtThemeState
{
    private readonly IReadOnlyList<AgtTheme> _themes;
    private string _theme;

    public AgtThemeState(IOptions<AgtUiOptions> options)
    {
        _themes = options.Value.AvailableThemes.Count > 0
            ? [.. options.Value.AvailableThemes]
            : [AgtTheme.Plum, AgtTheme.Ocean, AgtTheme.Dagobah, AgtTheme.Dathomir, AgtTheme.Hoth, AgtTheme.Tatooine, AgtTheme.Imperial, AgtTheme.Autotaalglas, AgtTheme.AutotaalglasContrast, AgtTheme.AutotaalglasPortal, AgtTheme.AutotaalglasMono];
        _theme = NormalizeTheme(options.Value.DefaultTheme, _themes);
    }

    public event Action? ThemeChanged;

    public string Theme => _theme;

    public IReadOnlyList<AgtTheme> AvailableThemes => _themes;

    public bool IsDark
    {
        get
        {
            var active = ActiveTheme;
            return string.Equals(_theme, active.DarkVariantId, StringComparison.OrdinalIgnoreCase);
        }
    }

    public AgtTheme ActiveTheme
    {
        get
        {
            var family = GetThemeFamilyName(_theme);
            return _themes.FirstOrDefault(t => string.Equals(t.Name, family, StringComparison.OrdinalIgnoreCase))
                ?? _themes[0];
        }
    }

    public void SetTheme(string theme)
    {
        var normalized = NormalizeTheme(theme, _themes);

        if (string.Equals(_theme, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _theme = normalized;
        ThemeChanged?.Invoke();
    }

    public void ToggleTheme()
    {
        var active = ActiveTheme;
        SetTheme(IsDark ? active.LightVariantId : active.DarkVariantId);
    }

    public void SetThemeFamily(string familyName)
    {
        var target = _themes.FirstOrDefault(t => string.Equals(t.Name, familyName, StringComparison.OrdinalIgnoreCase));
        if (target is null)
        {
            return;
        }

        SetTheme(IsDark ? target.DarkVariantId : target.LightVariantId);
    }

    private static string NormalizeTheme(string? theme, IReadOnlyList<AgtTheme> themes)
    {
        var requested = theme?.Trim();
        if (string.IsNullOrWhiteSpace(requested))
        {
            return themes[0].DarkVariantId;
        }

        if (string.Equals(requested, "dark", StringComparison.OrdinalIgnoreCase))
        {
            return themes.FirstOrDefault(t => string.Equals(t.Name, "plum", StringComparison.OrdinalIgnoreCase))?.DarkVariantId
                ?? themes[0].DarkVariantId;
        }

        if (string.Equals(requested, "light", StringComparison.OrdinalIgnoreCase))
        {
            return themes.FirstOrDefault(t => string.Equals(t.Name, "plum", StringComparison.OrdinalIgnoreCase))?.LightVariantId
                ?? themes[0].LightVariantId;
        }

        var exact = themes.FirstOrDefault(t => t.ContainsVariant(requested));
        if (exact is not null)
        {
            if (string.Equals(requested, exact.LightVariantId, StringComparison.OrdinalIgnoreCase))
            {
                return exact.LightVariantId;
            }

            return exact.DarkVariantId;
        }

        var family = themes.FirstOrDefault(t => string.Equals(t.Name, requested, StringComparison.OrdinalIgnoreCase));
        if (family is not null)
        {
            if (family.Name.StartsWith("autotaalglas", StringComparison.OrdinalIgnoreCase))
            {
                return family.LightVariantId;
            }

            return family.DarkVariantId;
        }

        return themes[0].DarkVariantId;
    }

    private static string GetThemeFamilyName(string theme)
    {
        var separatorIndex = theme.IndexOf('-');
        if (separatorIndex <= 0)
        {
            return theme;
        }

        return theme[..separatorIndex];
    }
}
