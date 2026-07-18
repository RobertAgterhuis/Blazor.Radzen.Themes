# Prompt 36 — Theme-state desync + hamburger werkt NOG STEEDS niet + restpunten

Observed on /components/forms/radio-list: the switcher displays "Plum Ink · Donkere variant" while the ENTIRE UI renders ocean-dark (teal nav active, teal radio, teal gradient). The `data-agt-theme` attribute on `<html>` and the `AgtThemeState` the switcher binds to are out of sync. ALSO: despite prompt 35, clicking the hamburger STILL does not show/hide the menu — this must be definitively fixed this time. Also: the catalog cross-link renders as a clumsy full-width banner above the page header. Fix root causes.

---

Copy below into Claude Code in the repo root:

---

## 1. Theme state ↔ DOM desync (kernbug)

Reproduce: persist a non-default theme (localStorage `agt-ui-theme` = ocean-dark), hard-refresh a component page. Expected: UI ocean AND switcher shows Ocean. Actual: UI ocean, switcher shows the default (Plum Ink).

Diagnose in this order and fix at the cause:
a) **Prerender vs circuit**: with Blazor Server interactivity there are TWO scoped `AgtThemeState` instances (prerender + circuit). The anti-FOUC script sets the DOM attribute from localStorage, but the circuit's state starts at `DefaultTheme` and nothing syncs it back before the switcher renders. Verify where the sync happens (MainLayout `OnAfterRenderAsync` reads `agtTheme.getStoredTheme` and calls `SetTheme`): does the SWITCHER re-render after that sync? (`SetTheme` only fires `ThemeChanged` when the value CHANGES — but also check the switcher actually re-renders: it binds `SelectedThemeFamily` in `OnParametersSet`, which only runs when the parent re-renders it.)
b) **Both shells**: the showcase layout must run the same sync; check it does (this page is the catalog shell, but fix both).
c) **Single source of truth**: consolidate — one place (the layout) reads persisted theme on first interactive render, updates state, and state drives BOTH the attribute (via interop) and the switcher display. The switcher must subscribe to `ThemeChanged` (with dispose) OR reliably re-render via the layout's `StateHasChanged`; prove which mechanism is used and test it.
d) Regression test: bUnit — render the switcher with a state, call `SetTheme("ocean-dark")` externally, assert the switcher's displayed value updates. Plus an E2E note in the test plan: persisted-theme refresh shows matching switcher + UI.

Also verify the toggle (light/dark) and Home hero reflect the synced theme after refresh — every consumer of ThemeState must agree.

## 2. Hamburger togglet de sidebar NIET — definitief oplossen

Prompt 35 asked for this; it still does not work: clicking the hamburger does nothing visible, the sidebar stays expanded. Treat this as a debugging task, not a styling task:

a) **Find out why the previous attempt failed.** Check in order: is the button's `Click` handler actually wired and firing (add a temporary log/breakpoint check)? Does it mutate the state that `RadzenSidebar.Expanded` is bound to (`@bind-Expanded` vs one-way `Expanded=` without change propagation — a classic)? Is the sidebar inside a `RadzenLayout` so Radzen's expand/collapse CSS applies at all? Is there custom CSS (fixed width on the sidebar or its container) that overrides the collapsed state so the toggle "works" in state but not visually? Is the button perhaps rendered by a component that lost its event wiring after the shell restructure (interactivity boundary — the layout must be interactive, not statically rendered)?
b) **Required behavior (acceptance criteria — test each one):**
   - Desktop ≥1024px: klik 1 → sidebar klapt in tot icon-rail (of geheel dicht, kies één en documenteer), klik 2 → weer uit. Zichtbaar, met soepele transition (reduced-motion → direct).
   - <1024px: sidebar is standaard dicht; hamburger opent hem als overlay-drawer met backdrop; sluit op buitenklik, Escape en navigatie.
   - Werkt in BEIDE shells (catalogus én showcase /app).
   - `aria-expanded` wisselt mee; keuze gepersisteerd (localStorage) en hersteld na refresh.
c) **Bewijs:** bUnit-test die de toggle klikt en asserteert dat de Expanded-state én `aria-expanded` wisselen; noteer in het rapport WAAROM het eerder niet werkte (de gevonden oorzaak uit a).

## 3. Cross-link plaatsing (catalogus ↔ Agt-pagina's)

The "Bekijk de rauwe Radzen-variant in het theme →" link renders as a full-width bordered bar ABOVE the page header — it reads as a debug banner. Restyle per the original intent: a small muted inline link at the END of the page-header block (under the description), text-secondary color, arrow icon, hover underline; same treatment for the reverse link on catalog pages ("Voor gebruik in applicaties: zie de Agt-wrappers →"). No full-width bordered containers.

## 4. Sidebar-subtitel

"De wrapper-API voor consumers" under AGT COMPONENTEN: verify it is styled as a muted 11px subtitle (non-focusable, not hoverable, excluded from keyboard navigation) and hidden in collapsed rail mode — polish if it still reads as a menu item.

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen incl. de nieuwe switcher-sync test en de hamburger-toggle test. Handmatig: (1) zet via de switcher Ocean → hard refresh → switcher toont Ocean én UI is Ocean; wissel naar Plum → refresh → beide Plum; zelfde check in de showcase-shell; toggle en Home-hero volgen mee. (2) Hamburger: klik op 1920px → sidebar klapt zichtbaar in/uit; op 800px → overlay-drawer met backdrop/Escape; in beide shells; state overleeft refresh. (3) Cross-links staan als bescheiden inline links onder de paginakop. Rapporteer expliciet: de gevonden oorzaak van de theme-desync én de gevonden oorzaak waarom de hamburger eerder niet werkte.
