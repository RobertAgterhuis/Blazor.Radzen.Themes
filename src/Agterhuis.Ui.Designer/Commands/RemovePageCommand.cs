using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class RemovePageCommand : IDesignDocumentCommand
{
    public RemovePageCommand(int pageIndex)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        PageIndex = pageIndex;
    }

    public int PageIndex { get; }

    public string Name => "Remove page";

    public bool Apply(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Pages.Count <= 1 || PageIndex >= document.Pages.Count)
        {
            return false;
        }

        document.Pages.RemoveAt(PageIndex);
        DesignDocumentMigrator.Migrate(document);
        return true;
    }
}
