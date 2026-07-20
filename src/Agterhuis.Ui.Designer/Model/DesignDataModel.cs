namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignDataModel
{
    public List<DesignEntity> Entities { get; set; } = [];

    public int Seed { get; set; } = 42;

    public int RowCount { get; set; } = 25;

    public static DesignDataModel CreateDefault() => DesignDataModelSeeder.CreateDefault();
}