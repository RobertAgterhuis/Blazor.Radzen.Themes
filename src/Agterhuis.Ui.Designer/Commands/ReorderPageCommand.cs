using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class ReorderPageCommand : IDesignDocumentCommand
{
    public ReorderPageCommand(int sourcePageIndex, int targetPageIndex)
    {
        if (sourcePageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourcePageIndex));
        }

        if (targetPageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetPageIndex));
        }

        SourcePageIndex = sourcePageIndex;
        TargetPageIndex = targetPageIndex;
    }

    public int SourcePageIndex { get; }

    public int TargetPageIndex { get; }

    public string Name => "Reorder page";

    public bool Apply(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (SourcePageIndex >= document.Pages.Count || TargetPageIndex >= document.Pages.Count || SourcePageIndex == TargetPageIndex)
        {
            return false;
        }

        var page = document.Pages[SourcePageIndex];
        document.Pages.RemoveAt(SourcePageIndex);

        var insertIndex = Math.Clamp(TargetPageIndex, 0, document.Pages.Count);
        document.Pages.Insert(insertIndex, page);
        DesignDocumentMigrator.Migrate(document);
        return true;
    }
}
