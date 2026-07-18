namespace Agterhuis.Ui.Demo.Services;

public sealed partial class ShowcaseDataService
{
    private readonly List<ShowcaseProject> _projects = [];
    private readonly List<ShowcaseProjectTask> _projectTasks = [];
    private readonly List<ShowcaseAssetNode> _assets = [];
    private readonly List<ShowcaseTicket> _tickets = [];
    private readonly List<ShowcaseChatMessage> _chatMessages = [];
    private readonly List<ShowcaseHelpLink> _helpLinks = [];

    private int _nextProjectId = 401;
    private int _nextTicketMessageId = 800;

    public IReadOnlyList<ShowcaseProject> Projects => _projects.OrderByDescending(p => p.Start).ToList();

    public IReadOnlyList<ShowcaseProjectTask> ProjectTasks => _projectTasks
        .GroupBy(task => task.Id)
        .Select(group => group.Last())
        .OrderBy(task => task.Start)
        .ThenBy(task => task.Id)
        .ToList();

    public IReadOnlyList<ShowcaseAssetNode> Assets => _assets.OrderBy(a => a.Path).ToList();

    public IReadOnlyList<ShowcaseTicket> Tickets => _tickets.OrderByDescending(t => t.UpdatedAt).ToList();

    public IReadOnlyList<ShowcaseHelpLink> HelpLinks => _helpLinks;

    public string HelpMarkdown => """
# Werkorders Handleiding

## Snel starten
1. Open **Werkorders** om tickets en opdrachten te beheren.
2. Gebruik **Planning** voor week- en maandbezetting.
3. Beheer assets via de boomstructuur op de pagina **Assets**.

## Veelgebruikte acties
- Maak een nieuwe werkorder aan via de mobiele FAB of de knop in Werkorders.
- Open een projectwizard in **Projecten**.
- Gebruik contextmenu-acties om assets direct om te zetten naar opdrachten.

## Kwaliteit
- Gebruik notities en foto's in de detailpagina van een werkorder.
- Controleer rapportages voor SLA, bezetting en voorraad.
""";

    public IReadOnlyList<ShowcaseChatMessage> GetMessages(int ticketId)
    {
        return _chatMessages.Where(m => m.TicketId == ticketId).OrderBy(m => m.When).ToList();
    }

    public ShowcaseWorkOrder? GetWorkOrderById(int id) => _workOrders.FirstOrDefault(x => x.Id == id);

    public IReadOnlyList<ShowcaseWorkOrderHistoryEntry> GetWorkOrderHistory(int id)
    {
        return GetWorkOrderById(id)?.History.OrderByDescending(h => h.When).ToList() ?? [];
    }

    public IReadOnlyList<ShowcaseAttachment> GetWorkOrderPhotos(int id)
    {
        return GetWorkOrderById(id)?.Photos ?? [];
    }

    public ShowcaseProject CreateProject(string name, int customerId, DateTime start, DateTime end, IEnumerable<int> technicianIds)
    {
        var customer = GetCustomer(customerId) ?? _customers[0];
        var project = new ShowcaseProject(_nextProjectId++, name, customer.Company, start, end, 0.35, "Actief", "midnightblue", "Project gegenereerd via wizard");
        _projects.Insert(0, project);

        var baseId = _projectTasks.Count + 1;
        var technicianNames = technicianIds.Select(id => GetTechnician(id)?.Name ?? "Onbekend").ToArray();
        _projectTasks.Add(new ShowcaseProjectTask(baseId, project.Id, "Scope en intake", start, start.AddDays(2), 0.7, null, "", technicianNames));
        _projectTasks.Add(new ShowcaseProjectTask(baseId + 1, project.Id, "Installatie op locatie", start.AddDays(2), end.AddDays(-2), 0.35, baseId, baseId.ToString(), technicianNames));
        _projectTasks.Add(new ShowcaseProjectTask(baseId + 2, project.Id, "Oplevering", end.AddDays(-2), end, 0.1, baseId + 1, (baseId + 1).ToString(), technicianNames));

        AddNotification("Project aangemaakt", $"{project.Name} voor {project.CustomerName}", ShowcaseIntent.Success);
        Changed?.Invoke();
        return project;
    }

