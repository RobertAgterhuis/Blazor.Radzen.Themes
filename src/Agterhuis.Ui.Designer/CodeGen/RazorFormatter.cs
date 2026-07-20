namespace Agterhuis.Ui.Designer.CodeGen;

/// <summary>
/// Handles indentation and formatting of generated Razor code.
/// </summary>
public sealed class RazorFormatter
{
    private readonly string _indentString;

    public RazorFormatter(int indentSize = 4)
    {
        _indentString = new string(' ', indentSize);
    }

    /// <summary>
    /// Indents a line by the given level.
    /// </summary>
    public string Indent(string text, int level) => level > 0
        ? string.Concat(Enumerable.Repeat(_indentString, level)) + text
        : text;

    /// <summary>
    /// Formats a parameter line, ensuring consistent spacing.
    /// </summary>
    public string FormatParameter(string name, string value)
    {
        // Razor parameters are typically in the form: @ComponentAttribute="value"
        return $"@{name}=\"{value}\"";
    }

    /// <summary>
    /// Formats a parameter with no value (for boolean true or render fragments).
    /// </summary>
    public string FormatParameterNoValue(string name) => $"@{name}";

    /// <summary>
    /// Normalizes line endings and removes trailing whitespace.
    /// </summary>
    public string Normalize(string text)
    {
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        return string.Join(
            Environment.NewLine,
            lines.Select(line => line.TrimEnd()));
    }
}
