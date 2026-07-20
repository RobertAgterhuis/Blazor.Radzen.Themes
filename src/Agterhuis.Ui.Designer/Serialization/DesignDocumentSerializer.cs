using System.Text.Json;
using Agterhuis.Ui.Designer.Model;

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
}