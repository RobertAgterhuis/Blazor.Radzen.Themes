# Prompt 52 — LowCode designer, fase 5: demo-datamodel en databinding

Vereist prompts 48–51. Deze fase geeft ontwerpen ECHTE inhoud: een datamodel-ontwerper (entiteiten + velden + seed), binding van datacomponenten (grid, dropdown, lijsten, formulieren) aan die entiteiten, en export van een gegenereerde in-memory dataservice volgens het bestaande ShowcaseDataService-patroon. Demo-data komt uit een MODEL — Monaco is alleen de geavanceerde bewerkroute.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Datamodel in het designdocument

- `DesignEntity` (naam, meervoudsnaam) met `DesignField`s (naam, type: string/int/decimal/bool/DateTime/enum-met-waarden, verplicht, voorbeeldpatroon) en een seed-instelling (aantal rijen, vaste seed voor determinisme — huisregel).
- UI: nieuw "Data"-paneel in de designer (tab naast het palet): entiteiten aanmaken/bewerken via de eigen form-wrappers; seed-PREVIEW als tabel (eerste 5 rijen). Seed-generatie: deterministische, Nederlandstalige realistische waarden per veldtype/patroon (namen, plaatsen, bedragen, datums — hergebruik/extraheer de generatorlogica van ShowcaseDataService naar een deelbare helper).
- Monaco JSON-tab (uit 51) toont het datamodel mee; bewerken met schema-validatie blijft de power-route.

## 2. Binding in de designer

- Bindbare parameters (registry-metadata markeert ze: `Data`, `TValue`-koppelingen, Text/Value-properties): het property-panel toont naast vrije invoer een BRON-keuze — entiteit (voor collecties) of entiteit.veld (voor waarden/kolommen).
- DataGrid krijgt een kolommen-editor in het property-panel: kolommen afleiden uit de entiteitvelden (aanvinken, titel, formaat — numeriek rechts uitgelijnd met tabular figures conform de huisregels), sorteer/filter/paging-vlaggen.
- Formulier-sectie: "genereer formulier uit entiteit" — velden → passende Agt-wrappers (het patroon uit de patronenbibliotheek volgt), incl. validators uit verplicht/type.
- Designtijd-rendering: de DesignRenderer voedt gebonden componenten met de seed-data (geen echte services in de editor).

## 3. Export met dataservice

- CodeGen breidt uit: per entiteit een record + een gegenereerde `<Naam>DataService` (in-memory, deterministische seed, CRUD-methoden signature-compatibel met het ShowcaseDataService-patroon), DI-registratie in de template-`Program.cs`, en bindingen in de gegenereerde Razor (`@inject`, `Data="@..."`, kolomdefinities).
- Compile-roundtrip-test uitgebreid: geëxporteerd project met gebonden grid buildt én de bUnit-render toont seed-rijen.
- `docs/designer/DATA.md`: het datamodel, de bindingsemantiek, en hoe je de gegenereerde service later vervangt door een echte API-service (het contract is het aanknopingspunt).

## Verificatie

Build/test groen. Handmatig: entiteit "Klant" met 5 velden maken → grid op canvas binden → kolommen kiezen → seed-data live zichtbaar in de designer → formulier genereren uit de entiteit → export → uitgepakt project draait met gevulde grid en werkend formulier. Rapporteer: de bindbare-parameter-dekking (welke componenten v1 bindbaar zijn), de seed-generator-mogelijkheden, en het service-contract in de export.
