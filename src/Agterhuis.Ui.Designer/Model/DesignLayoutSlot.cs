namespace Agterhuis.Ui.Designer.Model;

public sealed record DesignLayoutSlot(
    int Row = 1,
    int Column = 1,
    int RowSpan = 1,
    int ColumnSpan = 1);