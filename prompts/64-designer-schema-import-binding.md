# Prompt 64 — Schema-import en data-binding verbetering

Het data-model in de designer kent alleen handmatig aangemaakt entiteiten en de voorgedefinieerde autoruitschade-seedset. Er is geen manier om een bestaand dataschema te importeren. Daarnaast is de data-binding in het property panel beperkt: geen visuele picker, geen kolommen-editor voor DataGrid, en de formulier-generator is rudimentair. Verbeter beide.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Schema-import

### Import-bronnen
Voeg een "Importeer schema"-knop toe aan het Data-paneel met de volgende importmogelijkheden:

#### JSON Schema
- Upload een `.json`-bestand dat een JSON Schema bevat (draft-07 of 2020-12).
- Parser: extraheer `properties` → `DesignField`s met type-mapping:
  - `string` → `DesignFieldType.String`
  - `integer` → `DesignFieldType.Int`
  - `number` → `DesignFieldType.Decimal`
  - `boolean` → `DesignFieldType.Bool`
  - `string` + `format: "date-time"` → `DesignFieldType.DateTime`
  - `string` + `enum` → `DesignFieldType.Enum` met de enum-waarden
- `required`-array → `IsRequired=true` op de bijbehorende velden.
- `title` → `DesignEntity.Name`; `description` → opslaan als metadata.
- Geneste objecten: maak aparte entiteiten met een naamconventie (`Parent_Child`).
- Arrays van objecten: maak een aparte entiteit + markeer de relatie (FK-veld).

#### OpenAPI / Swagger
- Upload een `.json` of `.yaml` OpenAPI 3.x-specificatie.
- Parser: extraheer entiteiten uit `components.schemas`.
- Hetzelfde type-mapping als JSON Schema (OpenAPI schemas ZIJN JSON Schemas).
- Bonus: extraheer endpoint-informatie (pad, methode) en sla op als metadata op de entiteit — dit is nuttig voor toekomstige API-binding maar wordt in v1 alleen getoond als info, niet functioneel gekoppeld.

#### Voorbeeld-JSON
- Plak of upload een JSON-object of array-van-objecten.
- Parser: infer het schema uit de data (type-detectie per veld: string, number, boolean, datum-patroon, null).
- Bij een array: neem de union van alle velden over alle objecten.
- Genereer een `DesignEntity` met de afgeleide velden.

### Import-UI
- Dialoog met drie tabs: "JSON Schema", "OpenAPI", "Voorbeeld JSON".
- Upload-knop per tab (InputFile) of een textarea om JSON te plakken.
- Na parsing: preview van de afgeleide entiteiten en velden in een tabel.
- De gebruiker kan velden aan/uitzetten, namen wijzigen, types corrigeren, en `IsRequired` toggling VOORDAT de import wordt bevestigd.
- "Importeren"-knop: voegt de entiteiten toe aan het `DesignDataModel` (via command-stack, undo-baar).
- Als er al entiteiten met dezelfde naam bestaan: toon een keuze — "Overschrijven", "Hernoemen", "Annuleren".

## Fase 2 — Binding-picker in het property panel

### Visuele binding-picker
- Vervang de huidige vrije-tekst expressie-invoer voor bindbare parameters door een visuele picker.
- Bij een parameter die gebonden kan worden (registry markeert dit met `IsBindable=true`):
  - Toon een dropdown/picker-icoon naast het invoerveld.
  - Klik opent een picker met twee niveaus:
    1. Kies een entiteit (dropdown met alle entiteiten uit het `DesignDataModel`).
    2. Kies een veld (lijst van velden van de geselecteerde entiteit, gefilterd op compatibel type).
  - Bij collectie-parameters (bijv. `Data` op een DataGrid): alleen entiteit-niveau binding.
  - Bij waarde-parameters (bijv. `Value` op een TextField): entiteit.veld-binding.
- De binding wordt opgeslagen als een expressie in het model (`@entities.Klanten` of `@entities.Klanten.Select(k => k.Klantnaam)`).
- Ongebonden: de picker toont "Geen binding" met een duidelijke optie om terug te schakelen naar vrije invoer.

