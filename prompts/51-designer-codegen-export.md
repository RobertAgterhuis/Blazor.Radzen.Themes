# Prompt 51 — LowCode designer, fase 4: codegeneratie, Monaco-preview en projectexport

Vereist prompts 48–50. Deze fase maakt het waar: model → nette Razor-code, live te bekijken in een Monaco-paneel, en export als compleet .NET-project op basis van de `dotnet new`-starter (prompt 44) zodat het in een andere solution kan worden opgenomen. Eénrichtings-export (geen Razor-import) — bewuste v1-scope, documenteer dat.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken (starter-template, DemoExample-highlighting, token-audit als kwaliteitscheck op de output). Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Codegenerator (`Agterhuis.Ui.Designer.CodeGen`)

- Model → `.razor` per DesignPage: nette, MENSWAARDIGE code (correcte inspringing, parameters in logische volgorde, geen redundante defaults — alleen afwijkingen van de registry-default uitschrijven), `@page`-route, `PageTitle`, de grid-structuur als RadzenRow/Column-markup, slots recursief.
- Deterministisch: zelfde model ⇒ byte-identieke output (testbaar); generator-versie in een commentaarkop.
- Kwaliteit van de output is een harde eis: de gegenereerde code moet door de EIGEN huisregels komen — geen hardcoded kleuren (token-audit-patronen als post-check op de output), Label/AriaLabel aanwezig waar de guard het eist (het model valideert dit al — export blokkeert met een duidelijke foutenlijst zolang het model invalide is).
- Roundtrip-bewijs: genereer → compileer (in-memory of via een tijdelijk testproject in CI) → bUnit-render van de gegenereerde pagina levert dezelfde componentstructuur als de DesignRenderer (het dogfood-patroon uit 48, nu andersom).

## 2. Monaco-integratie (code-paneel)

- Monaco Editor zelf-gehost in de demo (npm-dependency, geen CDN — huisregel), lazy-loaded alleen op de designer-route; thema volgt licht/donker van de actieve familie (Monaco-theme gekoppeld aan de theme-switch).
- Tab "Code" naast het canvas: live de gegenereerde Razor van de actieve pagina (read-only in v1 — two-way sync is bewust uitgesteld; toon een "read-only preview"-badge), per-node highlight: selectie op canvas scrollt/markeert het bijbehorende codeblok (generator schrijft node-id's als markers die vóór weergave gestript worden).
- Tweede tab "Model (JSON)": het documentmodel in Monaco, WÉL bewerkbaar met schema-validatie (fouten inline); Apply = command (undo-baar). Dit is de power-user-route en het fundament voor de demo-data-fase.

## 3. Projectexport

- "Exporteren"-dialoog: projectnaam, themafamilie + standaardvariant, welke pagina's. **Export draait volledig client-side** (de host is WASM, prompt 54 — er is geen server om `dotnet new` uit te voeren): de bestanden van het starter-template (`Agterhuis.Ui.Templates` uit prompt 44) worden als embedded resources in de Designer-assembly meegebakken (build-stap die de template-inhoud embed — één bron, geen kopie die kan verlopen: een test vergelijkt de embedded set met de echte template-map), de naam/theme-substituties gebeuren in-browser, de gegenereerde pagina's + nav-items worden geïnjecteerd, het designdocument-JSON gaat mee als `design/document.json` (her-importeerbaar — het MODEL is round-trip, de code niet), en het geheel wordt in-browser gezipt (System.IO.Compression in WASM) en als download geleverd.
- CI-smoke: export van een voorbeelddocument (de exportlogica is host-onafhankelijk aanroepbaar vanuit een test) → `dotnet build` van het uitgepakte project slaagt (zelfde patroon als de template-smoke uit 44).
- `docs/designer/EXPORT.md`: wat de export bevat, hoe je hem in een bestaande solution opneemt, en de eenrichtings-beperking.

## Verificatie

Build/test groen incl. determinisme-test, compile-roundtrip en export-CI-smoke. Handmatig: pagina ontwerpen → Code-tab toont nette Razor die live meebeweegt → selectie-highlight werkt → JSON-tab bewerken + Apply → export downloaden, uitpakken, `dotnet run`: de app toont de ontworpen pagina in het gekozen theme zonder FOUC. Rapporteer: generator-beslissingen (defaults-weglating, marker-mechaniek), Monaco-bundelgrootte/lazy-load-bewijs, en de exportstructuur.
