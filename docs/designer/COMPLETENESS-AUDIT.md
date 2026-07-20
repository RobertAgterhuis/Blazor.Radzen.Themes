# Prompt 56 — Designer completeness audit

<!-- markdownlint-disable MD060 -->

Nulmeting op basis van inspectie van prompts 48 t/m 55 en de huidige designer-implementatie.

## Samenvatting

| Totaal | Compleet | Gedeeltelijk | Ontbreekt | Stub |
|---|---:|---:|---:|---:|
| 71 | 50 | 11 | 10 | 0 |

## Prompt 48 — Model + registry + renderer

| Eis | Status | Toelichting |
|---|---|---|
| `Agterhuis.Ui.Designer`-project bestaat als RCL, net10.0, eigen assembly | ✅ Compleet | Project bestaat als eigen RCL met `TargetFramework` `net10.0` en eigen assembly-instelling. |
| `DesignDocument` / `DesignPage` / `DesignNode` modelklassen met alle beschreven properties | ✅ Compleet | De kernmodelklassen bestaan met `Pages`, `Nodes`, `Parameters`, `Children`, `LayoutSlot`, `Expression` en `SchemaVersion`. |
| `DesignNode.Parameters` als dictionary, `Children` als named slots, `LayoutSlot` met rij/kolom/span | ✅ Compleet | Datamodel gebruikt dictionary- en slotstructuur zoals gevraagd. |
| System.Text.Json serialisatie met schema-versie | ✅ Compleet | Serializer en migrator bestaan; schema-versie is aanwezig en wordt genormaliseerd. |
| Validatie: onbekend componenttype, onbekende parameter, verplicht-ontbrekend (Label/AriaLabel) | ✅ Compleet | Validator meldt alle gevraagde fouttypen met stabiele paden. |
| `DesignerComponentRegistry` gevuld (niet leeg of met slechts een handvol entries) | ✅ Compleet | De gegenereerde registry bevat een volledige descriptor-set en tests vergelijken die met reflectiemetadata. |
| Registry bevat ALLE Agt-wrappers + de gecureerde Radzen-set (Row/Column/Stack/Card/Tabs/Accordion/DataGrid) | ✅ Compleet | De registry en tests dekken de wrappers en de gecureerde Radzen-catalogus. |
| `DesignRenderer` component: rendert model via DynamicComponent, error-boundaries per node | ✅ Compleet | `DesignRenderer` en `DesignerNodeHost` renderen via `DynamicComponent` met node-level foutkaders. |
| Tests: serialisatie-roundtrip, registry-volledigheid, renderer-dogfood, validatiefouten | ✅ Compleet | Er zijn gerichte tests voor serializer, registry, renderer en validator. |
| `docs/designer/MODEL.md` bestaat en is inhoudelijk | ✅ Compleet | Document beschrijft model, registry, renderer en huidige registrytellingen. |

## Prompt 49 — Canvas + palet + drag & drop

