namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignPage
{
    public string Route { get; set; } = "/";

    public string Title { get; set; } = string.Empty;

    public List<DesignNode> Nodes { get; set; } = [];
}