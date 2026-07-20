# Prompt 53 — LowCode designer, fase 6: persistentie, patronen-integratie en afwerking

Vereist prompts 48–52. Slotfase: projecten fatsoenlijk bewaren/beheren, de patronenbibliotheek als startpunten, designer-UX-afwerking, en de guards/documentatie die de designer productiewaardig maken.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Project-persistentie (volledig client-side — de host is WASM/SWA)

- Alle persistentie loopt via een `IDesignStore`-abstractie met in v1 één implementatie (lokaal). Dit is bewust: een latere centrale opslag (SWA managed Azure Functions + Blob/Cosmos, voor delen/synchroniseren tussen apparaten) wordt dan een tweede implementatie zonder designer-wijzigingen — zet dit als roadmap-kandidaat in docs/designer/README.md.
- Designdocumenten opslaan/openen als bestand: `.agtdesign`-JSON (met schema-versie + migratie vanaf de 48-versie). Openen via upload (InputFile) werkt overal; opslaan via de File System Access API waar beschikbaar (Chromium: echt "Opslaan/Opslaan als" met bestandshandle, zodat herhaald opslaan naar hetzelfde bestand gaat) met blob-download als fallback voor overige browsers — feature-detect, beide paden testen.
- localStorage-autosave wordt "herstel niet-opgeslagen werk" met een herstel-banner na crash/refresh; wees expliciet over de grens (apparaat- en browser-gebonden, kan door de gebruiker gewist worden — de banner en docs zeggen dat het bestand de echte opslag is).
- Projectbeheer-startscherm op `/designer`: recente ontwerpen (localStorage-index + bestandshandles waar de browser dat toestaat), nieuw leeg ontwerp, nieuw-vanuit-sjabloon (zie §2), importeren. Verwijderen met AgtConfirmDialog.
- Het geëxporteerde project bevat het document al (`design/document.json` uit 51) — "openen vanuit export-zip" hoeft niet; documenteer dat de JSON het uitwisselformaat is.

## 2. Patronen als startpunten

- De patronenbibliotheek (prompt 44 spoor 1) wordt bruikbaar IN de designer: per patroon (formulierpagina, lijst/CRUD, master-detail, wizard, dashboard) een kant-en-klaar DesignDocument-sjabloon — kies bij "nieuw" een patroon en krijg een correct opgebouwde pagina met gebonden voorbeeld-entiteit, klaar om aan te passen.
- Sjablonen zijn data (JSON in de Designer-assembly), geen code; een test valideert elk sjabloon tegen het model + de a11y-guards en rendert het (geen kapotte startpunten — de les van de lege voorbeelden).

## 3. UX-afwerking

- Command palette-integratie (Ctrl+K): designer-acties registreren (component invoegen op selectie, pagina toevoegen, exporteren, undo).
- Canvas: uitlijn-hints tijdens slepen (kolomgrenzen oplichten), leegstaat voor een leeg ontwerp (AgtEmptyState met "sleep een component of kies een patroon"), zoom niet nodig — viewport-schakelaar volstaat (bevestig of bouw alsnog indien praktijk anders uitwijst; rapporteer de keuze).
- Foutweerbaarheid: een document met inmiddels onbekende componenten (bijv. na een package-update) opent met die nodes als duidelijk gemarkeerde "onbekend component"-kaders i.p.v. crash; migratietest dekt dit.
- Dichtheid/thema van de EDITOR-chrome volgen de gewone toggles; canvas-theme blijft onafhankelijk (uit 49).

## 4. Guards & documentatie

- Alle bestaande sweeps over de designer-routes: contrast-sweep, bleed-audit, example-scan-principe (designer-startscherm en panelen renderen inhoud), visuele regressie: designer-startscherm + canvas-met-inhoud toevoegen aan de VR-paginaset.
- bUnit: save/open-roundtrip, sjabloon-instantiatie, onbekend-component-weerbaarheid, herstel-banner.
- `docs/designer/README.md`: overzicht van de zes fasen, architectuur (model-first), beperkingenlijst v1 (geen Razor-import, geen event-logica, single-select) en de roadmap-kandidaten (two-way Monaco, multi-select, logica-haakjes).
- README + wiki: designer-sectie met 3 screenshots (via het bestaande screenshotscript, blank-guard geldt).

## Verificatie

Build/test groen; alle sweeps nul violaties; VR-baselines bijgewerkt via `vr:approve` (bewust). E2E-doorloop handmatig: nieuw ontwerp vanuit het CRUD-patroon → entiteit aanpassen → component toevoegen via Ctrl+K → opslaan als bestand → refresh → herstel-banner NIET (was opgeslagen) → openen → exporteren → uitgepakt project draait. Rapporteer per sectie, plus een eindoordeel: wat is de v1-designer wél en niet (de beperkingenlijst), en welke roadmap-kandidaat het meeste waarde heeft.
