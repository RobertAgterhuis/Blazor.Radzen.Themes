# Prompt 25 — Realistische showcase-app ("Werkorders") in de demo

Goal: a realistic business application inside the demo site — like realestate.radzen.com is for Radzen — built with OUR theme and components, so users see what a real app looks like. Not another component page: a coherent app with its own layout, navigation, seeded data and workflows.

---

Copy below into Claude Code in the repo root:

---

Add a showcase application to `samples/Agterhuis.Ui.Demo` under the route prefix `/app`, presented as a fictitious field-service tool called **"Werkorders"** (all UI text Dutch). It must feel like a production app, not a demo page.

## 1. Own shell

- A dedicated layout for `/app/*` (NOT the catalog MainLayout): AgtSidebarLayout-based shell with its own compact sidebar (Dashboard, Werkorders, Planning, Klanten, Rapportage, Instellingen — icons per item), topbar with app name, a fake user profile menu (RadzenProfileMenu), notification bell, and the AgtThemeSwitcher + toggle so the whole showcase re-themes live.
- Entry from the demo: a "Voorbeeldapplicatie" item at the top level of the demo sidebar (in "Getting started", icon `rocket_launch`-style) that navigates to `/app`; inside the showcase, a subtle "← Terug naar componentbibliotheek" link in the profile menu or footer.
- The showcase follows the ACTIVE theme (all families) and is included in calm-route rules where data-dense.

## 2. Seeded domain data (in-memory, no backend)

A `ShowcaseDataService` (scoped) with realistic Dutch seed data: ~60 werkorders (nummer, klant, adres, type [Installatie/Reparatie/Onderhoud/Inspectie], status [Gepland/Onderweg/In uitvoering/Afgerond/Geannuleerd], prioriteit, monteur, datum/tijdvak, bedrag), ~15 klanten (bedrijf, contactpersoon, e-mail, telefoon, plaats), 6 monteurs, and 6 months of omzet/volume history for charts. Deterministic seed (fixed random) so screenshots are reproducible.

## 3. Pages (each a real workflow, mixing Agt wrappers + raw Radzen)

- **Dashboard** (`/app`): 4 metric cards (open orders, vandaag gepland, afgerond deze week, omzet MTD — count-up per signature rules), omzetlijn + orders-per-type donut (theme series palette), "vandaag" lijst, recente notificaties.
- **Werkorders** (`/app/werkorders`): full AgtDataGrid — paging, sorting, filtering, status badges (AgtBadge/intent colors + icon), row click → detail dialog; "Nieuwe werkorder" (gold/accent CTA) opens a dialog form using the Agt form wrappers with validators; delete via AgtConfirmDialog; mutations show AgtNotification toasts and update the grid.
- **Planning** (`/app/planning`): RadzenScheduler week view with the seeded orders as appointments, colored by status; drag/drop reschedule updates the data + toast.
- **Klanten** (`/app/klanten`): searchable list (DataList or grid) → klantdetail with tabbed layout (gegevens-formulier, werkorderhistorie-grid).
- **Rapportage** (`/app/rapportage`): 2–3 charts (staaf per monteur, lijn omzet, stacked per type) + export-knop (werkend CSV via JS download of het bestaande grid-export patroon).
- **Instellingen** (`/app/instellingen`): form-heavy page (AgtSwitch/Checkbox/Dropdown/TextField) that persists to the in-memory service — demonstrates a settings pattern.

## 4. Quality bar

- Everything token-themed (bleed audit stays clean); WCAG rules apply (labels, focus, contrast, no color-only status — badges carry icons/text).
- Realistic polish: empty states (AgtEmptyState when filters match nothing), loading panels on simulated latency (300ms), validation errors demonstrated, breadcrumbs where logical.
- bUnit smoke tests: every showcase page renders; one behavior test (nieuwe werkorder toevoegen verschijnt in grid).
- Keep routes/pages of the existing demo untouched.

## 5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (incl. new smoke tests; parity/bleed unaffected). Walk the full showcase in plum-dark, hoth-light and imperial-dark: shell, dashboard, grid-CRUD flow (aanmaken → toast → grid update → verwijderen met confirm), scheduler drag, charts, settings. Report pages built, components exercised (Agt vs raw Radzen), and any theme gaps discovered while building (fix them at the token source).
