using System.Text.RegularExpressions;
using Agterhuis.Ui.Designer.Commands;
using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Demo.Services;

/// <summary>
/// Parses generated Razor code back into DesignNode mutations.
/// Recognizes component tags like &lt;AgtTextField @bind-Value="..." Label="..." /&gt;
/// and maps them back to design model operations.
/// </summary>
public partial class RazorCodeParser
{
    /// <summary>
    /// Attempts to parse Razor code and return mutations to sync with the canvas.
    /// </summary>
    public ParseResult TryParse(string razorCode, DesignPage page)
    {
        try
        {
            var nodes = ExtractComponentNodes(razorCode);
            if (nodes.Count == 0)
            {
                return new ParseResult { Success = true, Nodes = [] };
            }

            return new ParseResult { Success = true, Nodes = nodes };
        }
        catch (Exception ex)
        {
            return new ParseResult 
            { 
                Success = false, 
                Error = ex.Message,
                ErrorLine = FindErrorLine(razorCode, ex)
            };
        }
    }

    /// <summary>
    /// Extracts component nodes from Razor markup using regex.
    /// Returns a list of (componentType, parameters, position).
    /// </summary>
    private List<(string ComponentType, Dictionary<string, string> Parameters, int Position)> ExtractComponentNodes(string razorCode)
    {
        var nodes = new List<(string, Dictionary<string, string>, int)>();

        // Match component tags like <AgtTextField ... /> or <AgtCard>...</AgtCard>
        var componentPattern = ComponentTagRegex();
        var matches = componentPattern.Matches(razorCode);

        foreach (Match match in matches)
        {
            var componentType = match.Groups["component"].Value;
            var tagContent = match.Groups["content"].Value;
            var parameters = ExtractParameters(tagContent);

            nodes.Add((componentType, parameters, match.Index));
        }

        return nodes;
    }

    /// <summary>
    /// Extracts parameter key-value pairs from a component tag.
    /// Handles @bind-*, regular attributes, and Razor expressions.
    /// </summary>
    private Dictionary<string, string> ExtractParameters(string tagContent)
    {
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);

        // Match @bind-Parameter="value" or Parameter="value" or @onclick="handler"
        var paramPattern = ParameterRegex();
        var matches = paramPattern.Matches(tagContent);

        foreach (Match match in matches)
        {
            var paramName = match.Groups["param"].Value;
            var paramValue = match.Groups["value"].Value;

            // Normalize parameter names
            if (paramName.StartsWith("@bind-", StringComparison.Ordinal))
            {
                paramName = paramName.Substring("@bind-".Length);
                paramName = ToPascalCase(paramName);
            }
            else if (paramName.StartsWith("@", StringComparison.Ordinal))
            {
                paramName = paramName.Substring(1);
            }

            parameters[paramName] = paramValue;
        }

        return parameters;
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }

    private static int FindErrorLine(string razorCode, Exception ex)
    {
        // Try to extract line number from exception message
        var lineMatch = Regex.Match(ex.Message, @"line (\d+)", RegexOptions.IgnoreCase);
        if (lineMatch.Success && int.TryParse(lineMatch.Groups[1].Value, out var lineNum))
        {
            return lineNum;
        }

        return 0;
    }

    [GeneratedRegex(@"<(?<component>Agt\w+)\s+(?<content>[\s\S]*?)(?:\/>|>[\s\S]*?</\w+>)", RegexOptions.IgnoreCase)]
    private static partial Regex ComponentTagRegex();

    [GeneratedRegex(@"(?<param>@?\w+(?:-\w+)*)\s*=\s*[""'](?<value>[^""']*)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ParameterRegex();

    public class ParseResult
    {
        public bool Success { get; set; }
        public List<(string ComponentType, Dictionary<string, string> Parameters, int Position)> Nodes { get; set; } = [];
        public string? Error { get; set; }
        public int ErrorLine { get; set; }
    }
}
