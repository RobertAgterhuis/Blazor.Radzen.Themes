using System.Globalization;

namespace Agterhuis.Ui.Demo.Services;

public sealed partial class ShowcaseDataService
{
    public event Action? Changed;

    private readonly List<ShowcaseCustomer> _customers;
    private readonly List<ShowcaseTechnician> _technicians;
    private readonly List<ShowcaseWorkOrder> _workOrders;
    private readonly List<ShowcaseNotification> _notifications;

    private ShowcaseSettings _settings = new();
    private int _nextWorkOrderId = 10061;
    private int _nextNotificationId = 9;

    public ShowcaseDataService()
    {
        _customers = CreateCustomers();
        _technicians = CreateTechnicians();
        _workOrders = CreateWorkOrders();
        _notifications = CreateNotifications();
        InitializeExtendedData();
    }

    public IReadOnlyList<ShowcaseCustomer> Customers => _customers;

    public IReadOnlyList<ShowcaseTechnician> Technicians => _technicians;

    public IReadOnlyList<ShowcaseWorkOrder> WorkOrders => _workOrders
        .OrderByDescending(item => item.PlannedStart)
        .ThenByDescending(item => item.Id)
        .ToList();

    public IReadOnlyList<ShowcaseNotification> Notifications => _notifications
        .OrderByDescending(item => item.When)
        .Take(10)
        .ToList();

    public ShowcaseSettings Settings => _settings;

    public IReadOnlyList<ShowcaseMonthlyPerformance> MonthlyPerformance => BuildMonthlyPerformance();

    public IReadOnlyList<ShowcaseTechnicianPerformance> TechnicianPerformance => BuildTechnicianPerformance();

    public IEnumerable<ShowcaseWorkOrder> TodayOrders => WorkOrders.Where(item => item.PlannedStart.Date == DateTime.Today).Take(6);

    public int OpenOrderCount => _workOrders.Count(item => item.Status is ShowcaseWorkOrderStatus.Gepland or ShowcaseWorkOrderStatus.Onderweg or ShowcaseWorkOrderStatus.InUitvoering);

    public int PlannedTodayCount => _workOrders.Count(item => item.PlannedStart.Date == DateTime.Today);

    public int CompletedThisWeekCount => _workOrders.Count(item => item.Status == ShowcaseWorkOrderStatus.Afgerond && item.PlannedStart.Date >= StartOfCurrentWeek());

    public decimal RevenueMonthToDate => _workOrders
        .Where(item => item.PlannedStart.Month == DateTime.Today.Month && item.PlannedStart.Year == DateTime.Today.Year && item.Status != ShowcaseWorkOrderStatus.Geannuleerd)
        .Sum(item => item.Amount);

    public ShowcaseWorkOrder CreateDraftWorkOrder()
    {
        var customer = _customers[0];
        var technician = _technicians[0];
        var start = DateTime.Today.AddDays(1).AddHours(9);

        return new ShowcaseWorkOrder
        {
            Id = _nextWorkOrderId,
            Number = BuildWorkOrderNumber(_nextWorkOrderId),
            CustomerId = customer.Id,
            CustomerName = customer.Company,
            Address = customer.Address,
            Type = ShowcaseWorkOrderType.Installatie,
            Status = ShowcaseWorkOrderStatus.Gepland,
            Priority = ShowcaseWorkOrderPriority.Hoog,
            TechnicianId = technician.Id,
            TechnicianName = technician.Name,
            PlannedStart = start,
            PlannedEnd = start.AddHours(2),
            Amount = 790m,
            Description = "Nieuwe klantinstallatie met oplevering op locatie."
        };
    }

    public ShowcaseWorkOrder AddWorkOrder(ShowcaseWorkOrder draft)
    {
        var order = CloneOrder(draft);
        order.Id = _nextWorkOrderId++;
        order.Number = BuildWorkOrderNumber(order.Id);
        ApplyLookupData(order);
        _workOrders.Insert(0, order);
        AddNotification($"Werkorder {order.Number} aangemaakt", $"{order.CustomerName} in {order.PlanningLabel}.", ShowcaseIntent.Success);
        Changed?.Invoke();
        return order;
    }

