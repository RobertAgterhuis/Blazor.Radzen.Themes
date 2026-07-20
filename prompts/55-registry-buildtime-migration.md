# Prompt 55 — Correctie: designer-registry van runtime-reflectie naar build-time generatie

Prompt 48 is uitgevoerd vóór de WASM-beslissing (prompt 54): de registry leunt nu op runtime-reflectie (`Registry/ComponentParameterIntrospector.cs` — System.Reflection + XML-doc-bestanden at runtime inladen). Dat breekt onder WASM: trimming verwijdert ongerefereerde parameters/types stilletjes, en XML-doc-bestanden shippen niet mee in een publish. Migreer naar build-time generatie ZONDER de rest van fase 48 (model, renderer, tests) te verstoren. Draai deze prompt VÓÓR prompt 54.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infrastructuur hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Behoud het contract, vervang het mechanisme

De publieke registry-API (`DesignerComponentRegistry` en de metadata-typen) blijft ONGEWIJZIGD — alleen de vulling verandert. Alles wat nu tegen de registry praat (renderer, tests, straks palet/property-panel) mag geen wijziging nodig hebben; bewijs dat met een ongewijzigd-blijvende testset.

## 2. Build-time generator

- Verplaats de introspectie-logica (parameterdetectie, typemapping, XML-summary-extractie) naar een **build-time stap**: voorkeur een incremental source generator (`Agterhuis.Ui.Designer.Generators`, netstandard2.0, als analyzer gerefereerd); alternatief een MSBuild-taak/console-tool die vóór compile een statische `DesignerComponentRegistry.g.cs` emit — kies op basis van wat betrouwbaar werkt met de Radzen-assembly als input en motiveer.
- De bestaande `ComponentParameterIntrospector` wordt de KERN van de generator (hergebruik, niet herschrijven) en verdwijnt uit de runtime-assembly (of blijft intern voor de xunit-verificatietest — zie §3 — maar wordt nergens meer at runtime aangeroepen: grep-bewijs in het rapport).
- XML-summaries worden build-time in de gegenereerde metadata gebakken (strings in de code), zodat er geen doc-bestanden meer nodig zijn at runtime.
- De gegenereerde registry-klasse refereert alle component-types expliciet → dit is meteen de trimmer-root voor WASM (documenteer met een commentaarregel in de generated output).

## 3. Verificatie-tests aanpassen, niet afzwakken

- De bestaande registry-volledigheidstest blijft, maar draait nu als vergelijking: runtime-reflectie (in de TEST, waar trimming geen rol speelt) vs. de build-time gegenereerde registry — elk verschil (ontbrekende component, parameter, afwijkende default of summary) laat de test falen. Daarmee kan de generator nooit stilletjes achterlopen op de werkelijke API, ook niet na een Radzen-upgrade.
- Serialisatie-, validatie- en renderer-tests uit 48 blijven ongewijzigd groen.

## 4. Consistentie

`docs/designer/MODEL.md` bijwerken (generatie-mechanisme + trimmer-root-rol); check of nog andere designer-code runtime-reflectie doet die onder trimming sneuvelt (grep op System.Reflection in het Designer-project; de DesignRenderer's type-resolutie moet via de registry lopen, niet via `Type.GetType` op strings).

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen — inclusief de nieuwe generator-vs-reflectie-vergelijkingstest en alle bestaande 48-tests ongewijzigd. Grep-bewijs: geen runtime `System.Reflection`-gebruik meer in de Designer-runtime-paden. Rapporteer: het gekozen generator-mechanisme (en waarom), en bevestiging dat de registry-API ongewijzigd bleef.
