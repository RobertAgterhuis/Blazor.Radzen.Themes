# Prompt 56 — Designer completeness audit: analyseer, rapporteer en repareer

De designer-reeks (prompts 48–55) beschrijft een uitgebreid systeem. De ervaring leert dat bij prompt-voor-prompt-uitvoering onderdelen worden overgeslagen, half geïmplementeerd, of als stub achterblijven. Deze prompt doet GEEN nieuwbouw — hij inventariseert wat er WEL en NIET staat en repareert alles wat ontbreekt of incompleet is.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Werkwijze

Werk in DRIE fasen: eerst analyseren (geen codewijzigingen), dan rapporteren, dan repareren. Sla geen fase over.

## Fase 1 — Analyse (ALLEEN lezen, nul wijzigingen)

Loop elk prompt-bestand (48 t/m 55) door en controleer per eis of de implementatie BESTAAT, COMPLEET is, en WERKT. Gebruik de onderstaande checklist als leidraad — maar lees ook de prompttekst zelf voor eisen die hier niet expliciet staan.

### Prompt 48 — Model + registry + renderer
- [ ] `Agterhuis.Ui.Designer`-project bestaat als RCL, net10.0, eigen assembly
- [ ] `DesignDocument` / `DesignPage` / `DesignNode` modelklassen met alle beschreven properties
- [ ] `DesignNode.Parameters` als dictionary, `Children` als named slots, `LayoutSlot` met rij/kolom/span
- [ ] System.Text.Json serialisatie met schema-versie
- [ ] Validatie: onbekend componenttype, onbekende parameter, verplicht-ontbrekend (Label/AriaLabel)
- [ ] `DesignerComponentRegistry` gevuld (niet leeg of met slechts een handvol entries)
- [ ] Registry bevat ALLE Agt-wrappers + de gecureerde Radzen-set (Row/Column/Stack/Card/Tabs/Accordion/DataGrid)
- [ ] `DesignRenderer` component: rendert model via DynamicComponent, error-boundaries per node
- [ ] Tests: serialisatie-roundtrip, registry-volledigheid, renderer-dogfood, validatiefouten
- [ ] `docs/designer/MODEL.md` bestaat en is inhoudelijk

### Prompt 49 — Canvas + palet + drag & drop
- [ ] Route `/designer` in de demo-app met eigen layout
- [ ] Palet links: componenten uit registry, gegroepeerd per categorie, filter
- [ ] Canvas midden: slot-gebaseerd grid, dropzones, invoeglijnen
- [ ] Drag & drop: palet → canvas, verplaatsen binnen canvas, droppen in containers
- [ ] HTML5 DnD met JS-interop (`designer-interop.js`), geen zware library
- [ ] Selectie: klik, Escape, Delete, pijltjes, broodkruimel
- [ ] Structuurboom (RadzenTree) synchroon met selectie
- [ ] Command-laag: Add/Move/Remove/Duplicate als commands, undo/redo stack
- [ ] Toolbar: nieuw/openen/opslaan, undo/redo, canvas-theme-keuze, viewport-schakelaar
- [ ] Autosave localStorage
- [ ] `designer-interop.js` bestaat en is functioneel (niet een lege stub)

### Prompt 50 — Property panel
- [ ] PropertyPanel component: gegenereerd uit registry-metadata
- [ ] Editor-mapping: string→TextField, int/decimal→Numeric, bool→Switch, enum→Dropdown, DateTime→DatePicker, kleur→tokenpicker
- [ ] Parameters gegroepeerd (Algemeen/Layout/Data/Toegankelijkheid/Overig)
- [ ] a11y-parameters bovenaan met waarschuwing bij leeg
- [ ] LayoutSlot-bewerking: steppers met live preview
- [ ] Wijziging = command (undo-baar), canvas rendert mee
- [ ] Reset-per-parameter naar default
- [ ] Validatie inline

### Prompt 51 — Codegeneratie + Monaco + export
- [ ] CodeGen: model → `.razor` met nette, menswaardige code
- [ ] Deterministisch: zelfde model = identieke output
- [ ] Kwaliteitscheck: geen hardcoded kleuren, Label/AriaLabel aanwezig
- [ ] Monaco Editor: lazy-loaded, zelf-gehost (npm, niet CDN)
- [ ] Code-tab (read-only Razor preview) met selectie-highlight
- [ ] Model JSON-tab (bewerkbaar met schema-validatie + Apply)
- [ ] Projectexport: volledig client-side (WASM), embedded template-resources
- [ ] Export zip via System.IO.Compression in browser
- [ ] CI-smoke: export → `dotnet build` van uitgepakt project slaagt
- [ ] `docs/designer/EXPORT.md` bestaat

### Prompt 52 — Demo-datamodel + databinding (autoruitschade)
- [ ] `DesignEntity` / `DesignField` in het documentmodel
- [ ] Data-paneel in de designer (tab naast palet)
- [ ] Voorgedefinieerde domeinentiteiten (Schadedossier, Klant, Voertuig, Werkorder, Factuur, Voorraad)
- [ ] Seed-generator: deterministische Nederlandstalige data, min. 25 dossiers
- [ ] Bindbare parameters in registry gemarkeerd
- [ ] DataGrid kolommen-editor in property panel
- [ ] Formulier genereren uit entiteit
- [ ] Export: per entiteit record + DataService, DI-registratie
- [ ] `docs/designer/DATA.md` bestaat

