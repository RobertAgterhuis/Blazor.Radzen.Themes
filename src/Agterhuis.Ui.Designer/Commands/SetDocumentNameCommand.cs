using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class SetDocumentNameCommand : IDesignDocumentCommand
{
    public SetDocumentNameCommand(string? name)
    {
        NextName = name?.Trim();
    }

    public string? NextName { get; }

    public string Name => "Rename document";

    public bool Apply(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(NextName) || string.Equals(NextName, document.Name, StringComparison.Ordinal))
        {
            return false;
        }

        document.Name = NextName;
        return true;
    }
}