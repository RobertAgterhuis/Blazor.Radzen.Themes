namespace Agterhuis.Ui.Designer.Model;

public sealed record DesignSeedRow(string EntityName, IReadOnlyDictionary<string, object?> Values);