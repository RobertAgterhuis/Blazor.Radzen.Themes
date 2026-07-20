using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Tests;

public sealed class DesignDocumentDataModelTests
{
    [Fact]
    public void Migrate_InitializesTheDefaultDataModel()
    {
        var document = new DesignDocument
        {
            Name = "Demo",
            Pages = [new DesignPage { Route = "/", Title = "Home" }]
        };

        var migrated = DesignDocumentMigrator.Migrate(document);

        Assert.Equal(DesignDocument.CurrentSchemaVersion, migrated.SchemaVersion);
        Assert.NotNull(migrated.DataModel);
        Assert.Contains(migrated.DataModel.Entities, entity => entity.Name == "Schadedossier");
    }

    [Fact]
    public void SerializeRoundtrip_PreservesTheDataModelShape()
    {
        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");

        var json = DesignDocumentSerializer.Serialize(document);
        var roundtrip = DesignDocumentSerializer.Deserialize(json);

        Assert.NotNull(roundtrip.DataModel);
        Assert.Equal(document.DataModel.Entities.Select(entity => entity.Name), roundtrip.DataModel.Entities.Select(entity => entity.Name));
    }
}