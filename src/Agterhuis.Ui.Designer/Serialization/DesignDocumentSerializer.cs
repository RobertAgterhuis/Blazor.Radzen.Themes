using System.Text.Json;
using Agterhuis.Ui.Designer.Model;
using System.Linq;

namespace Agterhuis.Ui.Designer.Serialization;

public static class DesignDocumentSerializer
{
    public static string Serialize(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        DesignDocumentMigrator.Migrate(document);
        return JsonSerializer.Serialize(document, DesignJsonOptions.Default);
    }

    public static DesignDocument Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var document = JsonSerializer.Deserialize<DesignDocument>(json, DesignJsonOptions.Default)
            ?? new DesignDocument();

        return DesignDocumentMigrator.Migrate(document);
    }

    public static string SerializeNode(DesignNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var wrapper = new DesignDocument
        {
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "clipboard",
                    Nodes = [node]
                }
            ]
        };

        return Serialize(wrapper);
    }

    public static DesignNode? DeserializeNode(string json)
    {
        var doc = Deserialize(json);
        return doc.Pages.FirstOrDefault()?.Nodes.FirstOrDefault();
    }
}