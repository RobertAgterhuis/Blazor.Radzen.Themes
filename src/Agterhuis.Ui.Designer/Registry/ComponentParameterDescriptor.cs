namespace Agterhuis.Ui.Designer.Registry;

public sealed record ComponentParameterDescriptor(
    string Name,
    string TypeName,
    Type ParameterType,
    string DefaultValue,
    string Description,
    bool IsEditorRequired,
    bool IsEventCallback,
    bool IsRenderFragment,
    bool IsTemplatedRenderFragment);