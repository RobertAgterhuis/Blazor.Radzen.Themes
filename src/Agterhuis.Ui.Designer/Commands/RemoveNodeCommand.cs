using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class RemoveNodeCommand : IDesignDocumentCommand
{
    public RemoveNodeCommand(int pageIndex, string nodeId)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        PageIndex = pageIndex;
        NodeId = nodeId;
    }

    public int PageIndex { get; }

    public string NodeId { get; }

    public string Name => "Remove";

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

        match.Container.RemoveAt(match.Index);
        return true;
    }
}
