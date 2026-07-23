# Prompt 78d — Export met rijke seeded data en developer toggle

Afhankelijk van: prompt 78b (DesignDataContext — dezelfde seed-logica wordt hergebruikt).

De huidige export genereert een `DesignDataService` die `random.Next()` en `string.Empty` produceert. De rijke, domein-specifieke data uit `DesignDataModelSeeder` (kentekens "AB-123-C", dossiernummers "ATG-2024-00001", klantnamen, factuurdetails) wordt niet meegeëxporteerd. Een ontwikkelaar die het geëxporteerde project draait ziet lege velden.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Verrijk GenerateSeedLiteral met domein-data

### Probleem
`ProjectExporter.GenerateSeedLiteral()` genereert generieke waarden:
```csharp
DesignFieldType.String => "string.Empty",
DesignFieldType.Int => "random.Next(1, 1000)",
```

Terwijl `DesignDataModelSeeder.GenerateValue()` rijke, entity-specifieke waarden heeft:
```csharp
("Schadedossier", "Dossiernummer", _) => $"ATG-2024-{index + 1:00000}",
("Klant", "Email", _) => $"klant{index + 1}@voorbeeld.nl",
```

### Fix

Pas `ProjectExporter.GenerateSeedLiteral()` aan zodat het de entity-naam meekrijgt en entity-specifieke expressies genereert:

```csharp
private static string GenerateSeedLiteral(DesignField field, string entityName)
    => (entityName, field.Name, field.Type) switch
    {
        // Schadedossier
        ("Schadedossier", "Dossiernummer", _) => "$\"ATG-2024-{index + 1:00000}\"",
        ("Schadedossier", "Status", _) => "Pick(random, new[] { \"Nieuw\", \"Ingepland\", \"InBehandeling\", \"Gereed\", \"Gefactureerd\", \"Gesloten\" })",
        ("Schadedossier", "Schadesoort", _) => "Pick(random, new[] { \"Sterretje\", \"Ster\", \"Barst\", \"TotaalBreuk\" })",
        ("Schadedossier", "GlasType", _) => "Pick(random, new[] { \"Voorruit\", \"Achterruit\", \"Zijruit\", \"Dakraam\" })",
        ("Schadedossier", "Actie", _) => "Pick(random, new[] { \"Reparatie\", \"Vervanging\" })",
        ("Schadedossier", "Opmerkingen", _) => "$\"Dossier {index + 1} opmerking.\"",

        // Klant
        ("Klant", "Klantnaam", _) => "$\"Klant {index + 1}\"",
        ("Klant", "KlantType", _) => "Pick(random, new[] { \"Particulier\", \"Lease\", \"Wagenparkbeheerder\", \"Dealer\" })",
        ("Klant", "Telefoonnummer", _) => "$\"06 {random.Next(10,99)} {random.Next(10,99)} {random.Next(10,99)}\"",
        ("Klant", "Email", _) => "$\"klant{index + 1}@voorbeeld.nl\"",
        ("Klant", "Verzekeraar", _) => "Pick(random, new[] { \"Centraal Beheer\", \"Interpolis\", \"FBTO\", \"Univé\", \"OHRA\" })",
        ("Klant", "Polisnummer", _) => "$\"POL-{random.Next(100000, 999999)}\"",
        ("Klant", "Woonplaats", _) => "Pick(random, new[] { \"Amsterdam\", \"Rotterdam\", \"Utrecht\", \"Den Haag\", \"Eindhoven\" })",

        // Voertuig
        ("Voertuig", "Kenteken", _) => "$\"{Pick(random, new[]{\"AB\",\"CD\",\"EF\",\"GH\"})}-{random.Next(100,999)}-{Pick(random, new[]{\"A\",\"B\",\"C\",\"D\"})}\"",
        ("Voertuig", "Merk", _) => "Pick(random, new[] { \"Volkswagen\", \"Toyota\", \"BMW\", \"Peugeot\", \"Opel\", \"Škoda\" })",
        ("Voertuig", "Model", _) => "Pick(random, new[] { \"Golf\", \"Corolla\", \"3 Serie\", \"208\", \"Astra\", \"Octavia\" })",
        ("Voertuig", "Kleur", _) => "Pick(random, new[] { \"Grijs\", \"Wit\", \"Zwart\", \"Blauw\", \"Rood\", \"Zilver\" })",

        // Werkorder
        ("Werkorder", "DossierNummer", _) => "$\"ATG-2024-{index + 1:00000}\"",
        ("Werkorder", "WerkorderType", _) => "Pick(random, new[] { \"RuitReparatie\", \"RuitVervanging\", \"AdasKalibratie\" })",
        ("Werkorder", "Monteur", _) => "Pick(random, new[] { \"J. de Vries\", \"M. Jansen\", \"S. Bakker\", \"T. Visser\" })",
        ("Werkorder", "Status", _) => "Pick(random, new[] { \"Gepland\", \"Onderweg\", \"Bezig\", \"Afgerond\" })",

        // Factuur
        ("Factuur", "FactuurNummer", _) => "$\"F-2024-{index + 1:00000}\"",
        ("Factuur", "BetaalStatus", _) => "Pick(random, new[] { \"Open\", \"Verzonden\", \"BetaaldKlant\", \"Volledig\" })",

        // Generieke fallbacks
        _ => field.Type switch
        {
            DesignFieldType.Int => "random.Next(1, 1000)",
            DesignFieldType.Decimal => "decimal.Round((decimal)(random.NextDouble() * 1000), 2)",
            DesignFieldType.Bool => "random.NextDouble() > 0.5",
            DesignFieldType.DateTime => "DateTime.Today.AddDays(-random.Next(0, 60))",
            DesignFieldType.Enum => field.EnumValues.Count > 0 ? $"Pick(random, new[] {{ {string.Join(", ", field.EnumValues.Select(v => $"\"{v}\""))} }})" : "string.Empty",
            _ => "$\"{field.Name} {index + 1}\""
        }
    };
```

