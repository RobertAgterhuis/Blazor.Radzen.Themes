using System.Globalization;

namespace Agterhuis.Ui.Formatting;

public static class AgtNumberFormat
{
    private static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("nl-NL");

    public static string Format(object? value, string format = "N0", string culture = "nl-NL")
    {
        if (value is null)
        {
            return string.Empty;
        }

        var cultureInfo = string.Equals(culture, DefaultCulture.Name, StringComparison.OrdinalIgnoreCase)
            ? DefaultCulture
            : CultureInfo.GetCultureInfo(culture);

        return value switch
        {
            decimal decimalValue => decimalValue.ToString(format, cultureInfo),
            double doubleValue => doubleValue.ToString(format, cultureInfo),
            float floatValue => floatValue.ToString(format, cultureInfo),
            int intValue => intValue.ToString(format, cultureInfo),
            long longValue => longValue.ToString(format, cultureInfo),
            short shortValue => shortValue.ToString(format, cultureInfo),
            IFormattable formattable => formattable.ToString(format, cultureInfo),
            _ => value.ToString() ?? string.Empty
        };
    }
}