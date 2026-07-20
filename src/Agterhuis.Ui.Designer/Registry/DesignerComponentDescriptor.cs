namespace Agterhuis.Ui.Designer.Registry;

public sealed record DesignerComponentDescriptor(
    string ComponentType,
    Type ClrType,
    string DisplayName,
    string Category,
    string Icon,
    bool AllowedInPalette,
    bool IsWrapper,
    IReadOnlyList<string> Slots,
    IReadOnlyList<ComponentParameterDescriptor> Parameters);