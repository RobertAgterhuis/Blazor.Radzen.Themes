# Prompt 61 — Uitgebreide validatie + Issues-panel

De huidige `DesignDocumentValidator` controleert basisfouten (onbekend componenttype, ontbrekende labels, onbekende parameters). Dat is onvoldoende: duplicate routes, broken bindings, ongeldige token-referenties, en layout-problemen worden niet gedetecteerd. Bovendien zijn validatieresultaten alleen beschikbaar via code — de designer-UI toont ze niet. Voeg uitgebreide validatielagen toe en een zichtbaar Issues-panel.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Validatielagen uitbreiden

Breid `DesignDocumentValidator` uit met de volgende controles. Elke controle krijgt een unieke `Code`, een `Severity` (Error, Warning, Info), en een `Path` die verwijst naar de exacte locatie in het document.

### Document-integriteit
- **DuplicateRoute**: twee of meer pagina's met dezelfde route → Error
- **DuplicateNodeId**: twee nodes met hetzelfde Id (kan ontstaan na handmatige JSON-edit) → Error
- **EmptyDocument**: document zonder pagina's → Error
- **EmptyPage**: pagina zonder nodes → Warning (niet per se fout, maar wijs erop)
- **InvalidRoute**: route die niet begint met `/` of ongeldige tekens bevat → Error

### Component-contract
- **RequiredSlotEmpty**: een component heeft een verplicht slot (bijv. ChildContent voor Card) dat geen kinderen bevat → Warning
- **IncompatibleNesting**: een component zit in een slot dat dat type niet verwacht (bijv. een DataGrid in een Button-slot) → Warning
- **DeprecatedComponent**: component dat in de registry als deprecated is gemarkeerd → Warning (voeg een `IsDeprecated`-vlag toe aan `DesignerComponentDescriptor` als die nog niet bestaat)

### Data-binding
- **BrokenEntityReference**: een component is gebonden aan een entiteit die niet in het `DesignDataModel` bestaat → Error
- **BrokenFieldReference**: een kolom of veld refereert naar een entiteitveld dat niet bestaat → Error
- **UnboundDataGrid**: een DataGrid-component zonder data-binding → Info (suggestie om te binden)

### Token-beleid
- **HardcodedColor**: een kleur-parameter bevat een hex-waarde in plaats van een token-referentie → Warning (de bestaande bleed-audit-logica hergebruiken)
- **InvalidTokenReference**: een token-referentie die niet bestaat in het actieve theme → Warning

### Toegankelijkheid (uitbreiding bestaand)
- **MissingFormLabel**: formulierveld zonder Label én zonder AriaLabel → Error (bestaat al, behoud)
- **EmptyButtonText**: een button zonder Text en zonder AriaLabel → Warning
- **ImageWithoutAlt**: een afbeeldingscomponent zonder alt-tekst → Warning

## Fase 2 — Issues-panel UI

### Layout
- Het Issues-panel is een inklapbare sectie ONDER de structuurboom (of als eigen tab in het linker paneel).
- Standaard ingeklapt als er geen issues zijn; automatisch open als er issues zijn.
- Header toont het aantal issues per severity: `3 fouten · 5 waarschuwingen · 2 info`.

### Lijst
- Elke issue toont: severity-icoon (❌ Error, ⚠️ Warning, ℹ️ Info — als Radzen-iconen, geen emoji), de code als korte titel, en de locatie (paginanaam + componentnaam).
- Klikken op een issue: selecteert de betreffende node op het canvas (wissel eerst van pagina als nodig) en scrollt het property panel naar de relevante parameter.
- Sorteerbaar op severity (errors eerst) en filterbaar (checkboxes per severity).

### Live updates
- Validatie draait na ELKE model-mutatie (via de command-stack notificatie). Gebruik debounce (200ms) om niet bij elke toetsaanslag te valideren.
- Issues verdwijnen direct zodra de gebruiker het probleem fixt.

### Auto-fix (waar mogelijk)
- Issues met een duidelijke fix tonen een "Fix"-knop:
  - **MissingFormLabel**: zet Label op de componentnaam → één command
  - **HardcodedColor**: bied een dropdown met matching tokens aan
  - **DuplicateRoute**: stel een unieke route voor
  - **EmptyButtonText**: zet Text op de componentnaam
- Auto-fix gaat via de command-stack (undo-baar).

## Fase 3 — Export-blokkade

- Export wordt GEBLOKKEERD als er Error-severity issues zijn. De export-knop is disabled en een tooltip toont "Los eerst alle fouten op".
- Warnings blokkeren NIET maar worden getoond in een bevestigingsdialoog vóór export: "Er zijn N waarschuwingen. Toch exporteren?"

## Fase 4 — Tests

- Per nieuwe validatieregel minimaal één test die de fout triggert en één die bewijst dat een correct document geen false positive geeft.
- Issues-panel bUnit: issues verschijnen bij een document met fouten, klik navigeert naar de node, auto-fix lost het issue op.
- Export-blokkade: export met errors → geblokkeerd; export met alleen warnings → bevestiging; export zonder issues → direct.

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- Handmatig: maak een document met een duplicate route → Issues-panel toont "DuplicateRoute" error → klik navigeert naar de pagina → fix de route → issue verdwijnt live → export is weer mogelijk
- Handmatig: voeg een DataGrid toe zonder binding → Issues-panel toont "UnboundDataGrid" info
- Handmatig: zet een hardcoded hex-kleur op een component → Issues-panel toont "HardcodedColor" warning met fix-knop → klik fix → kleur wordt token
- Rapporteer: het totaal aantal validatieregels, de auto-fix-dekking, en eventuele false-positive risico's
