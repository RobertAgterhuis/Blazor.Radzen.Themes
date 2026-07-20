using Agterhuis.Ui.Designer.Commands;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Tests;

public sealed class DesignCommandStackTests
{
    [Fact]
    public void AddMoveRemove_UndoRedo_PreserveInvariants()
    {
        var document = CreateDocument();
        var stack = new DesignDocumentCommandStack(document);

        var addNode = NewNode("AgtCard");
        Assert.True(stack.Execute(new AddNodeCommand(0, DesignNodeLocation.Root(1), addNode)));
        Assert.Contains(stack.Document.Pages[0].Nodes, node => string.Equals(node.ComponentType, "AgtCard", StringComparison.Ordinal));

        var moveTarget = new DesignNodeLocation(stack.Document.Pages[0].Nodes[0].Id, "ChildContent", 0);
        Assert.True(stack.Execute(new MoveNodeCommand(0, addNode.Id, moveTarget)));

        var parent = stack.Document.Pages[0].Nodes[0];
        Assert.True(parent.Children.TryGetValue("ChildContent", out var movedChildren));
        Assert.NotNull(movedChildren);
        Assert.Contains(movedChildren, node => string.Equals(node.Id, addNode.Id, StringComparison.Ordinal));

        Assert.True(stack.Execute(new RemoveNodeCommand(0, addNode.Id)));
        Assert.DoesNotContain(stack.Document.Pages[0].Nodes.SelectMany(static node => node.Children.Values.SelectMany(static childList => childList)), node => string.Equals(node.Id, addNode.Id, StringComparison.Ordinal));

        Assert.True(stack.Undo());
        Assert.Contains(parent.Children["ChildContent"], node => string.Equals(node.Id, addNode.Id, StringComparison.Ordinal));

        Assert.True(stack.Undo());
        Assert.Contains(stack.Document.Pages[0].Nodes, node => string.Equals(node.Id, addNode.Id, StringComparison.Ordinal));

        Assert.True(stack.Undo());
        Assert.DoesNotContain(stack.Document.Pages[0].Nodes, node => string.Equals(node.Id, addNode.Id, StringComparison.Ordinal));

        Assert.True(stack.Redo());
        Assert.True(stack.Redo());
        Assert.True(stack.Redo());
    }

    [Fact]
    public void DuplicateAndReorder_WorkAndAreUndoable()
    {
        var stack = new DesignDocumentCommandStack(CreateDocument());
        var firstNodeId = stack.Document.Pages[0].Nodes[0].Id;

        Assert.True(stack.Execute(new DuplicateNodeCommand(0, firstNodeId, DesignNodeLocation.Root(1))));
        Assert.Equal(2, stack.Document.Pages[0].Nodes.Count);

        Assert.True(stack.Undo());
        Assert.Single(stack.Document.Pages[0].Nodes);

        var reorderStack = new DesignDocumentCommandStack(CreateReorderDocument());
        var lowerNodeId = reorderStack.Document.Pages[0].Nodes[1].Id;

        Assert.True(reorderStack.Execute(new ReorderSiblingCommand(0, lowerNodeId, -1)));
        Assert.Equal("AgtCard", reorderStack.Document.Pages[0].Nodes[0].ComponentType);

        Assert.True(reorderStack.Undo());
        Assert.Equal("RadzenColumn", reorderStack.Document.Pages[0].Nodes[0].ComponentType);
    }

    private static DesignDocument CreateReorderDocument()
    {
        return DesignDocumentMigrator.Migrate(new DesignDocument
        {
            Name = "reorder",
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "Reorder",
                    Nodes =
                    [
                        new DesignNode { ComponentType = "RadzenColumn", Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal) },
                        new DesignNode { ComponentType = "AgtCard", Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal) }
                    ]
                }
            ]
        });
    }

    private static DesignDocument CreateDocument()
    {
        var root = new DesignNode
        {
            ComponentType = "RadzenColumn",
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["ChildContent"] = []
            }
        };

        return DesignDocumentMigrator.Migrate(new DesignDocument
        {
            Name = "test",
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "Test",
                    Nodes = [root]
                }
            ]
        });
    }

    private static DesignNode NewNode(string componentType)
    {
        return DesignDocumentMigrator.Migrate(new DesignDocument
        {
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "Node",
                    Nodes =
                    [
                        new DesignNode
                        {
                            ComponentType = componentType,
                            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                        }
                    ]
                }
            ]
        }).Pages[0].Nodes[0];
    }
}
