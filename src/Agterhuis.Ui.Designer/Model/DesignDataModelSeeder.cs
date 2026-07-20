using System.Globalization;

namespace Agterhuis.Ui.Designer.Model;

public static class DesignDataModelSeeder
{
    private static readonly string[] InsuranceCompanies = ["Centraal Beheer", "Interpolis", "FBTO", "Univé", "OHRA", "Allianz", "NN", "ASR"];
    private static readonly string[] LeaseCompanies = ["LeasePlan", "ALD Automotive", "Alphabet", "Arval", "Athlon"];
    private static readonly string[] CarBrands = ["Volkswagen", "Toyota", "BMW", "Peugeot", "Opel", "Škoda", "Ford", "Audi", "Mercedes-Benz", "Kia"];
    private static readonly Dictionary<string, string[]> CarModels = new(StringComparer.Ordinal)
    {
        ["Volkswagen"] = ["Golf", "Passat", "Tiguan", "ID.3"],
        ["Toyota"] = ["Yaris", "Corolla", "C-HR", "RAV4"],
        ["BMW"] = ["1 Serie", "3 Serie", "X1", "X3"],
        ["Peugeot"] = ["208", "308", "2008", "3008"],
        ["Opel"] = ["Corsa", "Astra", "Mokka", "Grandland"],
        ["Škoda"] = ["Fabia", "Octavia", "Karoq", "Enyaq"],
        ["Ford"] = ["Fiesta", "Focus", "Kuga", "Mustang Mach-E"],
        ["Audi"] = ["A3", "A4", "Q3", "Q5"],
        ["Mercedes-Benz"] = ["A-Klasse", "C-Klasse", "GLA", "EQB"],
        ["Kia"] = ["Picanto", "Ceed", "Niro", "Sportage"]
    };

    public static DesignDataModel CreateDefault()
    {
        var model = new DesignDataModel
        {
            Seed = 42,
            RowCount = 25,
            Entities =
            [
                BuildSchadedossier(),
                BuildKlant(),
                BuildVoertuig(),
                BuildWerkorder(),
                BuildFactuur(),
                BuildVoorraad()
            ]
        };

        return model;
    }