    public ShowcaseWorkOrder CreateWorkOrderFromAsset(int assetId)
    {
        var asset = _assets.FirstOrDefault(a => a.Id == assetId);
        var draft = CreateDraftWorkOrder();
        if (asset is not null)
        {
            draft.AssetId = asset.Id;
            draft.Address = asset.Location;
            draft.Description = $"Asset-opdracht voor {asset.Name} ({asset.KindLabel}).";
            draft.Postcode = asset.PostalCode;
        }

        var created = AddWorkOrder(draft);
        AddNotification("Werkorder uit asset", $"{created.Number} voor {asset?.Name ?? "asset"}", ShowcaseIntent.Info);
        Changed?.Invoke();
        return created;
    }

    public void SaveWorkOrderDetail(ShowcaseWorkOrder updated)
    {
        UpdateWorkOrder(updated);
    }

    public void SendChatMessage(int ticketId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
        if (ticket is null)
        {
            return;
        }

        _chatMessages.Add(new ShowcaseChatMessage(_nextTicketMessageId++, ticketId, "Planner", message.Trim(), DateTime.Now, true));
        _chatMessages.Add(new ShowcaseChatMessage(_nextTicketMessageId++, ticketId, "AI-assistent", "Ik heb je bericht verwerkt en een vervolgstap voorgesteld.", DateTime.Now.AddSeconds(2), false));
        ticket.UpdatedAt = DateTime.Now;
        ticket.LastMessage = message.Trim();
        AddNotification("Nieuw servicedesk-bericht", ticket.Subject, ShowcaseIntent.Info);
        Changed?.Invoke();
    }

    private void InitializeExtendedData()
    {
        SeedProjects();
        SeedAssets();
        SeedTickets();
        SeedHelpLinks();
        SeedWorkOrderDetailData();
    }

    private void SeedProjects()
    {
        _projects.Clear();
        _projects.Add(new ShowcaseProject(301, "Renovatie Zorggroep Maasstad", "Zorggroep Maasstad", DateTime.Today.AddDays(-8), DateTime.Today.AddDays(18), 0.56, "Actief", "midnightblue", "Vervangen en inregelen van installaties."));
        _projects.Add(new ShowcaseProject(302, "Campus inspectieronde", "TechCampus Zuid", DateTime.Today.AddDays(-3), DateTime.Today.AddDays(12), 0.42, "Actief", "steelblue", "Inspectie en preventief onderhoud."));

        _projectTasks.Clear();
        _projectTasks.Add(new ShowcaseProjectTask(1, 301, "Kick-off en opname", DateTime.Today.AddDays(-8), DateTime.Today.AddDays(-5), 1, null, "", ["Jasper van Leeuwen"]));
        _projectTasks.Add(new ShowcaseProjectTask(2, 301, "Uitvoering fase 1", DateTime.Today.AddDays(-5), DateTime.Today.AddDays(5), 0.7, 1, "1", ["Mila Smit", "Sem Jansen"]));
        _projectTasks.Add(new ShowcaseProjectTask(3, 301, "Oplevering", DateTime.Today.AddDays(6), DateTime.Today.AddDays(18), 0.25, 2, "2", ["Lotte de Vries"]));
        _projectTasks.Add(new ShowcaseProjectTask(4, 302, "Asset inventarisatie", DateTime.Today.AddDays(-3), DateTime.Today.AddDays(1), 0.9, null, "", ["Noah Bakker"]));
        _projectTasks.Add(new ShowcaseProjectTask(5, 302, "Inspectie per verdieping", DateTime.Today.AddDays(1), DateTime.Today.AddDays(10), 0.3, 4, "4", ["Sanne de Groot", "Noah Bakker"]));
    }