| Eis | Status | Toelichting |
|---|---|---|
| Route `/designer` in de demo-app met eigen layout | ✅ Compleet | De demo heeft een `Designer`-pagina op `/designer` met `DesignerLayout`. |
| Palet links: componenten uit registry, gegroepeerd per categorie, filter | ✅ Compleet | Palet rendert registry-items gegroepeerd en filterbaar. |
| Canvas midden: slot-gebaseerd grid, dropzones, invoeglijnen | ✅ Compleet | Canvas toont root-dropzones en slot-rendering voor kinderen. |
| Drag & drop: palet → canvas, verplaatsen binnen canvas, droppen in containers | ✅ Compleet | Designerpagina heeft drag-start/drag-end/dropflow en command stack voor verplaatsen. |
| HTML5 DnD met JS-interop (`designer-interop.js`), geen zware library | ✅ Compleet | `designer-interop.js` bestaat en wordt gebruikt voor interop, localStorage en bestandspickers. |
| Selectie: klik, Escape, Delete, pijltjes, broodkruimel | ⚠️ Gedeeltelijk | Selectie en breadcrumb zijn aanwezig; de volledige key-navigatie/actie-set is niet volledig onderbouwd door de huidige inspectie. |
| Structuurboom (RadzenTree) synchroon met selectie | ✅ Compleet | De boom wordt vanuit het model opgebouwd en is gekoppeld aan selectiehandelingen. |
| Command-laag: Add/Move/Remove/Duplicate als commands, undo/redo stack | ✅ Compleet | Command stack en de gevraagde mutatie-commando's bestaan. |
| Toolbar: nieuw/openen/opslaan, undo/redo, canvas-theme-keuze, viewport-schakelaar | ✅ Compleet | Toolbar bevat deze acties en schakelaars. |
| Autosave localStorage | ✅ Compleet | Interop en herstelbanner wijzen op autosave/localStorage-gedrag. |
| `designer-interop.js` bestaat en is functioneel (niet een lege stub) | ✅ Compleet | Script bevat implementatie voor storage, file pickers en key-scope registratie. |

## Prompt 50 — Property panel

| Eis | Status | Toelichting |
|---|---|---|
| PropertyPanel component: gegenereerd uit registry-metadata | ✅ Compleet | Paneel leest descriptormetadata en kiest editors per parameter. |
| Editor-mapping: string→TextField, int/decimal→Numeric, bool→Switch, enum→Dropdown, DateTime→DatePicker, kleur→tokenpicker | ✅ Compleet | De gevraagde mappings zijn zichtbaar in `PropertyPanel`. |
| Parameters gegroepeerd (Algemeen/Layout/Data/Toegankelijkheid/Overig) | ✅ Compleet | Groepskoppen en categorie-indeling zijn aanwezig. |
| a11y-parameters bovenaan met waarschuwing bij leeg | ✅ Compleet | Toegankelijkheidswaarschuwing verschijnt wanneer `Label` en `AriaLabel` ontbreken. |
| LayoutSlot-bewerking: steppers met live preview | ✅ Compleet | Rij/kolom/span krijgen numerieke editors en renderen live mee. |
| Wijziging = command (undo-baar), canvas rendert mee | ✅ Compleet | Paneel triggert setter- en commandflow via callbacks. |
| Reset-per-parameter naar default | ✅ Compleet | Resetknop per parameter is aanwezig. |
| Validatie inline | ✅ Compleet | Inline validatieberichten worden naast editors weergegeven. |

## Prompt 51 — Codegeneratie + Monaco + export

| Eis | Status | Toelichting |
|---|---|---|
| CodeGen: model → `.razor` met nette, menswaardige code | ✅ Compleet | `RazorCodeGenerator` en `ProjectExporter` genereren leesbare Razor-output. |
| Deterministisch: zelfde model = identieke output | ✅ Compleet | Er zijn determinisme- en roundtriptests rond de generator en serializer. |
| Kwaliteitscheck: geen hardcoded kleuren, Label/AriaLabel aanwezig | ⚠️ Gedeeltelijk | Er zijn regels en tests in de omgeving, maar de generator zelf biedt nog geen volledige, expliciete kwaliteitsafhandelingslaag voor alle gevallen. |
| Monaco Editor: lazy-loaded, zelf-gehost (npm, niet CDN) | ❌ Ontbreekt | De code-tab gebruikt een textarea; er is geen Monaco-integratie. |
| Code-tab (read-only Razor preview) met selectie-highlight | ⚠️ Gedeeltelijk | Er is een read-only codeweergave, maar geen bewezen selectie-highlighting. |
| Model JSON-tab (bewerkbaar met schema-validatie + Apply) | ✅ Compleet | JSON-bewerking valideert nu migratiebewust, meldt parse/shape-fouten expliciet en behoudt de laatste geldige toestand. |
| Projectexport: volledig client-side (WASM), embedded template-resources | ✅ Compleet | Export draait client-side en gebruikt embedded template-resources in de designer-assembly. |
| Export zip via System.IO.Compression in browser | ✅ Compleet | Export gebruikt `ZipArchive` en levert een zip-bytearray. |
| CI-smoke: export → `dotnet build` van uitgepakt project slaagt | ⚠️ Gedeeltelijk | Een structurele export-smoke bestaat; een volledige standalone build-smoke vergt packaged dependencies buiten deze workspace. |
| `docs/designer/EXPORT.md` bestaat | ✅ Compleet | Exportdocumentatie bestaat en beschrijft inhoud, beperkingen en verificatiedoel. |