    public static IReadOnlyList<DesignSeedRow> GeneratePreview(DesignDataModel model, string entityName)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);

        var entity = model.Entities.FirstOrDefault(item => string.Equals(item.Name, entityName, StringComparison.OrdinalIgnoreCase));
        if (entity is null)
        {
            return [];
        }

        return GenerateEntityRows(entity, Math.Max(1, entity.Seed.RowCount), entity.Seed.Seed)
            .Take(5)
            .ToList();
    }

    public static IReadOnlyList<DesignSeedRow> GenerateEntityRows(DesignEntity entity, int rowCount, int seed)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var rnd = new Random(seed);
        return Enumerable.Range(0, Math.Max(1, rowCount))
            .Select(index => new DesignSeedRow(entity.Name, GenerateRow(entity, rnd, index)))
            .ToList();
    }

    private static IReadOnlyDictionary<string, object?> GenerateRow(DesignEntity entity, Random rnd, int index)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var field in entity.Fields)
        {
            values[field.Name] = GenerateValue(entity.Name, field, rnd, index, values);
        }

        return values;
    }

    private static object? GenerateValue(string entityName, DesignField field, Random rnd, int index, IReadOnlyDictionary<string, object?> currentValues)
        => (entityName, field.Name, field.Type) switch
        {
            ("Schadedossier", "Dossiernummer", _) => $"ATG-2024-{index + 1:00000}",
            ("Schadedossier", "Status", _) => Pick(rnd, ["Nieuw", "Ingepland", "InBehandeling", "Gereed", "Gefactureerd", "Gesloten"]),
            ("Schadedossier", "Schadedatum", _) => DateTime.Today.AddDays(-rnd.Next(1, 120)).AddHours(rnd.Next(7, 18)),
            ("Schadedossier", "AanmaakDatum", _) => DateTime.Today.AddDays(-rnd.Next(2, 125)).AddHours(rnd.Next(7, 18)),
            ("Schadedossier", "Schadesoort", _) => Pick(rnd, ["Sterretje", "Ster", "Barst", "TotaalBreuk"]),
            ("Schadedossier", "GlasType", _) => Pick(rnd, ["Voorruit", "Achterruit", "Zijruit", "Dakraam"]),
            ("Schadedossier", "Actie", _) => Pick(rnd, ["Reparatie", "Vervanging"]),
            ("Schadedossier", "VoorexpertiseNodig", _) => rnd.NextDouble() < 0.3,
            ("Schadedossier", "VoorexpertiseStatus", _) => Pick(rnd, ["NietNodig", "Aangevraagd", "Goedgekeurd", "Afgewezen"]),
            ("Schadedossier", "AdasHerkalibratie", _) => false,
            ("Schadedossier", "AdasStatus", _) => Pick(rnd, ["NietNodig", "Ingepland", "Uitgevoerd", "NietMogelijkDoorverwijzing"]),
            ("Schadedossier", "Opmerkingen", _) => $"Dossier {index + 1} met vaste seed.",
            ("Klant", "KlantId", _) => index + 1,
            ("Klant", "Klantnaam", _) => $"Klant {index + 1}",
            ("Klant", "KlantType", _) => PickWeighted(rnd, [("Particulier", 40), ("Lease", 35), ("Wagenparkbeheerder", 15), ("Dealer", 10)]),
            ("Klant", "Telefoonnummer", _) => $"06 {rnd.Next(10, 99)} {rnd.Next(10, 99)} {rnd.Next(10, 99)}",
            ("Klant", "Email", _) => $"klant{index + 1}@voorbeeld.nl",
            ("Klant", "Verzekeraar", _) => Pick(rnd, InsuranceCompanies),
            ("Klant", "Polisnummer", _) => $"POL-{rnd.Next(100000, 999999)}",
            ("Klant", "Eigenrisico", _) => Math.Round((decimal)(rnd.NextDouble() * 750), 2),
            ("Klant", "LeasemaatschappijNaam", _) => Pick(rnd, LeaseCompanies),
            ("Klant", "Adres", _) => $"Voorbeeldstraat {rnd.Next(1, 200)}",
            ("Klant", "Postcode", _) => $"{rnd.Next(1000, 9999)} {Pick(rnd, ["AB", "CD", "EF", "GH", "JK", "LM", "NP", "RS", "TU", "VW"]) }",
            ("Klant", "Woonplaats", _) => Pick(rnd, ["Amsterdam", "Rotterdam", "Utrecht", "Den Haag", "Eindhoven", "Groningen", "Arnhem", "Leiden"]),
            ("Voertuig", "VoertuigId", _) => index + 1,
            ("Voertuig", "Kenteken", _) => $"{Pick(rnd, ["AB", "CD", "EF", "GH", "JK", "LM", "NP", "RS"])}-{rnd.Next(100, 999)}-{Pick(rnd, ["A", "B", "C", "D", "E", "F"]) }",
            ("Voertuig", "VinNummer", _) => BuildVin(index),
            ("Voertuig", "Merk", _) => Pick(rnd, CarBrands),
            ("Voertuig", "Model", _) => Pick(rnd, CarModels[(string)currentValues["Merk"]!]),
            ("Voertuig", "Bouwjaar", _) => rnd.Next(2014, 2025),
            ("Voertuig", "Kleur", _) => Pick(rnd, ["Grijs", "Wit", "Zwart", "Blauw", "Rood", "Zilver"]),
            ("Voertuig", "HeeftAdas", _) => rnd.NextDouble() < 0.55,
            ("Voertuig", "AdasSensoren", _) => Pick(rnd, ["Lane Assist, ACC, AEB", "ACC, Traffic Sign Assist", "AEB, Park Assist", "Lane Assist, Blind Spot"]),
            ("Voertuig", "GlascoatingAanwezig", _) => rnd.NextDouble() < 0.35,
            ("Voertuig", "Ruitsticker", _) => rnd.NextDouble() < 0.25,
            ("Werkorder", "WerkorderId", _) => index + 1,
            ("Werkorder", "DossierNummer", _) => $"ATG-2024-{index + 1:00000}",
            ("Werkorder", "WerkorderType", _) => Pick(rnd, ["RuitReparatie", "RuitVervanging", "AdasKalibratie", "NalevingControle"]),
            ("Werkorder", "PlanDatum", _) => DateTime.Today.AddDays(rnd.Next(0, 30)).AddHours(rnd.Next(7, 17)),
            ("Werkorder", "PlanTijdvak", _) => Pick(rnd, ["Ochtend", "Middag", "Dag"]),
            ("Werkorder", "Monteur", _) => Pick(rnd, ["J. de Vries", "M. Jansen", "S. Bakker", "T. Visser"]),
            ("Werkorder", "Locatie", _) => Pick(rnd, ["Werkplaats", "OpLocatie"]),
            ("Werkorder", "OpLocatieAdres", _) => $"Serviceweg {rnd.Next(1, 50)}",
            ("Werkorder", "Artikelnummer", _) => $"ART-{rnd.Next(1000, 9999)}",
            ("Werkorder", "Leverancier", _) => Pick(rnd, ["Saint-Gobain Sekurit", "Pilkington", "AGC"]),
            ("Werkorder", "Status", _) => Pick(rnd, ["Gepland", "Onderweg", "Bezig", "Afgerond", "Geannuleerd"]),
            ("Werkorder", "AdasKalibratieTool", _) => Pick(rnd, ["Hella Gutmann", "Bosch DAS 3000"]),
            ("Werkorder", "UrenBesteed", _) => Math.Round((decimal)(1 + rnd.NextDouble() * 5), 1),
            ("Werkorder", "Opmerkingen", _) => $"Werkordernotitie {index + 1}.",
            ("Factuur", "FactuurId", _) => index + 1,
            ("Factuur", "FactuurNummer", _) => $"F-2024-{index + 1:00000}",
            ("Factuur", "DossierNummer", _) => $"ATG-2024-{index + 1:00000}",
            ("Factuur", "FactuurDatum", _) => DateTime.Today.AddDays(-rnd.Next(0, 60)).Date,
            ("Factuur", "BedragExBtw", _) => Math.Round((decimal)(75 + rnd.NextDouble() * 1125), 2),
            ("Factuur", "BtwBedrag", _) => Math.Round(((decimal?)currentValues.GetValueOrDefault("BedragExBtw") ?? 0m) * 0.21m, 2),
            ("Factuur", "BedragInclBtw", _) => Math.Round(((decimal?)currentValues.GetValueOrDefault("BedragExBtw") ?? 0m) * 1.21m, 2),
            ("Factuur", "Eigenrisico", _) => Math.Round((decimal)(rnd.NextDouble() * 250), 2),
            ("Factuur", "VerzekeraarAandeel", _) => Math.Round((decimal)(rnd.NextDouble() * 900), 2),
            ("Factuur", "BetaalStatus", _) => Pick(rnd, ["Open", "Verzonden", "BetaaldKlant", "BetaaldVerzekeraar", "Volledig"]),
            ("Factuur", "Creditnota", _) => rnd.NextDouble() < 0.1,
            ("Voorraad", "ArtikelId", _) => index + 1,
            ("Voorraad", "Artikelnummer", _) => $"ART-{index + 1:0000}",
            ("Voorraad", "Omschrijving", _) => $"Voorruit {Pick(rnd, CarBrands)} {Pick(rnd, new[] { "Golf", "Corolla", "3 Serie", "208", "Astra", "Octavia" })} {rnd.Next(2018, 2025)} ADAS",
            ("Voorraad", "GlasType", _) => Pick(rnd, ["Voorruit", "Achterruit", "Zijruit", "Dakraam"]),
            ("Voorraad", "MetAdas", _) => rnd.NextDouble() < 0.4,
            ("Voorraad", "VoorraadAantal", _) => rnd.Next(0, 25),
            ("Voorraad", "MinimumVoorraad", _) => rnd.Next(1, 8),
            ("Voorraad", "InkoopPrijs", _) => Math.Round((decimal)(50 + rnd.NextDouble() * 450), 2),
            ("Voorraad", "VerkoopPrijs", _) => Math.Round((decimal)(100 + rnd.NextDouble() * 700), 2),
            ("Voorraad", "Leverancier", _) => Pick(rnd, ["Saint-Gobain Sekurit", "Pilkington", "AGC"]),
            ("Voorraad", "LevertijdDagen", _) => rnd.Next(1, 10),
            _ => field.Type switch
            {
                DesignFieldType.String => $"{field.Name} {index + 1}",
                DesignFieldType.Int => index + 1,
                DesignFieldType.Decimal => Math.Round((decimal)(rnd.NextDouble() * 100), 2),
                DesignFieldType.Bool => rnd.NextDouble() < 0.5,
                DesignFieldType.DateTime => DateTime.Today.AddDays(-rnd.Next(0, 365)),
                DesignFieldType.Enum => field.EnumValues.Count > 0 ? Pick(rnd, field.EnumValues.ToArray()) : field.Name,
                _ => field.Name
            }
        };

    private static string BuildVin(int index)
        => $"WVWZZZ{index:0000000000000}"[..17].PadRight(17, 'X');

    private static T Pick<T>(Random rnd, IReadOnlyList<T> items)
        => items[rnd.Next(items.Count)];

    private static string PickWeighted(Random rnd, IReadOnlyList<(string Value, int Weight)> items)
    {
        var total = items.Sum(item => item.Weight);
        var roll = rnd.Next(total);
        var running = 0;

        foreach (var item in items)
        {
            running += item.Weight;
            if (roll < running)
            {
                return item.Value;
            }
        }

        return items[^1].Value;
    }

    private static DesignEntity BuildSchadedossier()
        => new()
        {
            Name = "Schadedossier",
            PluralName = "Schadedossiers",
            Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 },
            Fields =
            [
                Field("Dossiernummer", DesignFieldType.String, true, "ATG-2024-NNNNN"),
                EnumField("Status", true, ["Nieuw", "Ingepland", "InBehandeling", "Gereed", "Gefactureerd", "Gesloten"]),
                Field("Schadedatum", DesignFieldType.DateTime, true),
                Field("AanmaakDatum", DesignFieldType.DateTime, true),
                EnumField("Schadesoort", true, ["Sterretje", "Ster", "Barst", "TotaalBreuk"]),
                EnumField("GlasType", true, ["Voorruit", "Achterruit", "Zijruit", "Dakraam"]),
                EnumField("Actie", true, ["Reparatie", "Vervanging"]),
                Field("VoorexpertiseNodig", DesignFieldType.Bool, true),
                EnumField("VoorexpertiseStatus", true, ["NietNodig", "Aangevraagd", "Goedgekeurd", "Afgewezen"]),
                Field("AdasHerkalibratie", DesignFieldType.Bool, true),
                EnumField("AdasStatus", false, ["NietNodig", "Ingepland", "Uitgevoerd", "NietMogelijkDoorverwijzing"]),
                Field("Opmerkingen", DesignFieldType.String, false)
            ]
        };

    private static DesignEntity BuildKlant()
        => new()
        {
            Name = "Klant",
            PluralName = "Klanten",
            Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 },
            Fields =
            [
                Field("KlantId", DesignFieldType.Int, true),
                Field("Klantnaam", DesignFieldType.String, true),
                EnumField("KlantType", true, ["Particulier", "Lease", "Wagenparkbeheerder", "Dealer"]),
                Field("Telefoonnummer", DesignFieldType.String, true),
                Field("Email", DesignFieldType.String, false),
                Field("Verzekeraar", DesignFieldType.String, false),
                Field("Polisnummer", DesignFieldType.String, false),
                Field("Eigenrisico", DesignFieldType.Decimal, false),
                Field("LeasemaatschappijNaam", DesignFieldType.String, false),
                Field("Adres", DesignFieldType.String, false),
                Field("Postcode", DesignFieldType.String, false, "1234 AB"),
                Field("Woonplaats", DesignFieldType.String, false)
            ]
        };

    private static DesignEntity BuildVoertuig()
        => new()
        {
            Name = "Voertuig",
            PluralName = "Voertuigen",
            Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 },
            Fields =
            [
                Field("VoertuigId", DesignFieldType.Int, true),
                Field("Kenteken", DesignFieldType.String, true, "AB-123-C"),
                Field("VinNummer", DesignFieldType.String, true),
                Field("Merk", DesignFieldType.String, true),
                Field("Model", DesignFieldType.String, true),
                Field("Bouwjaar", DesignFieldType.Int, true),
                Field("Kleur", DesignFieldType.String, false),
                Field("HeeftAdas", DesignFieldType.Bool, true),
                Field("AdasSensoren", DesignFieldType.String, false),
                Field("GlascoatingAanwezig", DesignFieldType.Bool, false),
                Field("Ruitsticker", DesignFieldType.Bool, false)
            ]
        };

    private static DesignEntity BuildWerkorder()
        => new()
        {
            Name = "Werkorder",
            PluralName = "Werkorders",
            Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 },
            Fields =
            [
                Field("WerkorderId", DesignFieldType.Int, true),
                Field("DossierNummer", DesignFieldType.String, true),
                EnumField("WerkorderType", true, ["RuitReparatie", "RuitVervanging", "AdasKalibratie", "NalevingControle"]),
                Field("PlanDatum", DesignFieldType.DateTime, true),
                EnumField("PlanTijdvak", true, ["Ochtend", "Middag", "Dag"]),
                Field("Monteur", DesignFieldType.String, true),
                EnumField("Locatie", true, ["Werkplaats", "OpLocatie"]),
                Field("OpLocatieAdres", DesignFieldType.String, false),
                Field("Artikelnummer", DesignFieldType.String, false),
                Field("Leverancier", DesignFieldType.String, false),
                EnumField("Status", true, ["Gepland", "Onderweg", "Bezig", "Afgerond", "Geannuleerd"]),
                Field("AdasKalibratieTool", DesignFieldType.String, false),
                Field("UrenBesteed", DesignFieldType.Decimal, false),
                Field("Opmerkingen", DesignFieldType.String, false)
            ]
        };

    private static DesignEntity BuildFactuur()
        => new()
        {
            Name = "Factuur",
            PluralName = "Facturen",
            Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 },
            Fields =
            [
                Field("FactuurId", DesignFieldType.Int, true),
                Field("FactuurNummer", DesignFieldType.String, true),
                Field("DossierNummer", DesignFieldType.String, true),
                Field("FactuurDatum", DesignFieldType.DateTime, true),
                Field("BedragExBtw", DesignFieldType.Decimal, true),
                Field("BtwBedrag", DesignFieldType.Decimal, true),
                Field("BedragInclBtw", DesignFieldType.Decimal, true),
                Field("Eigenrisico", DesignFieldType.Decimal, false),
                Field("VerzekeraarAandeel", DesignFieldType.Decimal, false),
                EnumField("BetaalStatus", true, ["Open", "Verzonden", "BetaaldKlant", "BetaaldVerzekeraar", "Volledig"]),
                Field("Creditnota", DesignFieldType.Bool, false)
            ]
        };

    private static DesignEntity BuildVoorraad()
        => new()
        {
            Name = "Voorraad",
            PluralName = "Voorraadartikelen",
            Seed = new DesignSeedSettings { RowCount = 30, Seed = 42 },
            Fields =
            [
                Field("ArtikelId", DesignFieldType.Int, true),
                Field("Artikelnummer", DesignFieldType.String, true),
                Field("Omschrijving", DesignFieldType.String, true),
                EnumField("GlasType", true, ["Voorruit", "Achterruit", "Zijruit", "Dakraam"]),
                Field("MetAdas", DesignFieldType.Bool, true),
                Field("VoorraadAantal", DesignFieldType.Int, true),
                Field("MinimumVoorraad", DesignFieldType.Int, true),
                Field("InkoopPrijs", DesignFieldType.Decimal, true),
                Field("VerkoopPrijs", DesignFieldType.Decimal, true),
                Field("Leverancier", DesignFieldType.String, true),
                Field("LevertijdDagen", DesignFieldType.Int, false)
            ]
        };

    private static DesignField Field(string name, DesignFieldType type, bool required, string? pattern = null)
        => new() { Name = name, Type = type, IsRequired = required, Pattern = pattern };

    private static DesignField EnumField(string name, bool required, IEnumerable<string> values)
        => new() { Name = name, Type = DesignFieldType.Enum, IsRequired = required, EnumValues = values.ToList() };
}