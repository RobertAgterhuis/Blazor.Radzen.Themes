# Prompt 66 — Designer UX-laag voor niet-technische gebruikers

De designer is na prompt 65 technisch functioneel, maar nog niet intuïtief voor een niet-technische gebruiker die UI-mockups wil samenstellen. Deze prompt voegt de "menselijke laag" toe: begrijpelijke namen, visuele hulpmiddelen, een startscherm met doel-gerichte keuzes, een preview-modus, en vereenvoudigde property-editing. Het eindresultaat is een tool waar een product-owner of business-analist een scherm bij elkaar kan klikken en een developer vervolgens het geëxporteerde project kan oppakken.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Startscherm met doelgerichte keuzes ("Wat wil je maken?")

Het huidige startscherm toont een template-dropdown in de toolbar. Vervang dit door een visueel startscherm dat verschijnt wanneer er geen document geladen is.

### Startscherm layout
- Toon het startscherm wanneer `_commands.Document` een ongewijzigd "Untitled" Blank-document is EN er geen `NameQuery`/`TemplateQuery` is.
- Verberg toolbar, panelen en canvas — alleen het startscherm is zichtbaar.

### Inhoud
1. **Hero-sectie**: "Wat wil je ontwerpen?" met een korte beschrijving.
2. **Template-kaarten** (grid van 6 kaarten, één per `DesignDocumentTemplateKind`):

| Template | Icoon | Titel | Beschrijving | Voorbeeld-afbeelding |
|---|---|---|---|---|
| Blank | `draft` | Leeg canvas | Begin helemaal opnieuw | Leeg grid |
| FormPage | `edit_note` | Invulformulier | Velden, validatie en opslaan | Mini-formulier preview |
| ListCrud | `table_rows` | Overzichtstabel | Zoeken, filteren en bewerken | Mini-tabel preview |
| MasterDetail | `view_sidebar` | Overzicht + detail | Lijst links, detail rechts | Twee-panelen preview |
| Wizard | `assistant` | Stappenwizard | Gebruiker door stappen leiden | Stappen-indicator preview |
| Dashboard | `dashboard` | Dashboard | KPI's, grafieken en samenvatting | Dashboard preview |

3. **Recente ontwerpen** (onder de templates): lijst van opgeslagen documenten met naam, datum, en "Open" knop.
4. **Import-sectie** (klein, onderaan): "Bestand importeren" knop voor `.agtdesign`-bestanden.

### Interactie
- Klik op een template-kaart → start een nieuw document met die template, toon de editor.
- Klik op een recent ontwerp → laad dat document, toon de editor.
- De template-kaarten tonen een hover-preview: een verkleinde, read-only rendering van de template in het kaart-formaat.

### Implementatie
- Voeg een `bool _showStartScreen` property toe aan `DesignerShell`.
- Voeg een `DesignerStartScreen` component toe in de Designer RCL met de startscherm-markup.
- Parameters: `TemplateOptions`, `RecentDocuments`, `OnTemplateSelected`, `OnDocumentSelected`, `OnImportRequested`.

## Fase 2 — Palette hernoemen en categoriseren op doel

De palette toont componentnamen als technische identifiers (e.g. "AgtAutoComplete" → "Auto Complete"). Hernoem naar Nederlandse, doelgerichte namen en hergroepeer.

### Nieuwe categorieën en namen

| Huidige Category | Huidige DisplayName | Nieuwe Category | Nieuwe DisplayName | Icoon |
|---|---|---|---|---|
| Forms & Inputs | Text Field | Invoer | Tekstveld | `text_fields` |
| Forms & Inputs | Numeric Field | Invoer | Getallenveld | `pin` |
| Forms & Inputs | Date Picker | Invoer | Datumkiezer | `calendar_today` |
| Forms & Inputs | Dropdown | Invoer | Keuzelijst | `arrow_drop_down_circle` |
| Forms & Inputs | Checkbox | Invoer | Aanvinkveld | `check_box` |
| Forms & Inputs | Switch | Invoer | Schakelaar | `toggle_on` |
| Forms & Inputs | Auto Complete | Invoer | Zoek & selecteer | `manage_search` |
| Forms & Inputs | File Upload | Invoer | Bestand uploaden | `upload_file` |
| Forms & Inputs | Form Actions | Invoer | Formulierknoppen | `done_all` |
| Layout & Display | Card | Opmaak | Kaart | `crop_portrait` |
| Layout & Display | Breadcrumb | Opmaak | Kruimelpad | `arrow_right` |
| Layout & Display | Drawer | Opmaak | Zijpaneel | `menu_open` |
| Layout & Display | Density Toggle | — | — (verberg uit palette) | — |
| Layout & Display | Command Palette | — | — (verberg uit palette) | — |
| Feedback & Overlays | Alert | Meldingen | Melding | `info` |
| Feedback & Overlays | Badge | Meldingen | Badge | `fiber_manual_record` |
| Feedback & Overlays | Empty State | Meldingen | Lege staat | `inbox` |
| Feedback & Overlays | Loading Panel | Meldingen | Laadpaneel | `hourglass_empty` |
| Feedback & Overlays | Delta | Meldingen | Verschil-indicator | `trending_up` |
| Data & Scheduling | Data Grid | Gegevens | Tabel | `table_chart` |

