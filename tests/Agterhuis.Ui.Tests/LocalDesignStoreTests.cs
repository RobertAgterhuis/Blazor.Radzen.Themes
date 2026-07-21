using Agterhuis.Ui.Demo.Services;
using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Tests;

public sealed class LocalDesignStoreTests
{
    [Fact]
    public async Task SaveAndLoad_Roundtrip_WorksWithNewContract()
    {
        var js = new DesignerJsRuntimeStub();
        var store = new LocalDesignStore(js);
        var document = CreateDocument("Roundtrip");

        var saved = await store.SaveAsync("Roundtrip", document, expectedETag: null);
        var loaded = await store.LoadAsync("Roundtrip");

        Assert.NotNull(loaded);
        Assert.Equal(saved.Version, loaded!.Version);
        Assert.Equal("Roundtrip", loaded.Name);
        Assert.Equal("Roundtrip", loaded.Document.Name);
    }

    [Fact]
    public async Task LoadAsync_BackwardCompatible_WithLegacyJson()
    {
        var js = new DesignerJsRuntimeStub();
        var legacyDocument = CreateDocument("Legacy");
        var legacyJson = Agterhuis.Ui.Designer.Serialization.DesignDocumentSerializer.Serialize(legacyDocument);
        js.SetResult("designerInterop.getText", legacyJson);

        var store = new LocalDesignStore(js);
        var loaded = await store.LoadAsync("Legacy");

        Assert.NotNull(loaded);
        Assert.Equal("Legacy", loaded!.Name);
        Assert.Equal("Legacy", loaded.Document.Name);
    }

    private static DesignDocument CreateDocument(string name)
    {
        return new DesignDocument
        {
            Name = name,
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = name,
                    Nodes = []
                }
            ]
        };
    }
}
