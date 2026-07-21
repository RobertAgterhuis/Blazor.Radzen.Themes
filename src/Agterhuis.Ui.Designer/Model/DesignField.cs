namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignField
{
    public string Name { get; set; } = string.Empty;

    public string? DisplayLabel { get; set; }

    public DesignFieldType Type { get; set; } = DesignFieldType.String;

    public bool IsRequired { get; set; }

    public bool IsForeignKey { get; set; }

    public string? ReferenceEntityName { get; set; }

    public string? Pattern { get; set; }

    public List<string> EnumValues { get; set; } = [];
}