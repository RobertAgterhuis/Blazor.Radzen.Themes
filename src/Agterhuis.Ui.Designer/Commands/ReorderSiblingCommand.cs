using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class ReorderSiblingCommand : IDesignDocumentCommand
{
    public ReorderSiblingCommand(int pageIndex, string nodeId, int delta)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (delta == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(delta));
        }

        PageIndex = pageIndex;
        NodeId = nodeId;
        Delta = delta;
    }

    public int PageIndex { get; }

    public string NodeId { get; }

    public int Delta { get; }

    public string Name => "Reorder";

    public bool Apply(DesignDocument document)
    {
        if (PageIndex >= document.Pages.Count)
        {
            return false;
        }

        var page = document.Pages[PageIndex];
        if (!DesignNodeQuery.TryFindNode(page, NodeId, out var current) || current is null)
        {
            return false;
        }

        var nextIndex = current.Index + Delta;
        if (nextIndex < 0 || nextIndex >= current.Container.Count)
        {
            return false;
        }

        current.Container.RemoveAt(current.Index);
        current.Container.Insert(nextIndex, current.Node);
        return true;
    }
}
