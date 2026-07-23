using System.Text.Json;
using System.Text.Json.Nodes;
using Agterhuis.Ui.Designer.Model;
using System.Linq;

namespace Agterhuis.Ui.Designer.Serialization;

public static class DesignDocumentSerializer
{
    public static string Serialize(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        DesignDocumentMigrator.Migrate(document);

        var jsonNode = JsonSerializer.SerializeToNode(document, DesignJsonOptions.Default)
            ?? throw new InvalidOperationException("Design document serialization produced an empty payload.");
        StripEmptyNodeMaps(jsonNode);

        return jsonNode.ToJsonString(DesignJsonOptions.Default);
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

    private static void StripEmptyNodeMaps(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            if (jsonObject.TryGetPropertyValue("inlineStyles", out var inlineStylesNode)
                && inlineStylesNode is JsonObject inlineStylesObject
                && inlineStylesObject.Count == 0)
            {
                jsonObject.Remove("inlineStyles");
            }

            if (jsonObject.TryGetPropertyValue("customAttributes", out var customAttributesNode)
                && customAttributesNode is JsonObject customAttributesObject
                && customAttributesObject.Count == 0)
            {
                jsonObject.Remove("customAttributes");
            }

            foreach (var child in jsonObject)
            {
                if (child.Value is not null)
                {
                    StripEmptyNodeMaps(child.Value);
                }
            }

            return;
        }

        if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    StripEmptyNodeMaps(item);
                }
            }
        }
    }
}