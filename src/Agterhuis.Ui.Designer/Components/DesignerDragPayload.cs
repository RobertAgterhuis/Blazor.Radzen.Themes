using Agterhuis.Ui.Designer.Commands;

namespace Agterhuis.Ui.Designer.Components;

public sealed record DesignerDragPayload(string Kind, string Value)
{
    public static DesignerDragPayload Palette(string componentType) => new("palette", componentType);

    public static DesignerDragPayload Node(string nodeId) => new("node", nodeId);
}