    public void UpdateWorkOrder(ShowcaseWorkOrder updated)
    {
        var index = _workOrders.FindIndex(item => item.Id == updated.Id);
        if (index < 0)
        {
            return;
        }

        var order = _workOrders[index];
        order.CustomerId = updated.CustomerId;
        order.Address = updated.Address;
        order.Type = updated.Type;
        order.Status = updated.Status;
        order.Priority = updated.Priority;
        order.TechnicianId = updated.TechnicianId;
        order.PlannedStart = updated.PlannedStart;
        order.PlannedEnd = updated.PlannedEnd;
        order.Amount = updated.Amount;
        order.Description = updated.Description;
        ApplyLookupData(order);
        AddNotification($"Werkorder {order.Number} bijgewerkt", order.PlanningLabel, ShowcaseIntent.Info);
        Changed?.Invoke();
    }

    public void DeleteWorkOrder(int workOrderId)
    {
        var removed = _workOrders.FirstOrDefault(item => item.Id == workOrderId);
        if (removed is null)
        {
            return;
        }

        _workOrders.Remove(removed);
        AddNotification($"Werkorder {removed.Number} verwijderd", removed.CustomerName, ShowcaseIntent.Warning);
        Changed?.Invoke();
    }

    public void RescheduleWorkOrder(int workOrderId, DateTime start, DateTime end)
    {
        var order = _workOrders.FirstOrDefault(item => item.Id == workOrderId);
        if (order is null)
        {
            return;
        }

        order.PlannedStart = start;
        order.PlannedEnd = end;
        order.Status = ShowcaseWorkOrderStatus.Gepland;
        AddNotification($"Werkorder {order.Number} verplaatst", order.PlanningLabel, ShowcaseIntent.Info);
        Changed?.Invoke();
    }

    public void UpdateSettings(ShowcaseSettings settings)
    {
        _settings = settings;
        AddNotification("Instellingen opgeslagen", settings.DefaultRegion, ShowcaseIntent.Success);
        Changed?.Invoke();
    }

    public IEnumerable<ShowcaseWorkOrder> GetWorkOrdersForCustomer(int customerId) => _workOrders.Where(item => item.CustomerId == customerId);

    public ShowcaseCustomer? GetCustomer(int customerId) => _customers.FirstOrDefault(item => item.Id == customerId);

    public ShowcaseTechnician? GetTechnician(int technicianId) => _technicians.FirstOrDefault(item => item.Id == technicianId);

    private static List<ShowcaseCustomer> CreateCustomers()
    {
        return [
            new(1, "Van Dijk Installaties B.V.", "Marloes van Dijk", "marloes@vandijk-installaties.nl", "010 512 44 80", "Rotterdam", "Marconistraat 18"),
            new(2, "Bouwcentrum De Lier", "Jeroen Bakker", "jeroen@bouwcentrumdelier.nl", "0174 62 13 90", "De Lier", "Laan van Westlands 12"),
            new(3, "Zorggroep Maasstad", "Fatima El Amrani", "fatima@maasstadzorg.nl", "010 778 31 22", "Schiedam", "Spaanse Polder 4"),
            new(4, "Havenlogistiek Delta", "Tom de Jong", "tom@delta-havenlogistiek.nl", "0181 55 91 40", "Spijkenisse", "Kadeweg 31"),
            new(5, "Scholenkoepel Rijnmond", "Saskia Peters", "saskia@rijnmondscholen.nl", "010 430 22 10", "Capelle aan den IJssel", "Prinses Margrietlaan 9"),
            new(6, "Hotel Mercurius", "Eva de Wit", "eva@hotelmercurius.nl", "010 204 90 70", "Delft", "Stationsplein 5"),
            new(7, "Woonstichting Noord", "Koen Visser", "koen@woonstichtingnoord.nl", "015 770 18 30", "Rijswijk", "Lindelaan 44"),
            new(8, "TechCampus Zuid", "Nadia Alami", "nadia@techcampuszuid.nl", "010 612 84 22", "Rotterdam", "Campusweg 7"),
            new(9, "Smederij De Brug", "Peter Jansen", "peter@smederijdebrug.nl", "0180 41 73 88", "Gouda", "Industrieweg 14"),
            new(10, "Restaurant Atlas", "Leonie Smits", "leonie@restaurantatlas.nl", "010 344 77 11", "Den Haag", "Boulevard 22"),
            new(11, "Fietsplein West", "Ruben Kok", "ruben@fietspleinwest.nl", "0172 58 66 33", "Zoetermeer", "Fietspad 3"),
            new(12, "Makelaarshuis Maas", "Anke de Boer", "anke@makelaarshuismaas.nl", "010 590 88 20", "Maassluis", "Havenstraat 27"),
            new(13, "Kantoor 365", "Daan Willems", "daan@kantoor365.nl", "010 663 55 40", "Rotterdam", "Coolsingel 88"),
            new(14, "Kinderopvang Sterren", "Iris Vermeer", "iris@kinderopvangsterren.nl", "0182 33 99 12", "Gorinchem", "Molenstraat 16"),
            new(15, "Waterschappen Midden", "Henk van Loon", "henk@waterschapmidden.nl", "010 711 04 60", "Barendrecht", "Waterweg 2")];
    }

