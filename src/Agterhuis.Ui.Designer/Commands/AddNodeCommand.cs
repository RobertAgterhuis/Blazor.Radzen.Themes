using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class AddNodeCommand : IDesignDocumentCommand
{
    public AddNodeCommand(int pageIndex, DesignNodeLocation location, DesignNode node)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(node);

        PageIndex = pageIndex;
        Location = location;
        Node = node;
    }

    public int PageIndex { get; }

    public DesignNodeLocation Location { get; }

    public DesignNode Node { get; }

    public string Name => "Add";

    public bool Apply(DesignDocument document)
    {
        if (PageIndex >= document.Pages.Count)
        {
            return false;
        }

        var page = document.Pages[PageIndex];
        if (!DesignNodeQuery.TryResolveContainer(page, Location, out var container) || container is null)
        {
            return false;
        }

        var cloned = DeepClone(Node);
        var insertIndex = Math.Clamp(Location.Index, 0, container.Count);
        container.Insert(insertIndex, cloned);
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
