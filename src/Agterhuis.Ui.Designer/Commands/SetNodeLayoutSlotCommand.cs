using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class SetNodeLayoutSlotCommand : IDesignDocumentCommand
{
    public SetNodeLayoutSlotCommand(int pageIndex, string nodeId, DesignLayoutSlot? layoutSlot)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        PageIndex = pageIndex;
        NodeId = nodeId;
        LayoutSlot = layoutSlot;
    }

    public int PageIndex { get; }

    public string NodeId { get; }

    public DesignLayoutSlot? LayoutSlot { get; }

    public string Name => "Set layout slot";

    public bool Apply(DesignDocument document)
    {
        if (PageIndex >= document.Pages.Count)
        {
            return false;
        }

        var page = document.Pages[PageIndex];
        if (!DesignNodeQuery.TryFindNode(page, NodeId, out var match) || match is null)
        {
            return false;
        }

        match.Node.LayoutSlot = LayoutSlot is null
            ? null
            : new DesignLayoutSlot(LayoutSlot.Row, LayoutSlot.Column, LayoutSlot.RowSpan, LayoutSlot.ColumnSpan);

        return true;
    }
}