# Prompt 28 — Showcase-uitbreiding: vrijwel elk Radzen-component in een echte workflow

Goal: extend the Werkorders showcase so it exercises nearly every Radzen component — but ONLY in placements that make real-world sense. Coverage is measured, not guessed: a matrix against `docs/RADZEN-COMPONENT-INVENTORY.md` decides what's done. Requires prompt 27 (shell fix) to be landed.

---

Copy below into Claude Code in the repo root:

---

Extend the Werkorders showcase app (`/app/*`) with the modules below. House rules stay in force: tokens only, WCAG 2.2 AA, calm rules on data-dense pages, all guard tests green. Dutch UI text. Simulated latency + AgtLoadingPanel on data operations; AgtEmptyState wherever filters can produce zero results.

## 1. New/extended modules (component placements in parentheses)

- **Werkorder-detail** (`/app/werkorders/{id}`): tabbed detail (Tabs) — Gegevens (form wrappers + validators, FormField), Notities (HtmlEditor for werkbonnotities + SpeechToTextButton for dicteren), Foto's (Upload/Dropzone + Carousel + Image; FileInput for single bijlage), Historie (Timeline of status changes + Gravatar per monteur), Werkbon (QRCode with order-URL + Barcode ordernummer, print-knop). Breadcrumb on top; SplitButton for "Opslaan / Opslaan en sluiten / Opslaan en nieuwe"; Rating for klanttevredenheid after Afgerond; badge/ProgressBar for voortgang.
- **Projecten** (`/app/projecten`, NEW): multi-order projects — Gantt (taken, dependencies, kritiek pad), Steps wizard "Nieuw project" (3 stappen met form wrappers), PickList monteurs toewijzen, Accordion per projectfase, CardGroup projectkaarten.
- **Assets/locaties** (`/app/assets`, NEW): Tree of locatie → gebouw → installatie (checkboxes, contextmenu: "Werkorder aanmaken"), DropDownTree in the werkorder-form to pick an asset, Table for specificaties, Chip/ChipList for asset-tags, ColorPicker voor label-kleur.
- **Servicedesk** (`/app/servicedesk`, NEW): Chat (klantgesprek-simulatie met seeded berichten) and AIChat (assistent-demo met canned antwoorden), ListBox open tickets, SelectBar filter (Open/Bezig/Gesloten), Login-component as a styled "sessie verlopen"-panel demo.
- **Planning uitbreiden**: Scheduler keeps week view; add month/day toggles, TimeSpanPicker for duur, RadioButtonList tijdvak-keuze, DatePicker range filter.
- **Werkorders uitbreiden**: DataFilter advanced-zoeken panel above the grid, DropDownDataGrid klantkeuze in het formulier, Mask (postcode), Numeric (bedrag), AutoComplete (adres), MultiSelect dropdown (vaardigheden), grid: grouping, column picker, frozen kolom, dichtheid-toggle, export CSV/Excel, inline edit on one column, Pager standalone under the DataList mobile view.
- **Rapportage uitbreiden**: PivotDataGrid (omzet per monteur × type), Sparkline in metric cards, ArcGauge (bezettingsgraad), RadialGauge (SLA-score), LinearGauge (voorraad), SpiderChart (monteur-competenties), SankeyDiagram (orderstroom type → status), stacked charts; DataList met DataListFooter.
- **Instellingen uitbreiden**: Slider (standaard reistijd), SecurityCode (2FA-demo), Fieldset-groepen, CompareValidator (wachtwoord-herhaling demo), ToggleButton dichtheid, Splitter in een voorkeuren-layout.
- **Help** (`/app/help`, NEW): Markdown-component rendering a seeded handleiding, TOC ernaast, Popup voor tooltips-uitleg, Link-verzameling.
- **Shell**: ProfileMenu uitbreiden (avatar/Gravatar, menu-items), notification bell opens a themed dropdown with recent meldingen, Tooltip op icon-knoppen, ContextMenu op grid-rijen (Openen/Dupliceren/Annuleren), Fab "Nieuwe werkorder" on mobile widths, Dialog side-variant voor snelle orderpreview.

## 2. Coverage matrix (the definition of done)

Generate `docs/SHOWCASE-COVERAGE.md`: every standalone component from `docs/RADZEN-COMPONENT-INVENTORY.md` with columns [component | showcase-plek (route + context) | status]. Target: ≥90% of standalone components used in a meaningful workflow. Allowed exceptions (mark "bewust niet", with reason): GoogleMap (API key), SSRSViewer (server), and anything requiring external services — give those a stubbed/disabled placement note instead. NO component may be placed purely decoratively: each row's context must name the user task it serves.

## 3. Data & realism

Extend ShowcaseDataService (deterministic seed): projects with taken/dependencies, assets-boom (3 niveaus), tickets + chatberichten, competentie-scores, voorraad/SLA-cijfers, foto-placeholders (eigen gegenereerde/CC0 afbeeldingen — geen merkmateriaal). Mutations blijven in-memory en tonen toasts.

## 4. Guardrails

Calm rules: werkorders/planning/rapportage/projecten pages get no ambient motion; Gantt/Pivot/Scheduler stay performant with the seed volume (no jank at 60 orders/20 taken). A11y: every new interactive element labeled, dialogs trap focus, grids keyboard-operable, charts get aria-labels + color-blind-safe series. Bleed audit stays clean; parity untouched.

## 5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green — add smoke tests for every new route + 3 behavior tests (project aanmaken via wizard, chat bericht sturen, asset-contextmenu → werkorder aanmaken). Walk all showcase routes in plum-dark + hoth-light. Report: the coverage percentage from SHOWCASE-COVERAGE.md, components still uncovered with reasons, and any theme gaps found (fixed at token source).