    private void SeedAssets()
    {
        _assets.Clear();
        _assets.Add(new ShowcaseAssetNode(1, null, "Zorggroep Maasstad", "Locatie", "Marconistraat 18", "3011AA", ["kritiek"], "midnightblue", [new ShowcaseAssetSpec("Regio", "Rotterdam")]));
        _assets.Add(new ShowcaseAssetNode(2, 1, "Gebouw A", "Gebouw", "Marconistraat 18", "3011AA", ["zorg"], "steelblue", [new ShowcaseAssetSpec("Bouwlaag", "4") ]));
        _assets.Add(new ShowcaseAssetNode(3, 2, "CV-installatie", "Installatie", "Marconistraat 18", "3011AA", ["verwarming", "jaarlijks"], "darkgoldenrod", [new ShowcaseAssetSpec("Serie", "CV-AX22"), new ShowcaseAssetSpec("Laatste onderhoud", "2026-05-11") ]));
        _assets.Add(new ShowcaseAssetNode(4, 2, "Luchtbehandeling", "Installatie", "Marconistraat 18", "3011AA", ["ventilatie"], "seagreen", [new ShowcaseAssetSpec("Filterstatus", "61%") ]));
        _assets.Add(new ShowcaseAssetNode(5, null, "TechCampus Zuid", "Locatie", "Campusweg 7", "3067BC", ["campus"], "cadetblue", [new ShowcaseAssetSpec("Gebouwen", "3") ]));
        _assets.Add(new ShowcaseAssetNode(6, 5, "Blok C", "Gebouw", "Campusweg 7", "3067BC", ["lab"], "peru", [new ShowcaseAssetSpec("Toegang", "Badge") ]));
        _assets.Add(new ShowcaseAssetNode(7, 6, "Koelunit 3", "Installatie", "Campusweg 7", "3067BC", ["koeling", "urgent"], "firebrick", [new ShowcaseAssetSpec("Temperatuur", "7.3C") ]));
    }

    private void SeedTickets()
    {
        _tickets.Clear();
        _tickets.Add(new ShowcaseTicket(701, "Warmtewisselaar storing", "Open", "Zorggroep Maasstad", DateTime.Now.AddMinutes(-24), "Melding ontvangen, monteur ingepland."));
        _tickets.Add(new ShowcaseTicket(702, "Nazorg oplevering", "Bezig", "Hotel Mercurius", DateTime.Now.AddHours(-2), "Onderdeel besteld, update volgt."));
        _tickets.Add(new ShowcaseTicket(703, "Contractvraag planning", "Gesloten", "Woonstichting Noord", DateTime.Now.AddDays(-1), "Afgesloten na akkoord klant."));

        _chatMessages.Clear();
        _chatMessages.Add(new ShowcaseChatMessage(801, 701, "Klant", "De ketel valt uit tijdens piekuren.", DateTime.Now.AddMinutes(-35), false));
        _chatMessages.Add(new ShowcaseChatMessage(802, 701, "Planner", "Dank, we plannen direct een storingsteam in.", DateTime.Now.AddMinutes(-30), true));
        _chatMessages.Add(new ShowcaseChatMessage(803, 702, "Klant", "Kunnen jullie de nazorg naar vrijdag verplaatsen?", DateTime.Now.AddHours(-3), false));
        _chatMessages.Add(new ShowcaseChatMessage(804, 702, "Planner", "Ja, vrijdag 09:00 staat gereserveerd.", DateTime.Now.AddHours(-2), true));
    }

    private void SeedHelpLinks()
    {
        _helpLinks.Clear();
        _helpLinks.Add(new ShowcaseHelpLink("Werkorders API", "https://internal.example.local/workorders", "Interne API-documentatie"));
        _helpLinks.Add(new ShowcaseHelpLink("Storingsprotocol", "https://internal.example.local/protocol", "Proces bij kritieke meldingen"));
        _helpLinks.Add(new ShowcaseHelpLink("Planner onboarding", "https://internal.example.local/onboarding", "Inwerkdocument voor planners"));
    }

