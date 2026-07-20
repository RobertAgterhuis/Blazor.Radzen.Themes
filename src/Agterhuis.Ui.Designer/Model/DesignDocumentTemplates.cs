using System.Text.Json.Nodes;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Model;

public static class DesignDocumentTemplates
{
    public sealed record TemplateDefinition(DesignDocumentTemplateKind Kind, string DisplayName, string Description);

    private static readonly IReadOnlyList<TemplateDefinition> Definitions =
    [
        new(DesignDocumentTemplateKind.Blank, "Leeg ontwerp", "Start met een basisrij en kolom."),
        new(DesignDocumentTemplateKind.FormPage, "Formulierpagina", "Een formulier met velden en acties."),
        new(DesignDocumentTemplateKind.ListCrud, "Lijst/CRUD", "Een overzicht met een zoekblok en lege-state."),
        new(DesignDocumentTemplateKind.MasterDetail, "Master-detail", "Twee-panelen met overzicht en detail."),
        new(DesignDocumentTemplateKind.Wizard, "Wizard", "Stappenstructuur voor geleide invoer."),
        new(DesignDocumentTemplateKind.Dashboard, "Dashboard", "Dashboard met kaarten en samenvatting.")
    ];

    public static IReadOnlyList<TemplateDefinition> DefinitionsList => Definitions;

    public static DesignDocument Create(DesignDocumentTemplateKind kind, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var document = kind switch
        {
            DesignDocumentTemplateKind.Blank => BuildBlank(name),
            DesignDocumentTemplateKind.FormPage => BuildFormPage(name),
            DesignDocumentTemplateKind.ListCrud => BuildListCrud(name),
            DesignDocumentTemplateKind.MasterDetail => BuildMasterDetail(name),
            DesignDocumentTemplateKind.Wizard => BuildWizard(name),
            DesignDocumentTemplateKind.Dashboard => BuildDashboard(name),
            _ => BuildBlank(name)
        };

        return DesignDocumentMigrator.Migrate(document);
    }

    public static TemplateDefinition GetDefinition(DesignDocumentTemplateKind kind)
        => Definitions.First(definition => definition.Kind == kind);

    private static DesignDocument BuildBlank(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/", "Leeg ontwerp", CreateCanvasRoot())]
        };

    private static DesignDocument BuildFormPage(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/formulier", "Formulierpagina", CreateFormPageNodes())]
        };

    private static DesignDocument BuildListCrud(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/lijst", "Lijst/CRUD", CreateListCrudNodes())]
        };

    private static DesignDocument BuildMasterDetail(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/master-detail", "Master-detail", CreateMasterDetailNodes())]
        };

    private static DesignDocument BuildWizard(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/wizard", "Wizard", CreateWizardNodes())]
        };

    private static DesignDocument BuildDashboard(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/dashboard", "Dashboard", CreateDashboardNodes())]
        };

    private static DesignPage CreateBasePage(string route, string title, IReadOnlyList<DesignNode> nodes)
        => new()
        {
            Route = route,
            Title = title,
            Nodes = nodes.ToList()
        };

    private static IReadOnlyList<DesignNode> CreateCanvasRoot()
    {
        var root = new DesignNode
        {
            ComponentType = "RadzenRow",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Gap"] = DesignParameterValue.FromValue("1rem")
            },
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["ChildContent"] =
                [
                    new DesignNode
                    {
                        ComponentType = "RadzenColumn",
                        Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                        {
                            ["Size"] = DesignParameterValue.FromValue(12)
                        },
                        Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                        {
                            ["ChildContent"] = []
                        }
                    }
                ]
            }
        };

        return [root];
    }

    private static IReadOnlyList<DesignNode> CreateFormPageNodes()
        =>
        [
            CreatePageHeader("Formulierpagina", "Werkvelden en acties voor een bewerkbaar formulier."),
            CreateCard([CreateTextField("Klantnaam"), CreateNumericField("Aantal"), CreateSwitch("Actief"), CreateFormActions()])
        ];

    private static IReadOnlyList<DesignNode> CreateListCrudNodes()
        =>
        [
            CreatePageHeader("Lijst/CRUD", "Startpunt voor zoeken, filteren en bewerken."),
            CreateCard([CreateTextField("Zoeken"), CreateEmptyState("Nog geen items", "Voeg gegevens toe of importeer een dataset.")])
        ];

    private static IReadOnlyList<DesignNode> CreateMasterDetailNodes()
        =>
        [
            CreatePageHeader("Master-detail", "Selecteer links, bekijk details rechts."),
            CreateCard([
                CreateTextField("Master item"),
                CreateCard([CreateTextField("Detail titel"), CreateSwitch("Publiceren")])
            ])
        ];

    private static IReadOnlyList<DesignNode> CreateWizardNodes()
        =>
        [
            CreatePageHeader("Wizard", "Leid de gebruiker door een stap-voor-stap flow."),
            CreateCard([CreateTextField("Stap 1"), CreateTextField("Stap 2"), CreateFormActions()]),
            CreateCard([CreateEmptyState("Volgende stap", "Gebruik de navigatieknoppen om door te gaan.")])
        ];

    private static IReadOnlyList<DesignNode> CreateDashboardNodes()
        =>
        [
            CreatePageHeader("Dashboard", "Statuskaarten en samenvattingen in één overzicht."),
            CreateCard([CreateEmptyState("Metrieken", "Toon hier KPI's of grafieken.")]),
            CreateCard([CreateTextField("Statusfilter"), CreateNumericField("Periode"), CreateSwitch("Live verversen")])
        ];

    private static DesignNode CreatePageHeader(string title, string description)
        => new()
        {
            ComponentType = "AgtPageHeader",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Title"] = DesignParameterValue.FromValue(title),
                ["Description"] = DesignParameterValue.FromValue(description)
            }
        };

    private static DesignNode CreateCard(IReadOnlyList<DesignNode> childContent)
        => new()
        {
            ComponentType = "AgtCard",
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["ChildContent"] = childContent.ToList()
            }
        };

    private static DesignNode CreateTextField(string label)
        => new()
        {
            ComponentType = "AgtTextField",
            Parameters = CreateAccessibleFieldParameters(label, "Vul tekst in")
        };

    private static DesignNode CreateNumericField(string label)
        => new()
        {
            ComponentType = "AgtNumericField",
            Parameters = CreateAccessibleFieldParameters(label, "Kies een getal")
        };

    private static DesignNode CreateSwitch(string label)
        => new()
        {
            ComponentType = "AgtSwitch",
            Parameters = CreateAccessibleFieldParameters(label, "Schakel dit veld")
        };

    private static DesignNode CreateEmptyState(string title, string description)
        => new()
        {
            ComponentType = "AgtEmptyState",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Icon"] = DesignParameterValue.FromValue("info"),
                ["Title"] = DesignParameterValue.FromValue(title),
                ["Description"] = DesignParameterValue.FromValue(description)
            }
        };

    private static DesignNode CreateFormActions()
        => new()
        {
            ComponentType = "AgtFormActions",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["SaveText"] = DesignParameterValue.FromValue("Opslaan"),
                ["CancelText"] = DesignParameterValue.FromValue("Annuleren")
            }
        };

    private static Dictionary<string, DesignParameterValue> CreateAccessibleFieldParameters(string label, string ariaLabel)
        => new(StringComparer.Ordinal)
        {
            ["Label"] = DesignParameterValue.FromValue(label),
            ["AriaLabel"] = DesignParameterValue.FromValue(ariaLabel)
        };
}