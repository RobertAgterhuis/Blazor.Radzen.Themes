namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignDocument
{
    public const int CurrentSchemaVersion = 2;

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0";

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public List<DesignPage> Pages { get; set; } = [];

    public DesignDataModel DataModel { get; set; } = DesignDataModelSeeder.CreateDefault();
}