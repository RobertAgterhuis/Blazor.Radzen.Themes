# Prompt 48 — LowCode designer, fase 1: documentmodel + componentregistry + renderer

Eerste van zes prompts (48–53) die samen een LowCode WYSIWYG-designer bouwen: componenten op een grid slepen binnen de MainLayout-structuur en het resultaat exporteren als .NET-project. DEZE fase bouwt het fundament — géén UI: het documentmodel, de designer-metadata over componenten, en een renderer die het model live rendert. Model-first is de kernbeslissing: de editor bewerkt straks JSON, nooit Razor-tekst.

## Uitvoeringscontext (GitHub Copilot agent mode)

Voer uit met GitHub Copilot in agent mode (terminal-toegang), repo-root `D:\repositories\Blazor.Radzen.Themes`. Volg `.github/copilot-instructions.md` (bindend). Werk in fasen met na elke fase `dotnet build -c Release && dotnet test` groen en een eigen commit. Hergebruik bestaande infrastructuur — nooit dupliceren. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Nieuw project `src/Agterhuis.Ui.Designer` (RCL, net10.0)

Eigen assembly zodat de designer NIET in het `Agterhuis.Ui`-consumerpackage meelift; wel dezelfde bouwregels (CPM, warnings=errors, lock file). Demo-app refereert het.

## 2. Documentmodel (`Agterhuis.Ui.Designer.Model`)

- `DesignDocument` (naam, versie, pagina's) → `DesignPage` (route, titel) → boom van `DesignNode`s: `ComponentType` (string, bijv. "AgtTextField" of "RadzenAccordion"), `Parameters` (dictionary, JSON-serialiseerbare waarden + expressie-placeholder voor later), `Children` (named slots voor templated componenten: ChildContent, Columns, Items...), en `LayoutSlot` (rij/kolom/span binnen het grid — SLOT-gebaseerd, geen pixelposities).
- System.Text.Json-serialisatie met schema-versie + migratiehaakje; deterministische node-id's (voor selection/undo straks).
- Validatie: onbekend componenttype, onbekende parameter, verplicht-ontbrekend (Label/AriaLabel-guard!) → modelfouten met pad, geen exceptions.

## 3. Componentregistry (designer-metadata, gegenereerd — niet met de hand)

- `DesignerComponentRegistry`: per toelaatbaar component — displaynaam, categorie, icoon, welke slots, welke parameters (naam, type, default, beschrijving) en of het in het palet mag.
- Bron: HERGEBRUIK de bestaande reflectie uit het ComponentPage-parametertabel-werk (prompt 43) + `docs/RADZEN-COMPONENT-INVENTORY.md` voor de categorie-indeling. Genereren bij build of first-run; een xunit-test verifieert dat elk Agt-wrappercomponent in de registry zit en dat parametermetadata klopt met de werkelijke API (reflectie-vergelijking).
- V1-scope: alle Agt-wrappers + een gecureerde set rauwe Radzen-componenten (Row/Column/Stack/Card/Tabs/Accordion/DataGrid); de rest krijgt `AllowedInPalette=false` maar bestaat in de registry.

## 4. Model-renderer

- `DesignRenderer`-component: rendert een `DesignPage` live via `DynamicComponent`/RenderTreeBuilder — types resolven via de registry, parameters casten naar het echte parametertype (enum/EventCallback-parkering: events renderen als no-op in designtijd), slots recursief.
- Render-fouten per node afvangen en als foutkader tonen (nooit de hele pagina laten crashen — les uit de catalogus).
- Dogfood-test: bouw programmatteel een DesignDocument na van een bestaande demo-pagina (bijv. de Form Actions-demo) en assert dat de renderer dezelfde componentstructuur oplevert (bUnit: zelfde component-types in dezelfde volgorde).

## Verificatie

Build/test groen; nieuwe tests: serialisatie-roundtrip (document → json → document identiek), registry-volledigheid, renderer-dogfood, validatiefouten met pad. Nog géén UI — rapporteer de modelstructuur (kort schema in `docs/designer/MODEL.md`) en de registry-telling per categorie.
