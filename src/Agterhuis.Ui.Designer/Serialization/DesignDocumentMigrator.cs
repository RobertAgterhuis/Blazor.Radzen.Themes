using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Serialization;

public static class DesignDocumentMigrator
{
    public static DesignDocument Migrate(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.SchemaVersion <= 0)
        {
            document.SchemaVersion = DesignDocument.CurrentSchemaVersion;
        }

        if (string.IsNullOrWhiteSpace(document.Version))
        {
            document.Version = "1.0";
        }

        document.Pages ??= [];

        for (var pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
        {
            var page = document.Pages[pageIndex] ?? new DesignPage();
            document.Pages[pageIndex] = page;
            page.Nodes ??= [];

            for (var nodeIndex = 0; nodeIndex < page.Nodes.Count; nodeIndex++)
            {
                NormalizeNode(page.Route, page.Nodes[nodeIndex], $"Nodes[{nodeIndex}]");
            }
        }

        return document;
    }

    private static void NormalizeNode(string route, DesignNode? node, string ancestry)
    {
        if (node is null)
        {
            return;
        }

        node.Parameters ??= new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal);
        node.Children ??= new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(node.Id))
        {
            node.Id = DesignNodeIdFactory.Create(route, ancestry, node.ComponentType);
        }

        foreach (var slot in node.Children.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            var children = slot.Value ?? [];
            node.Children[slot.Key] = children;

            for (var index = 0; index < children.Count; index++)
            {
                NormalizeNode(route, children[index], $"{ancestry}/Children[{slot.Key}][{index}]");
            }
        }
    }
}