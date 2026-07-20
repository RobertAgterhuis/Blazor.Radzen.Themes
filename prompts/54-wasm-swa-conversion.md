# Prompt 54 — Conversie naar standalone Blazor WebAssembly + Azure Static Web Apps

De demo-app draait op Server-interactiviteit; dat blokkeert Static Web Apps-hosting én is de verkeerde basis voor de komende WYSIWYG-designer (drag & drop over SignalR is stroperig; circuit-verlies = sessieverlies). Converteer naar ÉÉN standalone WASM-host (geen dual-host — dat verdubbelt de testmatrix) en richt SWA-deployment in. Draai deze prompt VÓÓR de designer-reeks (48–53).

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infrastructuur hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Conversie naar standalone WASM

- Zet `samples/Agterhuis.Ui.Demo` om naar een standalone Blazor WebAssembly-app (of vervang door een nieuw WASM-project dat alle Components/pagina's overneemt — kies de route met de kleinste diff en motiveer). RCL's (`Agterhuis.Ui`, straks `Agterhuis.Ui.Designer`) blijven ongewijzigd.
- Verwijder server-afhankelijkheden: geen `AddInteractiveServerComponents`, geen HttpContext-gebruik (grep!), `Program.cs` naar WebAssemblyHostBuilder, DI-registraties (AgtThemeState, ShowcaseDataService, opties) ongewijzigd overzetten.
- `index.html` vervangt `App.razor`-host: zelfde head-inhoud (theme-CSS-volgorde, fonts, theme-interop, anti-FOUC-snippet — die werkt in WASM juist beter: geen prerender-flits). Géén prerendering in v1 (bewuste keuze: designer en demo zijn geen SEO-doelen; de blog-showcase accepteert de trade-off — documenteer dit).
- Controleer de bekende WASM-verschillen: culture (nl-NL: `InvariantGlobalization=false` + icu; datum/getalnotatie in scheduler/grids blijft Nederlands), `Task.Delay`-gesimuleerde latency blijft werken, localStorage/JS-interop identiek.
- Laadtijd beheersen: publish-trimming aan met trimmer-roots voor Radzen + DynamicComponent-gebruik (test grondig — trimming is de klassieke WASM-breker), Brotli-compressie (SWA doet dit), lazy-load Monaco blijft, loading-scherm in huisstijl (token-based, met het brandmerk) i.p.v. de standaard Blazor-spinner.

## Fase 2 — Tests en guards mee

- bUnit-tests blijven (renderer-onafhankelijk); pas de Playwright-basis (`eng/*`) aan op het nieuwe startcommando; ALLE sweeps (contrast, bleed, example-scan, VR) moeten weer groen — VR-baselines opnieuw via `vr:approve` na visuele verificatie dat er niets veranderde behalve de host.
- Smoke: elke route rendert; theme-switch incl. crossfade en persistentie; showcase-CRUD; blog-journey.

## Fase 3 — Azure Static Web Apps

- `staticwebapp.config.json`: navigation fallback naar `index.html` (exclusief bestands-extensies), correcte mime/cache-headers voor `.wasm`/`.woff2`, 404 naar de eigen NotFound-route.
- GitHub Actions: `.github/workflows/azure-swa.yml` — build + `dotnet publish` van de WASM-app, upload `wwwroot`-output met de officiële SWA-action; deploy-token als secret `AZURE_STATIC_WEB_APPS_API_TOKEN` (documenteer in docs/RELEASING.md hoe ik de SWA-resource aanmaak en het token zet — dat doe ik zelf in Azure, de workflow slaat de stap over zolang het secret ontbreekt); PR-previews aanzetten (standaardgedrag van de action) — mooi koppelvlak met de visuele regressie.
- README: sectie "Live demo" met de SWA-URL-placeholder + badge.

## Fase 4 — Documentatie-consistentie

Werk `.github/copilot-instructions.md`, docs/CONSUMING.md en de designer-prompts-aannames bij: het hostmodel is nu WASM (consumers mogen uiteraard nog steeds Server gebruiken — de RCL is hostmodel-agnostisch; benoem dat expliciet), en de designer-reeks (48–53) bouwt op de WASM-host (registry build-time, export client-side — die aanpassingen staan al in 48/51).

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen; alle sweeps groen; `dotnet publish` + lokaal serveren van de output (bijv. `npx serve` of het SWA CLI-emulatorpad) — volledige walk: demo, catalogus, showcase, blog in twee families, theme-persistentie na refresh, nl-NL-datums, geen console-errors. Rapporteer: de gekozen conversieroute, publish-grootte (voor/na trimming), trimmer-issues die je tegenkwam, en de handmatige Azure-stappen die ik nog moet doen.
