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
        new(DesignDocumentTemplateKind.Dashboard, "Dashboard", "Dashboard met kaarten en samenvatting."),
        new(DesignDocumentTemplateKind.SidebarApp, "Sidebar app", "Meerdere pagina's met navigatie en vaste shell."),
        new(DesignDocumentTemplateKind.SettingsPage, "Instellingen", "Een compact instellingenformulier met secties."),
        new(DesignDocumentTemplateKind.DetailPage, "Detailpagina", "Detailweergave met samenvatting en gerelateerde data."),
        new(DesignDocumentTemplateKind.KanbanBoard, "Kanbanbord", "Kolommen voor werk in uitvoering en afronding."),
        new(DesignDocumentTemplateKind.LoginPage, "Loginpagina", "Inlogscherm met branding en actieknop."),
        new(DesignDocumentTemplateKind.TableWithFilters, "Tabel met filters", "Geavanceerde lijst met zoeken, filters en acties.")
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
            DesignDocumentTemplateKind.SidebarApp => BuildSidebarApp(name),
            DesignDocumentTemplateKind.SettingsPage => BuildSettingsPage(name),
            DesignDocumentTemplateKind.DetailPage => BuildDetailPage(name),
            DesignDocumentTemplateKind.KanbanBoard => BuildKanbanBoard(name),
            DesignDocumentTemplateKind.LoginPage => BuildLoginPage(name),
            DesignDocumentTemplateKind.TableWithFilters => BuildTableWithFilters(name),
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

    private static DesignDocument BuildSidebarApp(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages =
            [
                CreateBasePage("/", "Dashboard", CreateSidebarDashboardNodes()),
                CreateBasePage("/dossiers", "Schadedossiers", CreateSidebarListNodes()),
                CreateBasePage("/dossier/nieuw", "Nieuw dossier", CreateSidebarFormNodes()),
                CreateBasePage("/instellingen", "Instellingen", CreateSidebarSettingsNodes())
            ]
        };

    private static DesignDocument BuildSettingsPage(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/instellingen", "Instellingen", CreateSettingsPageNodes())]
        };

    private static DesignDocument BuildDetailPage(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/detail", "Detailpagina", CreateDetailPageNodes())]
        };

    private static DesignDocument BuildKanbanBoard(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/kanban", "Kanbanbord", CreateKanbanNodes())]
        };

    private static DesignDocument BuildLoginPage(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/login", "Loginpagina", CreateLoginNodes())]
        };

    private static DesignDocument BuildTableWithFilters(string name)
        => new()
        {
            Name = name,
            Version = "1.0",
            DataModel = DesignDataModelSeeder.CreateDefault(),
            Pages = [CreateBasePage("/tabel", "Tabel met filters", CreateTableWithFiltersNodes())]
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
            CreatePageHeader("Schadedossier Dashboard", "Een realtime overzicht van dossiers, klanten en werkorders."),
            CreateRow(
                CreateColumn(3, CreateCard([CreateEmptyState("Open dossiers", "124 open dossiers")])),
                CreateColumn(3, CreateCard([CreateEmptyState("Vandaag gepland", "18 werkorders")])),
                CreateColumn(3, CreateCard([CreateEmptyState("Gereed", "87 dossiers gereed")])),
                CreateColumn(3, CreateCard([CreateEmptyState("Facturatie", "€ 38k")]))),
            CreateRow(
                CreateColumn(8, CreateCard([CreateEmptyState("Schadedossiers", "Hier verschijnt de data-grid met seeded dossiers.")])),
                CreateColumn(4, CreateCard([CreateTextField("Filter op status"), CreateSwitch("Alleen urgente"), CreateTextField("Team")])) )
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarDashboardNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "Dashboard",
                [
                    CreatePageHeader("Dashboard", "Overzicht van dossiers en werkvoorraad."),
            CreateRow(
                CreateColumn(4, CreateCard([CreateEmptyState("Open", "48 dossiers")])),
                CreateColumn(4, CreateCard([CreateEmptyState("In behandeling", "22 dossiers")])),
                CreateColumn(4, CreateCard([CreateEmptyState("Afgerond", "61 dossiers")]))),
                    CreateCard([CreateEmptyState("Activiteit", "Grafieken en tabellen verschijnen hier met seeded data.")])
                ])
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarListNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "Schadedossiers",
                [
                    CreatePageHeader("Schadedossiers", "Selecteer een dossier en bekijk de details."),
                    CreateCard([CreateTextField("Zoek dossier"), CreateNumericField("Aantal"), CreateSwitch("Toon gesloten")]),
                    CreateCard([CreateEmptyState("Lijst", "Hier komt een seeded tabel met dossiers.")])
                ])
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarFormNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "Nieuw dossier",
                [
                    CreatePageHeader("Nieuw dossier", "Leg een nieuw schadegeval vast."),
                    CreateCard([CreateTextField("Dossiernummer"), CreateTextField("Klantnaam"), CreateSwitch("Voorrijkosten")]),
                    CreateCard([CreateNumericField("Eigen risico"), CreateTextField("Kenteken"), CreateFormActions()])
                ])
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarSettingsNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "Instellingen",
                [
                    CreatePageHeader("Instellingen", "Pas de werkruimte en meldingen aan."),
                    CreateCard([CreateTextField("Bedrijfsnaam"), CreateSwitch("Notificaties"), CreateSwitch("Donkere modus")]),
                    CreateCard([CreateNumericField("Standaard wachttijd"), CreateTextField("Support e-mail")])
                ])
        ];

    private static DesignNode CreateSidebarLayoutShell(string headerTitle, IReadOnlyList<DesignNode> content)
    {
        var layout = new DesignNode
        {
            ComponentType = "AgtSidebarLayout",
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["Logo"] = [CreateTextValue("Agt Autoschade")],
                ["HeaderActions"] = [CreateTextValue($"Gebruiker · {headerTitle}")],
                ["Sidebar"] =
                [
                    CreateNavLink("Dashboard", "/", "dashboard"),
                    CreateNavLink("Schadedossiers", "/dossiers", "table_rows"),
                    CreateNavLink("Nieuw dossier", "/dossier/nieuw", "add_circle"),
                    CreateNavLink("Instellingen", "/instellingen", "settings")
                ],
                ["ChildContent"] = content.ToList()
            }
        };

        return layout;
    }

    private static DesignNode CreateNavLink(string text, string href, string icon)
        => new()
        {
            ComponentType = "AgtNavLink",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Text"] = DesignParameterValue.FromValue(text),
                ["Href"] = DesignParameterValue.FromValue(href),
                ["Icon"] = DesignParameterValue.FromValue(icon)
            }
        };

    private static DesignNode CreateTextValue(string text)
        => new()
        {
            ComponentType = "RadzenText",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Text"] = DesignParameterValue.FromValue(text)
            }
        };

    private static IReadOnlyList<DesignNode> CreateSettingsPageNodes()
        =>
        [
            CreatePageHeader("Instellingen", "Configuratie van het portaal en teaminstellingen."),
            CreateCard([CreateTextField("Tenant"), CreateSwitch("Audit logging"), CreateNumericField("Sessieduur")]),
            CreateCard([CreateTextField("Thema"), CreateTextField("Logo"), CreateFormActions()])
        ];

    private static IReadOnlyList<DesignNode> CreateDetailPageNodes()
        =>
        [
            CreatePageHeader("Detailpagina", "Samenvatting, historie en gerelateerde records."),
            CreateRow(
                CreateColumn(7, CreateCard([CreateTextField("Dossiernummer"), CreateTextField("Status"), CreateTextField("Klantnaam")])),
                CreateColumn(5, CreateCard([CreateSwitch("Gepubliceerd"), CreateNumericField("Laatste update"), CreateTextField("Eigenaar")]))),
            CreateCard([CreateEmptyState("Gerelateerde data", "Tabellen en acties verschijnen hier.")])
        ];

    private static IReadOnlyList<DesignNode> CreateKanbanNodes()
        =>
        [
            CreatePageHeader("Kanbanbord", "Werk items van nieuw naar gereed."),
            CreateRow(
                CreateColumn(4, CreateCard([CreateEmptyState("Nieuw", "Kaarten voor nieuwe dossiers.")])),
                CreateColumn(4, CreateCard([CreateEmptyState("In behandeling", "Actieve werkitems.")])),
                CreateColumn(4, CreateCard([CreateEmptyState("Gereed", "Voltooide items.")])) )
        ];

    private static IReadOnlyList<DesignNode> CreateLoginNodes()
        =>
        [
            CreatePageHeader("Welkom terug", "Log in om verder te gaan."),
            CreateCard([CreateTextField("E-mail"), CreateTextField("Wachtwoord"), CreateFormActions()]),
            CreateCard([CreateEmptyState("Branding", "Logo en sfeerbeeld voor de inlogpagina.")])
        ];

    private static IReadOnlyList<DesignNode> CreateTableWithFiltersNodes()
        =>
        [
            CreatePageHeader("Tabel met filters", "Filter, sorteer en exporteer dossiers."),
            CreateCard([CreateTextField("Zoekterm"), CreateSwitch("Actief"), CreateNumericField("Pagina grootte")]),
            CreateCard([CreateEmptyState("Resultaten", "Seeded tabel met meerdere rijen.")])
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

    private static DesignNode CreateRow(params DesignNode[] children)
        => new()
        {
            ComponentType = "RadzenRow",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Gap"] = DesignParameterValue.FromValue("1rem")
            },
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["ChildContent"] = children.ToList()
            }
        };

    private static DesignNode CreateColumn(int size, params DesignNode[] children)
        => new()
        {
            ComponentType = "RadzenColumn",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Size"] = DesignParameterValue.FromValue(size)
            },
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["ChildContent"] = children.ToList()
            }
        };
}