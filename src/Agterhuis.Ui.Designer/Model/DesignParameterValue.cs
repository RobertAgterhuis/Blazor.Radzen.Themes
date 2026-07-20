using System.Text.Json;
using System.Text.Json.Nodes;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignParameterValue
{
    public JsonNode? Literal { get; set; }

    public string? Expression { get; set; }

    public static DesignParameterValue FromValue<T>(T value)
    {
        return new DesignParameterValue
        {
            Literal = value is null
                ? null
                : JsonSerializer.SerializeToNode(value, DesignJsonOptions.Default)
        };
    }
}