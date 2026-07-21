namespace Agterhuis.Ui.Designer.Registry;

public sealed record DesignerComponentDescriptor(
    string ComponentType,
    Type ClrType,
    string DisplayName,
    string Category,
    string Icon,
    bool AllowedInPalette,
    bool IsWrapper,
    bool IsDeprecated,
    IReadOnlyList<string> Slots,
    IReadOnlyList<ComponentParameterDescriptor> Parameters,
    string? DesignerDisplayName = null,
    string? DesignerCategory = null,
    string? DesignerDescription = null,
    string? DesignerIcon = null);