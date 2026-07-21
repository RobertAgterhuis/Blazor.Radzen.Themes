using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class DuplicatePageCommand : IDesignDocumentCommand
{
    public DuplicatePageCommand(int sourcePageIndex, string route, string? title = null)
    {
        if (sourcePageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourcePageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        SourcePageIndex = sourcePageIndex;
        Route = route.Trim();
        Title = title?.Trim();
    }

    public int SourcePageIndex { get; }

    public string Route { get; }

    public string? Title { get; }

    public string Name => "Duplicate page";

    public bool Apply(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (SourcePageIndex >= document.Pages.Count)
        {
            return false;
        }

        var sourcePage = document.Pages[SourcePageIndex];
        var clone = DesignDocumentSerializer.Deserialize(DesignDocumentSerializer.Serialize(new DesignDocument
        {
            Pages = [sourcePage]
        })).Pages.First();

        clone.Route = string.IsNullOrWhiteSpace(Route) ? "/" : Route;
        clone.Title = string.IsNullOrWhiteSpace(Title) ? sourcePage.Title : Title;

        document.Pages.Insert(SourcePageIndex + 1, clone);
        DesignDocumentMigrator.Migrate(document);
        return true;
    }
}
