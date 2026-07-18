# Prompt 34 — Enterprise-grade polish (additief, geen downgrade)

Goal: raise the perceived quality to top-tier enterprise product level (think Linear / Azure Portal / Datadog class) with additions pro users FEEL: density, numeric typography, keyboard, and workflow patterns. Hard constraint: ZERO downgrade — existing themes, tokens, guard tests, WCAG and performance rules all stay intact; everything below is additive and token-driven.

---

Copy below into Claude Code in the repo root:

---

Implement the following eight enterprise-polish tracks in Agterhuis.Ui + demo/showcase. All styling token-driven (parity + bleed guards stay green), AA maintained, reduced-motion respected.

## 1. Density system (the most enterprise feature there is)

- Token-driven density: `data-agt-density="comfortable|compact"` alongside the theme attribute. Compact remaps the structural spacing/control-height/font-size tokens (~-20%: controls 2rem, grid rows tighter, card padding reduced) — ONE remap layer, no per-component forks.
- `AgtDensityToggle` in the RCL; persisted in localStorage; showcase topbar gets it; grids/forms/nav respond instantly. Parity-style test: compact defines exactly the structural token set it overrides.

## 2. Numeric & data typography

- Tabular figures everywhere data lives: `font-variant-numeric: tabular-nums` token applied to grid numeric cells, metric cards, pagers, charts axes.
- Numeric grid columns right-aligned with consistent decimal formatting (nl-NL) via a small `AgtNumeric`/format helpers.
- Delta indicators: `AgtDelta` component (▲/▼/— + value, semantic color + sign so color is never alone) used in metric cards and rapportage.
- All-caps labels (kickers, grid headers) get a letter-spacing token; long text truncation ALWAYS pairs with a title/tooltip.

## 3. Command palette (Ctrl/Cmd+K)

- `AgtCommandPalette` in the RCL: themed glass modal, fuzzy search over registered commands (navigatie-routes + acties via a simple `IAgtCommandRegistry`), keyboard-first (arrows/enter/esc), recent items, section grouping. Demo + showcase register their routes and 3–4 actions ("Nieuwe werkorder", "Wissel thema...", "Ga naar planning").
- Fully accessible (dialog semantics, focus trap, aria-activedescendant listbox pattern).

## 4. Master-detail drawer pattern

- `AgtDrawer` (side panel, right, 480px default, resizable optional): the enterprise alternative to center dialogs for detail/edit. Glass surface per theme, focus trap, Escape, unsaved-changes guard hook.
- Showcase: werkorder-rij klik opent de drawer (detail/edit) in plaats van center-dialog; center dialogs blijven voor confirmaties.

## 5. Grid workflow layer (waar enterprise leeft)

- Active-filter chips row above AgtDataGrid: applied filters as removable chips + "Wis alles"; werkt met DataFilter/kolomfilters.
- Saved views: dropdown (Weergaven) that stores grid settings (bestaat al via save-settings) under a named view in localStorage; default view per user.
- Skeleton rows: loading state renders layout-matching skeleton rows in the grid (theme shimmer tokens) instead of a spinner overlay; same pattern for metric cards and lists (extend AgtLoadingPanel with `Variant="Skeleton"` presets: grid-rows, cards, form).
- Sticky grid summary-footer token styling (totals row) + sticky first column shadow cue when scrolled.

## 6. Keyboard visibility layer

- Shortcut hints in tooltips ("Nieuwe werkorder — N") and a "?"-overlay (`AgtShortcutOverlay`) listing shortcuts per page; registry-driven, themed, accessible.
- Navigation progress hairline: 2px accent line at the very top during page navigation/long operations (token-driven, reduced-motion → static).

## 7. Designed edge-states (beyond empty)

- Extend AgtEmptyState into a family: `NoResults` (filters actief → "Wis filters"-actie), `Error` (met retry), `NoPermission`, `Offline` — each with the per-theme generative motif, consistent copywriting (NL), demo page showing all.
- Consistent status-pill language: één `AgtStatusDot` (dot + label, semantic + icon) gebruikt in grid, timeline, notificaties, scheduler — vervang ad-hoc badges in de showcase.

## 8. Print & export finish

- Print stylesheet (`@media print`): werkbon-detailpagina print als nette zwart-wit bon (QR + barcode intact, geen nav/sidebar/ambient, brand-regel in de footer); test via de browser-printpreview.
- Grid-export knoppen krijgen een klein export-menu (CSV/Excel) met toast-bevestiging — consistent op alle grids in de showcase.

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (parity/bleed/a11y + nieuwe tests: density-tokenset, command palette open/navigate, drawer focus trap, delta rendering). Contrast-sweep opnieuw draaien (nieuwe oppervlakken: drawer, palette, chips, skeletons — nul violaties). Walk de showcase in compact + comfortable, twee families; Ctrl+K werkt overal; werkbon-print preview netjes. Rapporteer per track wat is toegevoegd en bevestig expliciet dat geen bestaand theme-gedrag is gewijzigd (diff-samenvatting van aangepaste bestanden).
