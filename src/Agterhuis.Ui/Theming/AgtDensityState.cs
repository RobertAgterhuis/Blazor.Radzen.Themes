using Agterhuis.Ui.Options;
using Microsoft.Extensions.Options;

namespace Agterhuis.Ui.Theming;

public sealed class AgtDensityState
{
    public const string Comfortable = "comfortable";
    public const string Compact = "compact";

    private string _density;

    public AgtDensityState(IOptions<AgtUiOptions> options)
    {
        _density = NormalizeDensity(options.Value.DefaultDensity);
    }

    public event Action? DensityChanged;

    public string Density => _density;

    public bool IsCompact => string.Equals(_density, Compact, StringComparison.OrdinalIgnoreCase);

    public void SetDensity(string? density)
    {
        var normalized = NormalizeDensity(density);
        if (string.Equals(_density, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _density = normalized;
        DensityChanged?.Invoke();
    }

    public void ToggleDensity()
    {
        SetDensity(IsCompact ? Comfortable : Compact);
    }

    public static string NormalizeDensity(string? density)
    {
        return string.Equals(density?.Trim(), Compact, StringComparison.OrdinalIgnoreCase)
            ? Compact
            : Comfortable;
    }
}