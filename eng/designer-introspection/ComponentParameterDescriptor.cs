namespace Agterhuis.Ui.Designer.Introspection;

public sealed record ComponentParameterDescriptor(
    string Name,
    string TypeName,
    Type ParameterType,
    string DefaultValue,
    string Description,
    bool IsBindable,
    bool IsEditorRequired,
    bool IsEventCallback,
    bool IsRenderFragment,
    bool IsTemplatedRenderFragment);