### Prompt 53 — Persistentie + patronen + afwerking
- [ ] `IDesignStore` abstractie
- [ ] `.agtdesign` JSON-formaat met schema-versie
- [ ] File System Access API (Chromium) + blob-download fallback
- [ ] localStorage autosave + herstel-banner
- [ ] Projectbeheer-startscherm op `/designer`
- [ ] Patronen als startpunten (JSON-sjablonen voor formulier, CRUD, master-detail, wizard, dashboard)
- [ ] Command palette integratie (Ctrl+K)
- [ ] Leegstaat voor leeg ontwerp (AgtEmptyState)
- [ ] Onbekend-component weerbaarheid
- [ ] Guards: contrast-sweep, bleed-audit over designer-routes
- [ ] `docs/designer/README.md` met overzicht zes fasen + beperkingen + roadmap

### Prompt 54 — WASM + SWA conversie
- [ ] Demo-app is standalone Blazor WebAssembly (geen Server)
- [ ] `index.html` met correcte head (theme-CSS, anti-FOUC)
- [ ] Publish-trimming met trimmer-roots
- [ ] Loading-scherm in huisstijl
- [ ] `staticwebapp.config.json` met fallback + cache-headers
- [ ] GitHub Actions workflow `.github/workflows/azure-swa.yml`
- [ ] Alle sweeps groen na conversie

### Prompt 55 — Registry build-time migratie
- [ ] Registry gevuld via build-time generator (source generator of MSBuild-stap), NIET runtime reflectie
- [ ] Geen `System.Reflection` in Designer-runtime-paden (grep-bewijs)
- [ ] Generator-vs-reflectie vergelijkingstest
- [ ] XML-summaries gebakken in gegenereerde code
- [ ] `docs/designer/MODEL.md` bijgewerkt

## Fase 2 — Rapport

Genereer een bestand `docs/designer/COMPLETENESS-AUDIT.md` met:

1. **Per prompt** een tabel: eis | status (✅ Compleet / ⚠️ Gedeeltelijk / ❌ Ontbreekt / 🔇 Stub) | toelichting
2. **Samenvatting**: totaal eisen, compleet, gedeeltelijk, ontbrekend, stub
3. **Reparatielijst**: genummerde lijst van alles wat gerepareerd moet worden, op volgorde van afhankelijkheid (fundament eerst)
4. **Niet-repareerbaar in deze prompt**: zaken die een architectuurwijziging of externe dependency vereisen

Commit dit rapport VOORDAT je begint met repareren (zodat het rapport de nulmeting vastlegt, niet de eindsituatie).

## Fase 3 — Reparatie

Werk de reparatielijst uit het rapport punt voor punt af:

### Regels
- **Fix, niet herschrijf**: repareer bestaande code, voeg ontbrekende stukken toe. Vervang niets dat al werkt.
- **Stubs → echte implementatie**: als er een klasse/methode bestaat maar de body is `throw new NotImplementedException()`, leeg, of een TODO-comment — implementeer de body conform de promptspecificatie.
- **Ontbrekende bestanden aanmaken**: als een heel bestand/component ontbreekt, maak het aan conform het prompt.
- **Tests meefixen**: als een test ontbreekt of faalt door de reparatie, fix de test. Geen tests verwijderen of `[Skip]`'en.
- **Per reparatie een eigen commit** met duidelijke message (`fix(designer): implement missing X from prompt NN`).
- **Na elke reparatie**: `dotnet build -c Release && dotnet test` moet groen zijn. Als een reparatie de build breekt, fix dat in dezelfde commit.

### Prioriteit
1. Modellaag (48): als het model incompleet is werkt niets
2. Registry (48/55): als de registry leeg is zijn palet + property panel leeg
3. Renderer (48): als de renderer niet werkt is het canvas leeg
4. Canvas + DnD (49): zonder canvas geen designer
5. Property panel (50): zonder panel geen bewerking
6. CodeGen + Monaco + export (51): zonder export geen output
7. Data-binding + autoruitschade-entiteiten (52): demo-data
8. Persistentie + patronen + afwerking (53): polish
9. WASM/SWA (54): hosting
10. Build-time registry (55): trimming-safety

## Verificatie

Na alle reparaties:
- `dotnet build -c Release` zero warnings
- `dotnet test` groen — inclusief alle tests uit de reparaties
- Alle sweeps (contrast, bleed, example-scan) nul violaties
- Handmatige doorloop: `/designer` opent → palet toont componenten → slepen naar canvas → property panel toont eigenschappen → seed-data zichtbaar → export downloadt een zip → uitgepakt project buildt
- Het `COMPLETENESS-AUDIT.md` rapport wordt bijgewerkt met de eindsituatie (een tweede tabel "na reparatie")
- Rapporteer: wat is gerepareerd (met commit-hashes), wat blijft open (en waarom), en de totale tijdsinvestering per reparatiepunt
