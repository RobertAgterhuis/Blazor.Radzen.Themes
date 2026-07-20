namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignEntity
{
    public string Name { get; set; } = string.Empty;

    public string PluralName { get; set; } = string.Empty;

    public List<DesignField> Fields { get; set; } = [];

    public DesignSeedSettings Seed { get; set; } = new();
}