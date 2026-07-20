# Prompt 44 — Capstone: van componentenbibliotheek naar volledig design system

Vijf sporen die het systeem compleet maken: gecodificeerde patronen, content-richtlijnen, een `dotnet new`-starter, formele visuele regressie, en token-export voor designers (Figma). Alles additief; alle bestaande guards (parity, bleed, contrast, a11y) blijven de poortwachters.

---

Copy below into Claude Code in the repo root:

---

## 1. Patronenbibliotheek (het verschil tussen bibliotheek en systeem)

Codificeer de UX-patronen die de showcase al demonstreert tot voorschriften mét levende referentie-implementaties:
- Nieuwe sectie in demo-nav "Patronen" + `docs/patterns/` met per patroon: wanneer gebruik je het, anatomie-schema (Mermaid of eenvoudige SVG), do's/don'ts, en een live referentiepagina in het DemoExample-sjabloon (code-tab!).
- Minimaal deze acht: **Formulierpagina** (labelplaatsing, secties, validatie-timing, AgtFormActions), **Lijst/CRUD-pagina** (grid + filterchips + saved views + drawer-detail), **Master-detail** (drawer vs. dialog vs. eigen pagina — beslisregels), **Wizard** (Steps, validatie per stap, terug-gedrag), **Dashboard** (metric cards, chart-dichtheid, calm rules), **Zoeken & filteren** (debounce, empty states, "wis filters"), **Foutafhandeling** (validatie vs. toast vs. error-state vs. errorpagina — beslisboom), **Bevestiging & destructieve acties** (wanneer confirm, wanneer undo-toast).
- Elke patroonpagina verwijst naar de showcase-plek waar het patroon in het echt draait.

## 2. Content-richtlijnen (schrijfgids)

`docs/CONTENT-GUIDELINES.md` + een demo-pagina "Schrijfwijzer" die de regels toont met goed/fout-voorbeelden:
- Toon (professioneel-direct, geen uitroeptekens in systeemteksten), NL als standaard met EN-equivalenten; werkwoord-eerst knoppen ("Opslaan", "Werkorder aanmaken" — nooit "OK"/"Klik hier"); foutmeldingen = wat gebeurde + wat te doen, geen jargon of foutcodes vooraan; empty states = uitnodiging, geen verontschuldiging; placeholders = echt voorbeeld, geen herhaling van het label; hoofdlettergebruik (sentence case), datum/getalnotatie (nl-NL), afkortingenbeleid.
- Audit de bestaande demo/showcase-teksten tegen de gids en corrigeer afwijkingen (rapporteer de wijzigingen).
- Voeg de kernregels toe aan CONTRIBUTING/copilot-instructions zodat agents ze afdwingen.

## 3. `dotnet new`-starter (adoptie-hefboom)

- Template-package `Agterhuis.Ui.Templates` (eigen csproj onder `templates/`): `dotnet new agterhuis-app -n MijnApp` levert een Blazor Web App die KANT-EN-KLAAR goed staat: package-referentie, `AddAgterhuisUi()`, correcte CSS/JS-volgorde + anti-FOUC-snippet, App.razor met theme-attribuut, een MainLayout met AgtSidebarLayout + switcher/dichtheid-toggle, één voorbeeldpagina per kernpatroon (formulier + lijst), NuGet.config-sjabloon, en de a11y-basics (skip-link, landmarks).
- Template-opties: `--theme <familie>` (default plum) en `--variant light|dark`.
- CI: template packt mee in de release-workflow; smoke-test in CI die het template instantieert en buildt (`dotnet new` + `dotnet build` van de output). Documenteer in README + docs/CONSUMING.md.

## 4. Formele visuele regressie (baselines)

- Bouw op de bestaande Playwright-infra (`eng/screenshots`): `eng/visual-regression/` met baseline-vergelijking — vaste pagina-set (Home, Buttons, DataGrid, Forms, één showcase-pagina, één blogpagina) × representatieve themes (plum-dark, hoth-light, imperial-dark, autotaalglas-light, volt-dark) × 2 viewports; pixel-diff met drempel (bijv. 0,1% afwijkende pixels), anti-flakiness (animaties uit via reduced-motion-emulatie, fonts geladen, deterministische seed-data — bestaat al).
- Baselines in de repo (`eng/visual-regression/baselines/`, PNG, geoptimaliseerd); npm-scripts `vr:test` en `vr:approve` (bewust bijwerken van baselines); heldere diff-output (voor/na/diff-drieluik in een artefactmap).
- Workflow-documentatie: draai vóór elke release en bij elke Radzen-upgrade; optionele CI-job (mag initieel handmatig blijven — documenteer de keuze).

## 5. Token-export voor designers (Figma)

- `eng/token-export/`: script dat de theme-CSS parseert (hergebruik de token-audit-parser!) en per themefamilie exporteert naar: (a) **W3C Design Tokens-formaat** (`design-tokens.<familie>.json`, het formaat dat Figma's Variables-import en Tokens Studio begrijpen) en (b) een vlak Style Dictionary-compatibel JSON. Kleuren, typografie(schaal), spacing, radius, schaduwen, motion-duraties — met de theme-scopes als modes (light/dark per familie).
- Round-trip-bewaking: de export draait als npm-script én als xunit-test die verifieert dat elke familie exporteert zonder ontbrekende tokens (zelfde bron als de pariteitstest — geen tweede waarheid).
- `docs/DESIGN-KIT.md`: hoe een designer de JSON in Figma importeert (Variables/Tokens Studio-stappen), de naamconventie-mapping (`--agt-color-primary-500` → `color/primary/500`), en de afspraak dat CODE de bron van waarheid is (designers consumeren de export; wijzigingen lopen via tokens-PR's).
- Publiceer de JSON's als release-artefact in de GitHub release-workflow.

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (incl. template-smoke, token-export-test). Handmatig: één patroonpagina + schrijfwijzer bekijken; `dotnet new agterhuis-app` lokaal instantiëren en draaien (thema correct, geen FOUC); `vr:test` draait groen tegen verse baselines; token-JSON van twee families opengeklapt controleren (modes, volledige set). Rapporteer per spoor wat er staat, en sluit af met een gap-oordeel: wat ontbreekt er nu nog aan een volledig design system (verwacht antwoord: niets wezenlijks — benoem eventuele bewuste restpunten).
