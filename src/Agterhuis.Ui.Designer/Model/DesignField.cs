namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignField
{
    public string Name { get; set; } = string.Empty;

    public DesignFieldType Type { get; set; } = DesignFieldType.String;

    public bool IsRequired { get; set; }

    public string? Pattern { get; set; }

    public List<string> EnumValues { get; set; } = [];
}