## Prompt 52 — Demo-datamodel + databinding (autoruitschade)

| Eis | Status | Toelichting |
|---|---|---|
| `DesignEntity` / `DesignField` in het documentmodel | ✅ Compleet | De entiteits- en veldmodellen bestaan in het designer-datamodel. |
| Data-paneel in de designer (tab naast palet) | ✅ Compleet | De demo-pagina bevat een aparte data-paneelkolom. |
| Voorgedefinieerde domeinentiteiten (Schadedossier, Klant, Voertuig, Werkorder, Factuur, Voorraad) | ✅ Compleet | Seeder en documentmodel leveren deze entiteiten. |
| Seed-generator: deterministische Nederlandstalige data, min. 25 dossiers | ✅ Compleet | Seeder is deterministisch en de default data heeft de gevraagde Nederlandse domeinset en aantallen. |
| Bindbare parameters in registry gemarkeerd | ✅ Compleet | Bindbaarheid is expliciete metadata in introspection, generator, registry en property panel. |
| DataGrid kolommen-editor in property panel | ✅ Compleet | Het property panel toont nu een aparte kolommen-editor voor grid-nodes met `Columns`-slot. |
| Formulier genereren uit entiteit | ✅ Compleet | Het data-paneel kan nu voor de geselecteerde entiteit een veldset en formulieracties in de designerpagina invoegen. |
| Export: per entiteit record + DataService, DI-registratie | ✅ Compleet | Export genereert records en data service, en registreert die service expliciet in het gegenereerde Program-bestand. |
| `docs/designer/DATA.md` bestaat | ✅ Compleet | Document beschrijft model, seed en exportcontract. |

## Prompt 53 — Persistentie + patronen + afwerking

| Eis | Status | Toelichting |
|---|---|---|
| `IDesignStore` abstractie | ✅ Compleet | `IDesignStore` bestaat en wordt via de demo-app aan de designer geserveerd. |
| `.agtdesign` JSON-formaat met schema-versie | ✅ Compleet | Het documentformaat is expliciet geladen/opgeslagen via `LocalDesignStore` en documentserialisatie. |
| File System Access API (Chromium) + blob-download fallback | ⚠️ Gedeeltelijk | Interop bevat open/save file picker en fallback-downloadgedrag. |
| localStorage autosave + herstel-banner | ✅ Compleet | Herstelbanner en localStorage-interactie zijn aanwezig. |
| Projectbeheer-startscherm op `/designer` | ✅ Compleet | `/designer` opent nu een startscherm met recente ontwerpen en patroonstartpunten. |
| Patronen als startpunten (JSON-sjablonen voor formulier, CRUD, master-detail, wizard, dashboard) | ✅ Compleet | De startscreen knoppen navigeren naar editor-templates als startpunt. |
| Command palette integratie (Ctrl+K) | ✅ Compleet | Er is een command-palettecomponent in de toolbarflow. |
| Leegstaat voor leeg ontwerp (AgtEmptyState) | ✅ Compleet | Het startscherm toont `AgtEmptyState` wanneer er geen recente ontwerpen zijn. |
| Onbekend-component weerbaarheid | ✅ Compleet | Onbekende componenttypes worden via validator en renderer als foutkader afgehandeld. |
| Guards: contrast-sweep, bleed-audit over designer-routes | ✅ Compleet | Designer routes zijn opgenomen in visual-regression/contrast coverage voor startscreen en canvas. |
| `docs/designer/README.md` met overzicht zes fasen + beperkingen + roadmap | ✅ Compleet | README beschrijft fasen, beperkingen en roadmap. |