    private void SeedWorkOrderDetailData()
    {
        var rnd = new Random(112358);
        foreach (var order in _workOrders)
        {
            order.ProgressPercent = rnd.Next(15, 95);
            order.SatisfactionRating = rnd.Next(3, 6);
            order.Postcode = order.CustomerId % 2 == 0 ? "3011AA" : "3067BC";
            order.AssetId = _assets[rnd.Next(_assets.Count)].Id;
            order.SkillsCsv = order.Type switch
            {
                ShowcaseWorkOrderType.Installatie => "Installatie,Veiligheid",
                ShowcaseWorkOrderType.Reparatie => "Storingsdienst,Diagnose",
                ShowcaseWorkOrderType.Onderhoud => "Inspectie,Onderhoud",
                _ => "Inspectie,Rapportage"
            };
            order.NotesHtml = $"<p><strong>{order.Number}</strong> - notities voor {order.CustomerName}.</p>";

            order.History =
            [
                new ShowcaseWorkOrderHistoryEntry(order.PlannedStart.AddDays(-2), ShowcaseWorkOrderStatus.Gepland, "Werkorder aangemaakt", order.TechnicianName, $"{order.TechnicianName.Replace(" ", ".").ToLowerInvariant()}@werkorders.nl"),
                new ShowcaseWorkOrderHistoryEntry(order.PlannedStart.AddDays(-1), ShowcaseWorkOrderStatus.Onderweg, "Planning bevestigd", order.TechnicianName, $"{order.TechnicianName.Replace(" ", ".").ToLowerInvariant()}@werkorders.nl"),
                new ShowcaseWorkOrderHistoryEntry(order.PlannedStart, order.Status, "Uitvoering update", order.TechnicianName, $"{order.TechnicianName.Replace(" ", ".").ToLowerInvariant()}@werkorders.nl")
            ];

            order.Photos =
            [
                new ShowcaseAttachment($"{order.Number}-1", "Installatie-overzicht", "https://placehold.co/640x360/png?text=Werkorder+Foto+1"),
                new ShowcaseAttachment($"{order.Number}-2", "Detail opname", "https://placehold.co/640x360/png?text=Werkorder+Foto+2")
            ];
        }
    }
}

public sealed record ShowcaseProject(int Id, string Name, string CustomerName, DateTime Start, DateTime End, double Progress, string Status, string Color, string Description);

public sealed record ShowcaseProjectTask(int Id, int ProjectId, string Text, DateTime Start, DateTime End, double Progress, int? ParentId, string Dependencies, IReadOnlyList<string> Assignees);

public sealed record ShowcaseAssetSpec(string Name, string Value);

public sealed record ShowcaseAssetNode(int Id, int? ParentId, string Name, string KindLabel, string Location, string PostalCode, IReadOnlyList<string> Tags, string LabelColor, IReadOnlyList<ShowcaseAssetSpec> Specs)
{
    public string Path => ParentId is null ? Name : $"{ParentId}-{Name}";
}

public sealed class ShowcaseTicket
{
    public ShowcaseTicket(int id, string subject, string status, string customerName, DateTime updatedAt, string lastMessage)
    {
        Id = id;
        Subject = subject;
        Status = status;
        CustomerName = customerName;
        UpdatedAt = updatedAt;
        LastMessage = lastMessage;
    }

    public int Id { get; }
    public string Subject { get; }
    public string Status { get; set; }
    public string CustomerName { get; }
    public DateTime UpdatedAt { get; set; }
    public string LastMessage { get; set; }
}

public sealed record ShowcaseChatMessage(int Id, int TicketId, string Sender, string Text, DateTime When, bool IsAgent);

public sealed record ShowcaseHelpLink(string Title, string Url, string Description);

public sealed record ShowcaseWorkOrderHistoryEntry(DateTime When, ShowcaseWorkOrderStatus Status, string Note, string TechnicianName, string TechnicianEmail);

public sealed record ShowcaseAttachment(string Id, string Name, string Url);