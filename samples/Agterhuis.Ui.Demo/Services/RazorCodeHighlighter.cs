using System.Net;
using System.Text.RegularExpressions;

namespace Agterhuis.Ui.Demo.Services;

public static partial class RazorCodeHighlighter
{
    public static string Highlight(string source)
    {
        var encoded = WebUtility.HtmlEncode(source ?? string.Empty);

        encoded = TagPattern().Replace(encoded, "<span class=\"demo-code__tag\">$0</span>");
        encoded = AttributePattern().Replace(encoded, " <span class=\"demo-code__attr\">$1</span>=<span class=\"demo-code__value\">$2</span>");
        encoded = RazorDirectivePattern().Replace(encoded, "<span class=\"demo-code__directive\">$0</span>");
        encoded = StringPattern().Replace(encoded, "<span class=\"demo-code__string\">$0</span>");

        return encoded;
    }

    [GeneratedRegex("&lt;/?[A-Za-z][A-Za-z0-9\\.\\-]*")]
    private static partial Regex TagPattern();

    [GeneratedRegex("\\s([A-Za-z_:][A-Za-z0-9_:\\.-]*)=(\"[^\"]*\")")]
    private static partial Regex AttributePattern();

    [GeneratedRegex("(?m)^\\s*@\\w[^\\r\\n]*")]
    private static partial Regex RazorDirectivePattern();

    [GeneratedRegex("\"[^\"]*\"")]
    private static partial Regex StringPattern();
}