### Implementatie
- Voeg `DesignerDisplayName` en `DesignerCategory` properties toe aan `DesignerComponentDescriptor`:
  ```csharp
  public sealed record DesignerComponentDescriptor(
      ...,
      string? DesignerDisplayName = null,
      string? DesignerCategory = null);
  ```
- Maak een `DesignerComponentDisplayMap` dictionary die per `ComponentType` de Nederlandse naam en categorie bevat.
- In de palette: gebruik `DesignerDisplayName ?? DisplayName` en `DesignerCategory ?? Category`.
- Verberg componenten waar `DesignerCategory` is ingesteld op `null` of leeg (Density Toggle, Command Palette).

### Palette tooltip
- Bij hover op een palette-item: toon een tooltip met:
  - De Nederlandse naam (vet)
  - Eén-regel beschrijving (bv. "Een tekstveld voor vrije invoer")
  - Het technische componenttype (klein, grijs: `AgtTextField`)

## Fase 3 — Vereenvoudigd property panel met "Simpel / Geavanceerd" toggle

### Simpele modus (standaard)
Wanneer een component geselecteerd is, toont het property panel standaard alleen de **essentiële parameters**:

| Componenttype | Simpele parameters |
|---|---|
| Alle invoer-componenten | Label, Placeholder, Value |
| Knoppen | Text, Icon, ButtonStyle (als visuele keuze) |
| Card | Title |
| DataGrid | Data (als entiteit-keuze), paginering |
| Alert | Intent (als icoon-keuze: ℹ️ ⚠️ ❌ ✅) |
| Badge | Text, Intent |
| Empty State | Title, Description, Icon |
| Layout-componenten | Geen (of minimaal) |

### Geavanceerde modus
- Toggle "Geavanceerd" bovenaan het property panel.
- Geavanceerd toont ALLE parameters (huidige gedrag).
- EventCallback parameters worden altijd verborgen (ook in geavanceerde modus) — ze zijn niet relevant voor mockup-bouwers.

### Visuele verbeteringen
- **ButtonStyle** als visuele keuze: 4 mini-knoppen die de stijl tonen (Primary blauw, Secondary grijs, Danger rood, Success groen) in plaats van een tekst-dropdown.
- **Intent** als icoon-keuze: gekleurde icoon-knoppen (Info blauw, Warning oranje, Danger rood, Success groen).
- **Icon** als icoon-picker: een zoekbaar grid van beschikbare Material Icons (gebruik de `DesignerIconPicker` — nieuw component).
- **Color** parameters: toon een visuele kleur-swatch naast de token-dropdown.
- Parameter labels: gebruik Nederlandse namen. Mapping:

| Parameter naam | Nederlands label |
|---|---|
| Label | Labeltekst |
| Placeholder | Voorbeeldtekst |
| Value | Waarde |
| Text | Tekst |
| Title | Titel |
| Description | Omschrijving |
| Icon | Icoon |
| AriaLabel | Toegankelijkheidslabel |
| Disabled | Uitgeschakeld |
| Visible | Zichtbaar |
| Required | Verplicht |

### Implementatie
- Voeg een `bool _simpleMode = true` toe aan `PropertyPanel`.
- Definieer een `HashSet<string> SimpleParameterNames` per componenttype in een lookup dictionary.
- Filter `GroupedParameters` op `_simpleMode`.
- Voeg de visuele editors (ButtonStyle-picker, Intent-picker, IconPicker) toe als nieuwe Razor-componenten in de Designer RCL.

## Fase 4 — Preview-modus

Voeg een preview-modus toe die alle editor-chrome verbergt en alleen het ontwerp toont zoals een eindgebruiker het zou zien.

### Toggle
- Toolbar-knop: "Preview" (icoon: `visibility`) / "Bewerken" (icoon: `edit`).
- Keyboard shortcut: `Ctrl+P` (registreer in CommandRegistry).

### Preview weergave
- Verberg: palette, property panel, data panel, structure tree, code panel, page tabs, toolbar (behalve de preview-toggle en viewport-knoppen).
- Toon: de canvas met de actieve pagina, full-width, met het geselecteerde thema.
- Canvas nodes tonen GEEN selectie-bars, drag-handles, of dropzones.
- Als het document meerdere pagina's heeft: toon een eenvoudige navigatiebalk bovenaan met pagina-links.

