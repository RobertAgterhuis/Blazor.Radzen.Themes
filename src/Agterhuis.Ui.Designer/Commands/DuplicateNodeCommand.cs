using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class DuplicateNodeCommand : IDesignDocumentCommand
{
    public DuplicateNodeCommand(int pageIndex, string sourceNodeId, DesignNodeLocation target)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceNodeId);
        ArgumentNullException.ThrowIfNull(target);

        PageIndex = pageIndex;
        SourceNodeId = sourceNodeId;
        Target = target;
    }

    public int PageIndex { get; }

    public string SourceNodeId { get; }

    public DesignNodeLocation Target { get; }

    public string Name => "Duplicate";

    public bool Apply(DesignDocument document)
    {
        if (PageIndex >= document.Pages.Count)
        {
            return false;
        }

        var page = document.Pages[PageIndex];
        if (!DesignNodeQuery.TryFindNode(page, SourceNodeId, out var source) || source is null)
        {
            return false;
        }

        if (!DesignNodeQuery.TryResolveContainer(page, Target, out var targetContainer) || targetContainer is null)
        {
            return false;
        }

        var clone = DeepClone(source.Node);
        var insertIndex = Math.Clamp(Target.Index, 0, targetContainer.Count);
        targetContainer.Insert(insertIndex, clone);
        DesignDocumentMigrator.Migrate(document);
        return true;
    }

    private static DesignNode DeepClone(DesignNode node)
    {
        var tempDocument = new DesignDocument
        {
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "clone",
                    Nodes = [node]
                }
            ]
        };

        var cloned = DesignDocumentSerializer.Deserialize(DesignDocumentSerializer.Serialize(tempDocument));
        return cloned.Pages.First().Nodes.First();
    }
}