### DataGrid kolommen-editor
- Wanneer een DataGrid gebonden is aan een entiteit: het property panel toont een speciale **kolommen-editor**:
  - Lijst van alle velden van de gebonden entiteit met checkboxes (aan/uit per kolom).
  - Per aangevinkt veld:
    - Kolomtitel (standaard: `DisplayLabel` of veldnaam)
    - Formaat (tekst, numeriek rechts uitgelijnd met tabular figures, datum, boolean als icoon)
    - Sorteerbaar (checkbox)
    - Filterbaar (checkbox)
    - Breedte (auto of pixels)
  - Drag & drop om kolomvolgorde te wijzigen.
  - "Alle selecteren" / "Geen selecteren"-knoppen.
- Paging-instellingen: pagina-grootte (10/25/50/100), paginering aan/uit.
- Wijzigingen worden opgeslagen als `DesignNode.Children["Columns"]` met per kolom een child-node die de kolomconfiguratie bevat.

## Fase 3 — Formulier-generator verbetering

De bestaande "Formulier genereren"-functie wordt uitgebreid:

### Veld-selectie
- In plaats van alle velden automatisch: toon een selectie-dialoog met checkboxes per veld.
- Standaard: alle verplichte velden geselecteerd + de eerste 5 optionele velden.

### Slimme mapping
- Per veldtype de juiste Agt-wrapper kiezen:
  - `String` → `AgtTextField` (of `AgtTextArea` als de veldnaam "Opmerkingen", "Beschrijving", "Notes" bevat)
  - `Int` / `Decimal` → `AgtNumericField`
  - `Bool` → `AgtSwitch`
  - `DateTime` → `AgtDatePicker`
  - `Enum` → `AgtDropdown` met de enum-waarden als items
  - FK-veld (referentie naar andere entiteit) → `AgtDropdown` gebonden aan de gerelateerde entiteit
- Labels: `DisplayLabel` uit de veldmetadata, met fallback naar de veldnaam.
- Validators: `Required`-validator voor verplichte velden.

### Layout
- Genereer een `AgtCard` met een `AgtPageHeader` (titel = entiteitsnaam) als container.
- Velden in een twee-koloms grid (RadzenRow/Column met span 6) — smalle velden naast elkaar, brede velden (textarea, dropdown met lange waarden) over de volle breedte.
- Onderaan: `AgtFormActions` met een "Opslaan"-knop en een "Annuleren"-knop.

### Resultaat
- De gegenereerde formulier-nodes worden toegevoegd aan het canvas via de command-stack (undo-baar als één batch-command).
- Na generatie: het formulier is zichtbaar op het canvas met seed-data in de velden.

## Fase 4 — Tests

- JSON Schema parser: test met een schema dat alle typen bevat (string, int, number, bool, datetime, enum, nested object, array).
- OpenAPI parser: test met een minimale OpenAPI 3.0 spec met twee schemas.
- Voorbeeld-JSON parser: test met een array van objecten met mixed types.
- Binding-picker bUnit: selecteer een entiteit → velden verschijnen → selecteer een veld → binding wordt opgeslagen in het model.
- Kolommen-editor bUnit: bind DataGrid aan entiteit → kolommen verschijnen → vink velden aan/uit → kolom-configuratie wordt opgeslagen.
- Formulier-generator bUnit: genereer formulier uit entiteit → canvas bevat de juiste componenten met de juiste bindings.
- Import-preview: verifieer dat de preview-tabel de juiste velden en typen toont vóór bevestiging.

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- Handmatig: importeer een JSON Schema → preview toont entiteiten → importeer → entiteiten verschijnen in het Data-paneel met seed-data
- Handmatig: bind een DataGrid aan "Schadedossiers" → kolommen-editor verschijnt → selecteer 5 kolommen → canvas toont de grid met die kolommen en seed-data
- Handmatig: genereer een formulier uit "Werkorder" → canvas toont een formulier met de juiste velden, labels en validators
- Handmatig: plak voorbeeld-JSON → schema wordt afgeleid → entiteit aangemaakt
- Rapporteer: de ondersteunde schema-features per importbron, de binding-expressie-syntax, en de formulier-generator-mappingtabel
