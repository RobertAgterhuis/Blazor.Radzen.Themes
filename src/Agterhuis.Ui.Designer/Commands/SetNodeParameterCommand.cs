using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class SetNodeParameterCommand : IDesignDocumentCommand
{
    public SetNodeParameterCommand(int pageIndex, string nodeId, string parameterName, DesignParameterValue? value)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        PageIndex = pageIndex;
        NodeId = nodeId;
        ParameterName = parameterName;
        Value = value;
    }

    public int PageIndex { get; }

    public string NodeId { get; }

    public string ParameterName { get; }

    public DesignParameterValue? Value { get; }

    public string Name => "Set parameter";

    public bool Apply(DesignDocument document)
    {
        if (PageIndex >= document.Pages.Count)
        {
            return false;
        }

        var page = document.Pages[PageIndex];
        if (!DesignNodeQuery.TryFindNode(page, NodeId, out var match) || match is null)
        {
            return false;
        }

        match.Node.Parameters ??= new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal);

        if (Value is null)
        {
            return match.Node.Parameters.Remove(ParameterName);
        }

        match.Node.Parameters[ParameterName] = Clone(Value);
        return true;
    }

    private static DesignParameterValue Clone(DesignParameterValue value)
    {
        return new DesignParameterValue
        {
            Literal = value.Literal?.DeepClone(),
            Expression = value.Expression
        };
    }
}