### Implementatie
- Voeg `bool _previewMode` toe aan `DesignerShell`.
- In preview-modus: render `DesignRenderer` direct in plaats van `DesignerCanvasNode`.
- `DesignRenderer` bestaat al — het wordt gebruikt in de versiegeschiedenis-preview.
- Voeg het canvas-thema toe aan de preview-wrapper.

## Fase 5 — Visuele drag-feedback verbetering

### Component-preview bij slepen
- Wanneer een palette-item gesleept wordt: toon een semi-transparant preview van het component naast de cursor.
- Gebruik `e.dataTransfer.setDragImage()` met een dynamisch gegenereerd element dat het icoon en de naam bevat.

### Canvas glow-effect
- Wanneer een item gesleept wordt OVER de canvas: toon een subtiele glow/rand op de canvas die aangeeft "je kunt hier droppen".
- Dropzones pulseren zachtjes (opacity animatie) om aandacht te trekken.

### Drop-feedback
- Na een succesvolle drop: flash het nieuw toegevoegde component kort (300ms, zachte glow) om te bevestigen waar het terecht is gekomen.
- Scroll de canvas automatisch naar het nieuwe component als het buiten beeld is.

## Fase 6 — Snelle layout-hulp

### "Voeg rij toe" knop
- Onderaan de canvas (na alle bestaande nodes): een "Rij toevoegen" knop die een `RadzenRow` + `RadzenColumn Size=12` invoegt.
- Optioneel: een mini-dropdown bij de knop met kolomverdelingen:
  - 1 kolom (12)
  - 2 kolommen (6+6)
  - 3 kolommen (4+4+4)
  - Zijbalk + hoofdpaneel (4+8)
  - Hoofdpaneel + zijbalk (8+4)

### Kolom-splitsing
- Wanneer een `RadzenColumn` geselecteerd is: toon een "Splits kolom" knop in het property panel die de kolom in tweeën deelt (bv. Size 12 → twee kolommen van Size 6).

### Implementatie
- Voeg de "Rij toevoegen" sectie toe aan de canvas-markup in `DesignerShell.razor`, na de nodes-loop.
- De kolomverdelingen maken gebruik van de bestaande `AddNodeCommand` met vooraf geconfigureerde nodes.

## Fase 7 — Onboarding-tips

### Eerste-gebruik hints
- Bij het eerste bezoek (check localStorage vlag `agt-designer-onboarded`):
  - Toon een korte welkomst-overlay met 3 stappen:
    1. "Kies een template of begin leeg"
    2. "Sleep of klik componenten uit het palet naar de canvas"
    3. "Pas eigenschappen aan in het rechterpaneel"
  - Een "Begrepen" knop sluit de overlay en zet de vlag.

### Contextual tips
- Lege canvas: "Sleep een component hierheen of klik op een component in het palet links" (al deels aanwezig via `AgtEmptyState`).
- Lege slot: "Sleep hier een component" (al aanwezig via CSS `::before` content).
- Geen node geselecteerd: property panel toont pagina-eigenschappen met een hint "Klik op een component op de canvas om de eigenschappen te bewerken".

## Fase 8 — Export-duidelijkheid voor de developer-handoff

### Export-dialoog
- In plaats van direct een .zip downloaden: toon een export-dialoog met:
  - Samenvatting: aantal pagina's, aantal componenten, gebruikte entiteiten
  - Thema: welk thema is geselecteerd
  - Issues: eventuele waarschuwingen (met optie "Toch exporteren")
  - "Download projectpakket" knop
  - Uitleg voor developers: "Dit pakket bevat een Blazor-project met de ontworpen pagina's. Open het in Visual Studio of VS Code met `dotnet run`."

### Design-specificatie export (bonus)
- Optionele "Design spec" knop die een HTML-rapport genereert met:
  - Per pagina: screenshot (via canvas rendering), componentenlijst met properties, data-bindings
  - Bruikbaar als handoff-document voor developers die niet met het .zip-project werken

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- **User journey test** (handmatig, simuleer een niet-technische gebruiker):
  1. Open de designer → startscherm verschijnt met template-kaarten
  2. Klik "Invulformulier" → editor opent met een formulier-template
  3. Klik op "Tekstveld" in het palet → tekstveld verschijnt op de canvas
  4. Klik op het tekstveld op de canvas → property panel toont "Labeltekst", "Voorbeeldtekst", "Waarde"
  5. Wijzig "Labeltekst" naar "Naam klant" → label verandert live op de canvas
  6. Klik "Preview" → editor-chrome verdwijnt, alleen het formulier is zichtbaar
  7. Klik "Bewerken" → terug naar de editor
  8. Klik "Rij toevoegen" → kies "2 kolommen" → nieuwe rij verschijnt
  9. Klik "Exporteren" → export-dialoog toont samenvatting → download .zip
  10. De .zip bevat een werkend Blazor-project met de ontworpen pagina
- Rapporteer: de palette-namenvertaling (oud → nieuw), de simpele-parameters per componenttype, de startscherm-template-kaarten, en de preview-modus implementatie
