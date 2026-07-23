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
                CreateBasePage("/dossiers", "Dossiers", CreateSidebarListNodes()),
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
            CreatePageHeader("Nieuw schadedossier", "Vul de gegevens in voor een nieuw dossier."),
            CreateCard([
                CreateRow(
                    CreateColumn(6, CreateTextField("Dossiernummer")),
                    CreateColumn(6, CreateDatePicker("Schadedatum"))),
                CreateRow(
                    CreateColumn(6, CreateDropdown("Schadesoort")),
                    CreateColumn(6, CreateDropdown("Glastype"))),
                CreateRow(
                    CreateColumn(6, CreateDropdown("Actie")),
                    CreateColumn(6, CreateSwitch("Voorexpertise nodig"))),
                CreateTextArea("Opmerkingen"),
                CreateFormActions()
            ])
        ];

    private static IReadOnlyList<DesignNode> CreateListCrudNodes()
        =>
        [
            CreatePageHeader("Schadedossiers", "Zoek, filter en beheer dossiers."),
            CreateRow(
                CreateColumn(4, CreateTextField("Zoeken")),
                CreateColumn(4, CreateDropdown("Status")),
                CreateColumn(4, CreateDropdown("Glastype"))),
            CreateCard([CreateEmptyState("Geen resultaten", "Pas de filters aan of maak een nieuw dossier.", "search")])
        ];

    private static IReadOnlyList<DesignNode> CreateMasterDetailNodes()
        =>
        [
            CreatePageHeader("Dossieroverzicht", "Selecteer links een dossier en werk rechts de details bij."),
            CreateRow(
                CreateColumn(
                    4,
                    CreateCard([
                        CreateTextField("Zoeken"),
                        CreateDropdown("Status"),
                        CreateDropdown("Actie"),
                        CreateEmptyState("Geen selectie", "Kies links een dossier voor detailinformatie.", "fact_check")
                    ])),
                CreateColumn(
                    8,
                    CreateCard([
                        CreatePageHeader("Dossierdetails", "Werk de belangrijkste dossiergegevens bij."),
                        CreateRow(
                            CreateColumn(6, CreateTextField("Dossiernummer")),
                            CreateColumn(6, CreateDatePicker("Schadedatum"))),
                        CreateRow(
                            CreateColumn(6, CreateDropdown("Schadesoort")),
                            CreateColumn(6, CreateDropdown("Glastype"))),
                        CreateTextArea("Opmerkingen"),
                        CreateFormActions()
                    ])))
        ];

    private static IReadOnlyList<DesignNode> CreateWizardNodes()
        =>
        [
            CreatePageHeader("Nieuwe claim wizard", "Doorloop de stappen om een claim volledig vast te leggen."),
            CreateCard([
                CreateEmptyState("Stap 1 van 3", "Vastleggen van basisgegevens.", "looks_one"),
                CreateRow(
                    CreateColumn(6, CreateTextField("Dossiernummer")),
                    CreateColumn(6, CreateDatePicker("Schadedatum")),
                    CreateColumn(6, CreateDropdown("Schadesoort")),
                    CreateColumn(6, CreateDropdown("Glastype")))
            ]),
            CreateCard([
                CreateEmptyState("Stap 2 van 3", "Bepaal vervolgactie en planning.", "looks_two"),
                CreateRow(
                    CreateColumn(6, CreateDropdown("Actie")),
                    CreateColumn(6, CreateSwitch("Voorexpertise nodig"))),
                CreateTextArea("Opmerkingen")
            ]),
            CreateCard([
                CreateEmptyState("Stap 3 van 3", "Controleer en bevestig de aanvraag.", "looks_3"),
                CreateFormActions()
            ])
        ];

    private static IReadOnlyList<DesignNode> CreateDashboardNodes()
        =>
        [
            CreatePageHeader("Schadedossier Dashboard", "Overzicht van lopende dossiers en werkorders."),
            CreateRow(
                CreateColumn(3, CreateCard([CreatePageHeader("Nieuwe dossiers", "Vandaag toegevoegd")])),
                CreateColumn(3, CreateCard([CreatePageHeader("In behandeling", "Actieve werkorders")])),
                CreateColumn(3, CreateCard([CreatePageHeader("Gereed", "Klaar voor factuur")])),
                CreateColumn(3, CreateCard([CreatePageHeader("Gefactureerd", "Afgeronde administratie")]))),
            CreateRow(
                CreateColumn(8, CreateCard([
                    CreatePageHeader("Recente dossiers", "Laatste wijzigingen uit de werkvoorraad."),
                    CreateEmptyState("Data wordt geladen...", "Dossiers verschijnen hier zodra de gegevens beschikbaar zijn.", "sync")
                ])),
                CreateColumn(4, CreateCard([
                    CreatePageHeader("Status verdeling", "Samenvatting per status."),
                    CreateEmptyState("Nog geen grafiek", "Voeg een visualisatie toe voor statusverdeling.", "pie_chart")
                ])) )
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarDashboardNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "/",
                [
                    CreatePageHeader("Schadedossier Dashboard", "Overzicht van lopende dossiers en werkorders."),
                    CreateRow(
                        CreateColumn(3, CreateCard([CreatePageHeader("Nieuwe dossiers", "Vandaag toegevoegd")])),
                        CreateColumn(3, CreateCard([CreatePageHeader("In behandeling", "Actieve werkorders")])),
                        CreateColumn(3, CreateCard([CreatePageHeader("Gereed", "Klaar voor factuur")])),
                        CreateColumn(3, CreateCard([CreatePageHeader("Gefactureerd", "Afgeronde administratie")]))),
                    CreateRow(
                        CreateColumn(8, CreateCard([
                            CreatePageHeader("Recente dossiers", "Laatste wijzigingen uit de werkvoorraad."),
                            CreateEmptyState("Data wordt geladen...", "Dossiers verschijnen hier zodra de gegevens beschikbaar zijn.", "sync")
                        ])),
                        CreateColumn(4, CreateCard([
                            CreatePageHeader("Status verdeling", "Samenvatting per status."),
                            CreateEmptyState("Nog geen grafiek", "Voeg een visualisatie toe voor statusverdeling.", "pie_chart")
                        ])))
                ])
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarListNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "/dossiers",
                [
                    CreatePageHeader("Schadedossiers", "Zoek, filter en beheer dossiers."),
                    CreateRow(
                        CreateColumn(4, CreateTextField("Zoeken")),
                        CreateColumn(4, CreateDropdown("Status")),
                        CreateColumn(4, CreateDropdown("Glastype"))),
                    CreateCard([CreateEmptyState("Geen resultaten", "Pas de filters aan of maak een nieuw dossier.", "search")])
                ])
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarFormNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "/dossier/nieuw",
                [
                    CreatePageHeader("Nieuw schadedossier", "Vul de gegevens in voor een nieuw dossier."),
                    CreateCard([
                        CreateRow(
                            CreateColumn(6, CreateTextField("Dossiernummer")),
                            CreateColumn(6, CreateDatePicker("Schadedatum"))),
                        CreateRow(
                            CreateColumn(6, CreateDropdown("Schadesoort")),
                            CreateColumn(6, CreateDropdown("Glastype"))),
                        CreateRow(
                            CreateColumn(6, CreateDropdown("Actie")),
                            CreateColumn(6, CreateSwitch("Voorexpertise nodig"))),
                        CreateTextArea("Opmerkingen"),
                        CreateFormActions()
                    ])
                ])
        ];

    private static IReadOnlyList<DesignNode> CreateSidebarSettingsNodes()
        =>
        [
            CreateSidebarLayoutShell(
                "/instellingen",
                [
                    CreatePageHeader("Instellingen", "Beheer profiel, notificaties en weergavevoorkeuren."),
                    CreateCard([
                        CreatePageHeader("Profiel", "Persoonlijke en organisatorische gegevens."),
                        CreateRow(
                            CreateColumn(6, CreateTextField("Naam")),
                            CreateColumn(6, CreateTextField("E-mail")))
                    ]),
                    CreateCard([
                        CreatePageHeader("Notificaties", "Ontvang updates voor belangrijke gebeurtenissen."),
                        CreateSwitch("Nieuwe dossiers"),
                        CreateSwitch("Statuswijzigingen"),
                        CreateSwitch("Facturatie updates")
                    ]),
                    CreateCard([
                        CreatePageHeader("Weergave", "Kies weergave-instellingen voor de werkplek."),
                        CreateDropdown("Thema"),
                        CreateSwitch("Compacte modus")
                    ]),
                    CreateFormActions()
                ])
        ];

    private static DesignNode CreateSidebarLayoutShell(string activeRoute, IReadOnlyList<DesignNode> content)
    {
        var layout = new DesignNode
        {
            ComponentType = "AgtSidebarLayout",
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["Logo"] = [CreatePageHeader("Agterhuis Autoschade", "Schadeportaal")],
                ["HeaderActions"] = [CreateTextValue("Planner · Team Noord")],
                ["Sidebar"] =
                [
                    CreateNavLink("Dashboard", "/", "dashboard", string.Equals(activeRoute, "/", StringComparison.OrdinalIgnoreCase)),
                    CreateNavLink("Dossiers", "/dossiers", "table_rows", string.Equals(activeRoute, "/dossiers", StringComparison.OrdinalIgnoreCase)),
                    CreateNavLink("Nieuw dossier", "/dossier/nieuw", "add_circle", string.Equals(activeRoute, "/dossier/nieuw", StringComparison.OrdinalIgnoreCase)),
                    CreateNavLink("Instellingen", "/instellingen", "settings", string.Equals(activeRoute, "/instellingen", StringComparison.OrdinalIgnoreCase))
                ],
                ["ChildContent"] = content.ToList()
            }
        };

        return layout;
    }

    private static DesignNode CreateNavLink(string text, string href, string icon, bool isActive = false)
        => new()
        {
            ComponentType = "AgtNavLink",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Text"] = DesignParameterValue.FromValue(text),
                ["Href"] = DesignParameterValue.FromValue(href),
                ["Icon"] = DesignParameterValue.FromValue(icon),
                ["IsActive"] = DesignParameterValue.FromValue(isActive)
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
            CreatePageHeader("Instellingen", "Beheer profiel, notificaties en weergavevoorkeuren."),
            CreateCard([
                CreatePageHeader("Profiel", "Persoonlijke en organisatorische gegevens."),
                CreateRow(
                    CreateColumn(6, CreateTextField("Naam")),
                    CreateColumn(6, CreateTextField("E-mail")))
            ]),
            CreateCard([
                CreatePageHeader("Notificaties", "Ontvang updates voor belangrijke gebeurtenissen."),
                CreateSwitch("Nieuwe dossiers"),
                CreateSwitch("Statuswijzigingen"),
                CreateSwitch("Facturatie updates")
            ]),
            CreateCard([
                CreatePageHeader("Weergave", "Kies weergave-instellingen voor de werkplek."),
                CreateDropdown("Thema"),
                CreateSwitch("Compacte modus")
            ]),
            CreateFormActions()
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
            CreatePageHeader("Voertuigen", "Filter en doorzoek het voertuigenoverzicht."),
            CreateRow(
                CreateColumn(3, CreateDropdown("Merk")),
                CreateColumn(3, CreateDropdown("Bouwjaar")),
                CreateColumn(3, CreateDropdown("Kleur")),
                CreateColumn(3, CreateDropdown("ADAS"))),
            CreateCard([CreateEmptyState("Tabel volgt", "Voeg een gegevensbron toe om voertuigen te tonen.", "table_chart")])
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

    private static DesignNode CreateDatePicker(string label)
        => new()
        {
            ComponentType = "AgtDatePicker",
            Parameters = CreateAccessibleFieldParameters(label, "Kies een datum")
        };

    private static DesignNode CreateDropdown(string label)
        => new()
        {
            ComponentType = "AgtDropdown",
            Parameters = CreateAccessibleFieldParameters(label, "Kies een optie")
        };

    private static DesignNode CreateTextArea(string label)
        => new()
        {
            ComponentType = "AgtTextArea",
            Parameters = CreateAccessibleFieldParameters(label, "Geef aanvullende informatie")
        };

    private static DesignNode CreateEmptyState(string title, string description, string icon = "info")
        => new()
        {
            ComponentType = "AgtEmptyState",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Icon"] = DesignParameterValue.FromValue(icon),
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