    private static List<ShowcaseTechnician> CreateTechnicians()
    {
        return [
            new(1, "Jasper van Leeuwen", "West", "Vroege ploeg"),
            new(2, "Mila Smit", "Oost", "Dagdienst"),
            new(3, "Noah Bakker", "Zuid", "Dagdienst"),
            new(4, "Sanne de Groot", "Noord", "Late ploeg"),
            new(5, "Sem Jansen", "Randstad", "Vroege ploeg"),
            new(6, "Lotte de Vries", "Regio Rijn", "Dagdienst")];
    }

    private List<ShowcaseWorkOrder> CreateWorkOrders()
    {
        var seed = new Random(271828);
        var orders = new List<ShowcaseWorkOrder>();
        var startDate = DateTime.Today.AddDays(-84);

        for (var index = 0; index < 60; index++)
        {
            var customer = _customers[seed.Next(_customers.Count)];
            var technician = _technicians[seed.Next(_technicians.Count)];
            var dayOffset = seed.Next(0, 84);
            var start = startDate.AddDays(dayOffset).AddHours(7 + seed.Next(10));
            var durationHours = 1 + seed.Next(1, 4);
            var end = start.AddHours(durationHours).AddMinutes(seed.Next(0, 2) * 30);
            var status = (ShowcaseWorkOrderStatus)seed.Next(0, 5);
            var type = (ShowcaseWorkOrderType)seed.Next(0, 4);
            var priority = (ShowcaseWorkOrderPriority)seed.Next(0, 4);
            var amount = 165m + (decimal)(seed.NextDouble() * 1485);

            orders.Add(new ShowcaseWorkOrder
            {
                Id = 10001 + index,
                Number = BuildWorkOrderNumber(10001 + index),
                CustomerId = customer.Id,
                CustomerName = customer.Company,
                Address = customer.Address,
                Type = type,
                Status = status,
                Priority = priority,
                TechnicianId = technician.Id,
                TechnicianName = technician.Name,
                PlannedStart = start,
                PlannedEnd = end,
                Amount = decimal.Round(amount, 2),
                Description = BuildDescription(type, customer.Company)
            });
        }

        _nextWorkOrderId = 10061;
        return orders;
    }

    private List<ShowcaseNotification> CreateNotifications()
    {
        var now = DateTime.Now;
        return [
            new(1, now.AddMinutes(-18), "Nieuwe werkorder", "Van Dijk Installaties vroeg om spoedplanning.", ShowcaseIntent.Info),
            new(2, now.AddMinutes(-42), "Monteur onderweg", "Jasper heeft werkorder WO-10052 bevestigd.", ShowcaseIntent.Success),
            new(3, now.AddHours(-2), "Tijdvak aangepast", "Werkorder WO-10044 is verplaatst naar de middag.", ShowcaseIntent.Warning),
            new(4, now.AddHours(-4), "Afgerond", "WO-10039 is succesvol afgerond en gefactureerd.", ShowcaseIntent.Success),
            new(5, now.AddHours(-7), "Klantvraag", "Zorggroep Maasstad wil een terugbelverzoek.", ShowcaseIntent.Info),
            new(6, now.AddDays(-1), "Inspectie ingepland", "Nieuwe inspectieronde voor Hotel Mercurius.", ShowcaseIntent.Info),
            new(7, now.AddDays(-1).AddHours(-4), "Materiaal geverifieerd", "Ochtendrit heeft voldoende onderdelen geladen.", ShowcaseIntent.Success),
            new(8, now.AddDays(-2), "Annulering geregistreerd", "WO-10012 is geannuleerd op verzoek van de klant.", ShowcaseIntent.Warning)
        ];
    }

