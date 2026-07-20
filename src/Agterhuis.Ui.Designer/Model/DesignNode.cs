namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignNode
{
    public string Id { get; set; } = string.Empty;

    public string ComponentType { get; set; } = string.Empty;

    public Dictionary<string, DesignParameterValue> Parameters { get; set; } = new(StringComparer.Ordinal);

    public Dictionary<string, List<DesignNode>> Children { get; set; } = new(StringComparer.Ordinal);

    public DesignLayoutSlot? LayoutSlot { get; set; }
}