using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class MoveNodeCommand : IDesignDocumentCommand
{
    public MoveNodeCommand(int pageIndex, string nodeId, DesignNodeLocation target)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentNullException.ThrowIfNull(target);

        PageIndex = pageIndex;
        NodeId = nodeId;
        Target = target;
    }

    public int PageIndex { get; }

    public string NodeId { get; }

    public DesignNodeLocation Target { get; }

    public string Name => "Move";

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

        if (Target.ParentNodeId is not null)
        {
            if (string.Equals(Target.ParentNodeId, NodeId, StringComparison.Ordinal))
            {
                return false;
            }

            if (DesignNodeQuery.IsDescendant(current.Node, Target.ParentNodeId))
            {
                return false;
            }
        }

        current.Container.RemoveAt(current.Index);

        if (!DesignNodeQuery.TryResolveContainer(page, Target, out var targetContainer) || targetContainer is null)
        {
            return false;
        }

        var insertIndex = Math.Clamp(Target.Index, 0, targetContainer.Count);

        if (ReferenceEquals(current.Container, targetContainer) && current.Index < insertIndex)
        {
            insertIndex--;
        }

        targetContainer.Insert(insertIndex, current.Node);
        return true;
    }
}
