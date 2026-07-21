using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class RenamePageCommand : IDesignDocumentCommand
{
    public RenamePageCommand(int pageIndex, string? title)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        PageIndex = pageIndex;
        Title = title?.Trim();
    }

    public int PageIndex { get; }

    public string? Title { get; }

    public string Name => "Rename page";

    public bool Apply(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (PageIndex >= document.Pages.Count || string.IsNullOrWhiteSpace(Title))
        {
            return false;
        }

        document.Pages[PageIndex].Title = Title;
        return true;
    }
}
