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

    [Fact]
    public void SetParameterLayoutAndPageProperty_AreUndoable()
    {
        var stack = new DesignDocumentCommandStack(CreateDocument());
        var nodeId = stack.Document.Pages[0].Nodes[0].Id;

        Assert.True(stack.Execute(new SetNodeParameterCommand(0, nodeId, "Label", DesignParameterValue.FromValue("Naam"))));
        Assert.Equal("Naam", stack.Document.Pages[0].Nodes[0].Parameters["Label"].Literal!.GetValue<string>());

        Assert.True(stack.Execute(new SetNodeLayoutSlotCommand(0, nodeId, new DesignLayoutSlot(Row: 2, Column: 3, RowSpan: 1, ColumnSpan: 2))));
        Assert.NotNull(stack.Document.Pages[0].Nodes[0].LayoutSlot);
        Assert.Equal(2, stack.Document.Pages[0].Nodes[0].LayoutSlot!.Row);

        Assert.True(stack.Execute(new SetPagePropertyCommand(0, nameof(DesignPage.Title), "Gewijzigde pagina")));
        Assert.Equal("Gewijzigde pagina", stack.Document.Pages[0].Title);

        Assert.True(stack.Undo());
        Assert.Equal("Test", stack.Document.Pages[0].Title);

        Assert.True(stack.Undo());
        Assert.Null(stack.Document.Pages[0].Nodes[0].LayoutSlot);

        Assert.True(stack.Undo());
        Assert.False(stack.Document.Pages[0].Nodes[0].Parameters.ContainsKey("Label"));
    }

    [Fact]
    public void AddRemoveDuplicateRenameReorderPage_AreUndoable()
    {
        var stack = new DesignDocumentCommandStack(CreateDocument());

        var addPage = new DesignPage
        {
            Route = "/page-2",
            Title = "Page 2",
            Nodes =
            [
                new DesignNode
                {
                    ComponentType = "AgtCard",
                    Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                }
            ]
        };

        Assert.True(stack.Execute(new AddPageCommand(addPage)));
        Assert.Equal(2, stack.Document.Pages.Count);

        Assert.True(stack.Execute(new RenamePageCommand(1, "Hernoemd")));
        Assert.Equal("Hernoemd", stack.Document.Pages[1].Title);

        Assert.True(stack.Execute(new DuplicatePageCommand(1, "/page-3", "Kopie")));
        Assert.Equal(3, stack.Document.Pages.Count);
        Assert.Equal("Kopie", stack.Document.Pages[2].Title);

        Assert.True(stack.Execute(new ReorderPageCommand(2, 0)));
        Assert.Equal("Kopie", stack.Document.Pages[0].Title);

        Assert.True(stack.Execute(new RemovePageCommand(0)));
        Assert.Equal(2, stack.Document.Pages.Count);

        Assert.True(stack.Undo());
        Assert.Equal(3, stack.Document.Pages.Count);

        Assert.True(stack.Undo());
        Assert.Equal("Test", stack.Document.Pages[0].Title);

        Assert.True(stack.Undo());
        Assert.Equal(2, stack.Document.Pages.Count);

        Assert.True(stack.Undo());
        Assert.Equal("Page 2", stack.Document.Pages[1].Title);

        Assert.True(stack.Undo());
        Assert.Single(stack.Document.Pages);
    }

    [Fact]
    public void ImportEntitiesCommand_AddsAndSupportsUndo()
    {
        var stack = new DesignDocumentCommandStack(CreateDocument());
        var initialCount = stack.Document.DataModel.Entities.Count;

        var imported = new DesignEntity
        {
            Name = "Klant",
            PluralName = "Klanten",
            Fields =
            [
                new DesignField { Name = "Klantnaam", Type = DesignFieldType.String, IsRequired = true }
            ]
        };

        Assert.True(stack.Execute(new ImportEntitiesCommand([imported], new SchemaImportApplyOptions { ConflictResolution = SchemaImportConflictResolution.Rename })));
        Assert.Equal(initialCount + 1, stack.Document.DataModel.Entities.Count);
        Assert.Contains(stack.Document.DataModel.Entities, entity => entity.Name == "Klant");

        Assert.True(stack.Undo());
        Assert.Equal(initialCount, stack.Document.DataModel.Entities.Count);
    }

    [Fact]
    public void ImportEntitiesCommand_RenameConflict_CreatesUniqueName()
    {
        var stack = new DesignDocumentCommandStack(CreateDocument());
        var initialCount = stack.Document.DataModel.Entities.Count;

        var imported = new DesignEntity
        {
            Name = "Klant",
            PluralName = "Klanten",
            Fields =
            [
                new DesignField { Name = "Klantnaam", Type = DesignFieldType.String, IsRequired = true }
            ]
        };

        Assert.True(stack.Execute(new ImportEntitiesCommand([imported], new SchemaImportApplyOptions { ConflictResolution = SchemaImportConflictResolution.Rename })));
        Assert.Equal(initialCount + 1, stack.Document.DataModel.Entities.Count);
        Assert.Contains(stack.Document.DataModel.Entities, entity => entity.Name == "Klant_2");
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
