using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class AddPageCommand : IDesignDocumentCommand
{
    public AddPageCommand(DesignPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Page = page;
    }

    public DesignPage Page { get; }

    public string Name => "Add page";

    public bool Apply(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.Pages.Add(new DesignPage
        {
            Route = string.IsNullOrWhiteSpace(Page.Route) ? "/" : Page.Route,
            Title = string.IsNullOrWhiteSpace(Page.Title) ? "Nieuwe pagina" : Page.Title,
            Nodes = Page.Nodes.Select(static node => node).ToList()
        });

        DesignDocumentMigrator.Migrate(document);
        return true;
    }
}