Voeg ook de `Pick` helper toe aan de gegenereerde `DesignDataService`:

```csharp
lines.Add("    private static T Pick<T>(Random random, IReadOnlyList<T> items)");
lines.Add("        => items[random.Next(items.Count)];");
```

Update alle call-sites van `GenerateSeedLiteral` om `entityName` mee te geven.

---

## Fase 2 — UseSeedData toggle in export

### Doel
De ontwikkelaar moet de seeded data aan/uit kunnen zetten via `appsettings.json`.

### Implementatie

**Stap 1: Voeg een checkbox toe aan de export-dialog.**

In `DesignerShell.razor`, in de export-dialog sectie:

```razor
<div class="designer-modal__content">
    @* bestaande export-opties *@
    <AgtSwitch Label="Voorbeelddata meegeven"
               Value="@_exportIncludeSeedData"
               ValueChanged="v => _exportIncludeSeedData = v" />
    <p class="designer-export__hint">
        Het geëxporteerde project bevat realistische voorbeelddata.
        Schakel uit via <code>appsettings.json → "UseSeedData": false</code>.
    </p>
</div>
```

In `DesignerShell.razor.cs`:
```csharp
private bool _exportIncludeSeedData = true;
```

**Stap 2: Pas `ProjectExporter.ExportProject` aan.**

Voeg een `includeSeedData` parameter toe:
```csharp
public ExportResult ExportProject(
    DesignDocument document,
    string projectName,
    string themeFamily = "plum",
    bool includeSeedData = true)
```

Wanneer `includeSeedData` false is, genereer een lege `DesignDataService` die lege collecties retourneert.

**Stap 3: Voeg `appsettings.json` toe aan de export-templates.**

Maak `Export/Templates/appsettings.json.template`:
```json
{
  "UseSeedData": __USE_SEED_DATA__
}
```

In `GetStarterTemplateFiles`, voeg toe:
```csharp
[$"{projectName}/appsettings.json"] = LoadTemplateText("appsettings.json.template", projectName, themeFamily)
    .Replace("__USE_SEED_DATA__", includeSeedData ? "true" : "false"),
```

**Stap 4: Pas `Program.template` aan.**

Voeg configuratie-lezing toe zodat de developer seed data kan uitschakelen:
```csharp
var useSeedData = builder.Configuration.GetValue("UseSeedData", true);
if (useSeedData)
{
    builder.Services.AddSingleton<DesignDataService>();
}
```

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Export/ProjectExporter.cs` | 1, 2 | `GenerateSeedLiteral` verrijkt, `includeSeedData` parameter, `Pick` helper |
| `Components/DesignerShell.razor` | 2 | Export-dialog checkbox |
| `Components/DesignerShell.razor.cs` | 2 | `_exportIncludeSeedData` veld, doorgeven aan exporter |
| `Export/Templates/appsettings.json.template` | 2 | NIEUW |
| `Export/Templates/Program.template` | 2 | UseSeedData configuratie |

## Verificatie

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Export met "Voorbeelddata meegeven" AAN | ZIP bevat DesignDataService met rijke data |
| 2 | `dotnet run` geëxporteerd project | Kentekens, dossiernummers, klantnamen zichtbaar |
| 3 | Zet `UseSeedData: false` in appsettings.json | App start zonder seed data |
| 4 | Export met toggle UIT | DesignDataService retourneert lege collecties |
| 5 | Gegenereerde code compileert | `dotnet build` op geëxporteerd project slaagt |
