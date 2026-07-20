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
        document.DataModel = document.DataModel is null
            ? DesignDataModelSeeder.CreateDefault()
            : Migrate(document.DataModel);

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

    private static DesignDataModel Migrate(DesignDataModel dataModel)
    {
        dataModel.Entities ??= [];

        if (dataModel.Seed <= 0)
        {
            dataModel.Seed = 42;
        }

        if (dataModel.RowCount <= 0)
        {
            dataModel.RowCount = 25;
        }

        foreach (var entity in dataModel.Entities)
        {
            entity.Fields ??= [];
            entity.Seed ??= new DesignSeedSettings();

            if (string.IsNullOrWhiteSpace(entity.PluralName))
            {
                entity.PluralName = $"{entity.Name}s";
            }

            if (entity.Seed.Seed <= 0)
            {
                entity.Seed.Seed = dataModel.Seed;
            }

            if (entity.Seed.RowCount <= 0)
            {
                entity.Seed.RowCount = dataModel.RowCount;
            }
        }

        return dataModel;
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