    private IReadOnlyList<ShowcaseMonthlyPerformance> BuildMonthlyPerformance()
    {
        var months = Enumerable.Range(0, 6)
            .Select(index => DateTime.Today.AddMonths(-5 + index))
            .Select(date => new DateTime(date.Year, date.Month, 1))
            .ToList();

        var grouped = _workOrders
            .Where(order => order.PlannedStart >= months.First() && order.PlannedStart < months.Last().AddMonths(1))
            .GroupBy(order => new DateTime(order.PlannedStart.Year, order.PlannedStart.Month, 1))
            .ToDictionary(group => group.Key, group => group.ToList());

        return months.Select(month =>
        {
            grouped.TryGetValue(month, out var orders);
            orders ??= [];

            return new ShowcaseMonthlyPerformance(
                month.ToString("MMM", CultureInfo.GetCultureInfo("nl-NL")),
                orders.Sum(item => item.Amount),
                orders.Count,
                orders.Count(item => item.Status == ShowcaseWorkOrderStatus.Afgerond),
                orders.Count(item => item.Type == ShowcaseWorkOrderType.Installatie),
                orders.Count(item => item.Type == ShowcaseWorkOrderType.Reparatie),
                orders.Count(item => item.Type == ShowcaseWorkOrderType.Onderhoud),
                orders.Count(item => item.Type == ShowcaseWorkOrderType.Inspectie));
        }).ToList();
    }

    private IReadOnlyList<ShowcaseTechnicianPerformance> BuildTechnicianPerformance()
    {
        return _technicians
            .Select(technician => new ShowcaseTechnicianPerformance(
                technician.Name,
                _workOrders.Count(order => order.TechnicianId == technician.Id),
                decimal.Round(_workOrders.Where(order => order.TechnicianId == technician.Id).Sum(order => order.Amount), 2),
                _workOrders.Count(order => order.TechnicianId == technician.Id && order.Status == ShowcaseWorkOrderStatus.Afgerond)))
            .ToList();
    }

    private static ShowcaseWorkOrder CloneOrder(ShowcaseWorkOrder order)
    {
        return new ShowcaseWorkOrder
        {
            Id = order.Id,
            Number = order.Number,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            Address = order.Address,
            Type = order.Type,
            Status = order.Status,
            Priority = order.Priority,
            TechnicianId = order.TechnicianId,
            TechnicianName = order.TechnicianName,
            PlannedStart = order.PlannedStart,
            PlannedEnd = order.PlannedEnd,
            Amount = order.Amount,
            Description = order.Description
        };
    }

    private void ApplyLookupData(ShowcaseWorkOrder order)
    {
        var customer = GetCustomer(order.CustomerId);
        if (customer is not null)
        {
            order.CustomerName = customer.Company;
            order.Address = customer.Address;
        }

        var technician = GetTechnician(order.TechnicianId);
        if (technician is not null)
        {
            order.TechnicianName = technician.Name;
        }
    }

    private void AddNotification(string title, string detail, ShowcaseIntent intent)
    {
        _notifications.Insert(0, new ShowcaseNotification(_nextNotificationId++, DateTime.Now, title, detail, intent));
        while (_notifications.Count > 12)
        {
            _notifications.RemoveAt(_notifications.Count - 1);
        }
    }

    private static string BuildWorkOrderNumber(int id) => $"WO-{id}";

    private static string BuildDescription(ShowcaseWorkOrderType type, string company)
    {
        return type switch
        {
            ShowcaseWorkOrderType.Installatie => $"Installatie bij {company} inclusief inbedrijfstelling en uitleg.",
            ShowcaseWorkOrderType.Reparatie => $"Storingsanalyse en herstelwerk voor {company}.",
            ShowcaseWorkOrderType.Onderhoud => $"Onderhoudsbeurt met inspectie en controlelijst bij {company}.",
            _ => $"Visuele inspectie en rapportage voor {company}."
        };
    }