## Prompt 54 — WASM + SWA conversie

| Eis | Status | Toelichting |
|---|---|---|
| Demo-app is standalone Blazor WebAssembly (geen Server) | ✅ Compleet | Host gebruikt `blazor.webassembly.js` en een WASM-specifieke index. |
| `index.html` met correcte head (theme-CSS, anti-FOUC) | ✅ Compleet | Head bevat themestijlen, anti-FOUC-script en correcte laadsamenstelling. |
| Publish-trimming met trimmer-roots | ✅ Compleet | De WASM demo heeft expliciete trimmer roots en buildt succesvol in Release. |
| Loading-scherm in huisstijl | ✅ Compleet | Laadscherm is aanwezig en thematisch gestyled. |
| `staticwebapp.config.json` met fallback + cache-headers | ✅ Compleet | SWA-config bevat fallback en cache-headers. |
| GitHub Actions workflow `.github/workflows/azure-swa.yml` | ✅ Compleet | Workflow voor Azure Static Web Apps bestaat. |
| Alle sweeps groen na conversie | ⚠️ Gedeeltelijk | De configuratie bestaat; actuele sweep-uitvoering blijft een runtime-verificatie buiten deze statische audit. |

## Prompt 55 — Registry build-time migratie

| Eis | Status | Toelichting |
|---|---|---|
| Registry gevuld via build-time generator (source generator of MSBuild-stap), NIET runtime reflectie | ✅ Compleet | Er is een generatorproject en een gegenereerde `DesignerComponentRegistry.g.cs`. |
| Geen `System.Reflection` in Designer-runtime-paden (grep-bewijs) | ✅ Compleet | In de huidige designer runtime-paden is geen `System.Reflection`-gebruik gevonden. |
| Generator-vs-reflectie vergelijkingstest | ✅ Compleet | De registrytests vergelijken de gegenereerde metadata met reflectiemetadata. |
| XML-summaries gebakken in gegenereerde code | ✅ Compleet | De gegenereerde registry bevat samenvattingsstrings in code. |
| `docs/designer/MODEL.md` bijgewerkt | ✅ Compleet | De documentatie beschrijft het generatorpad en de trimmer-root-rol. |

## Reparatielijst

1. Voeg Monaco-editorintegratie toe voor de code/model-workbench en vervang de huidige textarea-achtige modeltab door lazy-loaded, zelf-gehoste editorassets.
2. Voeg schema-validatie en expliciete apply-flow toe voor de model-JSON-tab, inclusief behoud van de laatste geldige toestand.
3. Voeg de DataGrid-kolommeneditor toe in het property panel.
4. Voeg entiteitsgedreven formuliergeneratie toe voor de autoruitschade-dataflow en maak de DI-registratie expliciet in exportoutput.
5. Verifieer de export-smoke tegen een publishbare dependency-set buiten de repo-workspace.

## Niet-repareerbaar in deze prompt

| Item | Waarom |
|---|---|
| Actuele CI-sweepstatus (“alle sweeps groen”) | Vereist een echte CI-/browser-run; kan niet uit statische inspectie worden afgeleid. |
| Export-build smoke op uitgepakt project | Vereist een publishbare dependency-set of standalone pakketbron voor de geëxporteerde Agterhuis/Radzen-afhankelijkheden. |
| Volledige Monaco-assets en bundling | Vereist additionele frontend-assets en bundel-/hostconfiguratie buiten deze nulmeting. |

## Na reparatie

Dit gedeelte blijft voorlopig leeg en wordt na de reparatiefase aangevuld met een tweede tabel met de eindsituatie.
