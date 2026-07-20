namespace Agterhuis.Ui.Designer.Commands;

public sealed record DesignNodeLocation(string? ParentNodeId, string SlotName, int Index)
{
    public static readonly string RootSlotName = "$root";

    public static DesignNodeLocation Root(int index)
        => new(null, RootSlotName, index);
}