    private static DateTime StartOfCurrentWeek()
    {
        var today = DateTime.Today;
        var offset = ((int)today.DayOfWeek + 6) % 7;
        return today.AddDays(-offset);
    }
}

public enum ShowcaseWorkOrderType
{
    Installatie,
    Reparatie,
    Onderhoud,
    Inspectie
}

public enum ShowcaseWorkOrderStatus
{
    Gepland,
    Onderweg,
    InUitvoering,
    Afgerond,
    Geannuleerd
}

public enum ShowcaseWorkOrderPriority
{
    Laag,
    Normaal,
    Hoog,
    Kritiek
}

public enum ShowcaseIntent
{
    Info,
    Success,
    Warning,
    Danger
}

public sealed class ShowcaseWorkOrder
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public ShowcaseWorkOrderType Type { get; set; }

    public ShowcaseWorkOrderStatus Status { get; set; }

    public ShowcaseWorkOrderPriority Priority { get; set; }

    public int TechnicianId { get; set; }

    public string TechnicianName { get; set; } = string.Empty;

    public DateTime PlannedStart { get; set; }

    public DateTime PlannedEnd { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public int ProgressPercent { get; set; } = 20;

    public int SatisfactionRating { get; set; } = 4;

    public string NotesHtml { get; set; } = "<p>Werkbonnotities worden hier bijgehouden.</p>";

    public string Postcode { get; set; } = "3011AA";

    public int? AssetId { get; set; }

    public string SkillsCsv { get; set; } = "Installatie,Inspectie";

    public List<ShowcaseWorkOrderHistoryEntry> History { get; set; } = [];

    public List<ShowcaseAttachment> Photos { get; set; } = [];

    public string TypeLabel => Type switch
    {
        ShowcaseWorkOrderType.Installatie => "Installatie",
        ShowcaseWorkOrderType.Reparatie => "Reparatie",
        ShowcaseWorkOrderType.Onderhoud => "Onderhoud",
        _ => "Inspectie"
    };

    public string StatusLabel => Status switch
    {
        ShowcaseWorkOrderStatus.Gepland => "Gepland",
        ShowcaseWorkOrderStatus.Onderweg => "Onderweg",
        ShowcaseWorkOrderStatus.InUitvoering => "In uitvoering",
        ShowcaseWorkOrderStatus.Afgerond => "Afgerond",
        _ => "Geannuleerd"
    };

    public string PriorityLabel => Priority switch
    {
        ShowcaseWorkOrderPriority.Laag => "Laag",
        ShowcaseWorkOrderPriority.Normaal => "Normaal",
        ShowcaseWorkOrderPriority.Hoog => "Hoog",
        _ => "Kritiek"
    };

    public string PlanningLabel => $"{PlannedStart:dd-MM HH:mm} - {PlannedEnd:HH:mm}";

    public string TimeWindow => $"{PlannedStart:dd MMM HH:mm} – {PlannedEnd:HH:mm}";
}

public sealed record ShowcaseCustomer(int Id, string Company, string ContactPerson, string Email, string Phone, string City, string Address);

public sealed record ShowcaseTechnician(int Id, string Name, string Region, string Shift);

public sealed record ShowcaseNotification(int Id, DateTime When, string Title, string Detail, ShowcaseIntent Intent);

public sealed record ShowcaseMonthlyPerformance(string Month, decimal Revenue, int Volume, int Completed, int Installations, int Repairs, int Maintenance, int Inspections);

public sealed record ShowcaseTechnicianPerformance(string Technician, int Orders, decimal Revenue, int Completed);

public sealed class ShowcaseSettings
{
    public string DefaultRegion { get; set; } = "Randstad";

    public string PlanningWindow { get; set; } = "Hele dag";

    public string ContactEmail { get; set; } = "planning@werkorders.nl";

    public bool AutoAssignTechnicians { get; set; } = true;

    public bool SmsAlerts { get; set; } = false;

    public bool ShowRevenueExclVat { get; set; } = true;

    public int DefaultTravelMinutes { get; set; } = 35;

    public bool CompactGridDensity { get; set; }
}