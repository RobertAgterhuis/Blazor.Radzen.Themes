# Prompt 8 — Readability fixes (screenshot audit, 5 root causes + crashes)

Findings from a visual audit of the running demo (13 screenshots, both modes). These are mostly five systemic root causes — fix the cause once in the theme, then verify every listed symptom.

---

Copy below into Claude Code in the repo root:

---

Fix the following readability and stability issues in the Agterhuis.Ui theme and demo. Fix root causes in theme tokens/partials — not per-page patches. Work top-down.

## ROOT CAUSE 1 — White text on gold (invisible labels)

Symptom list: pager active page (gold circle, number invisible), menu top-level active item (gold block, no readable text), SelectBar active segment, Gantt view-toggle active button ("Week" = empty gold block), secondary/gold buttons ("SECONDARY" white-on-gold), dropdown selected item (bright gold bar, text invisible), TimePicker OK-button.

Fix: introduce ONE token `--agt-on-accent: #241a00` (dark mode) / `#241a00` (light mode) and map every Radzen "on secondary/accent" foreground to it: `--rz-on-secondary`, selected-item text, pager active text, menu active text, toggle/selectbar active text. Rule: any surface filled with accent gold ALWAYS uses `--agt-on-accent` for text and icons. Grep all theme partials for gold fills and verify each has a dark foreground. Where Radzen hardcodes white on selection, override the specific class (`.rz-state-highlight`, active menu/pager/selectbar classes).

## ROOT CAUSE 2 — Portaled overlays render light-mode in dark theme

Symptom list: DatePicker calendar popup is white with pale-lavender selected day (number invisible), dropdown panel is light gray in dark mode, time-picker hour/minute dropdowns white, chips/SelectBar chips light gray on dark pages.

Likely cause: the `data-agt-theme` attribute is set on an app-level div, while Radzen renders popups/dialogs in a portal at `<body>` level — outside the themed scope. Fix: apply the theme attribute on `<html>` (or `<body>`) via the ThemeToggle JS/interop so ALL portaled content inherits it. Then complete the dark styles for popup surfaces: calendar panel, day cells (hover/selected = purple fill + white text, today = gold ring), month/year dropdowns, time column dropdowns, dropdown/listbox panels, autocomplete panel, context menus, tooltips. Selected calendar day must be readable: purple-600 fill + white text, NOT pale lavender + white.

## ROOT CAUSE 3 — Headings and muted text unreadable on dark

Symptom list: component page titles ("AgtSidebarLayout", "AgtNumericField", "AgtDatePicker", "AgtDropdown") render dark purple on the plum canvas — near invisible; description lines under titles are too dim.

Fix: heading color token for dark mode = `#f2edf7` (near-white), keep the gold underline accent. The AgtPageHeader/demo page title styles must use the text tokens, not primary-500. Bump dark muted text to ≥ `#a293b8` and verify 4.5:1 against `#17101f`. Audit ALL text tokens in dark scope against surface-0..3.

## ROOT CAUSE 4 — Unthemed/blank component states

- Buttons catalog: "LIGHT" variant = white text on white fill (define light-variant fg), danger button renders with no visible label (fix label + contrast), SplitButton stretches full width (demo layout: give it natural width in a Stack).
- Scheduler (light): renders as an empty white void — give it a fixed height + sample appointments this month so the month grid actually shows; style day headers/cells both modes.
- Gantt: bars are default gray — theme them (purple fill, gold critical path, readable labels).
- Chat/AIChat demo: empty headers, near-invisible placeholders and borders; theme chat surface (surface-1), message bubbles (own = purple-600/white, other = surface-3), placeholder ≥ 4.5:1, mic/send icons visible in both modes; remove the duplicated second chat instance if unintentional.
- AgtSidebarLayout demo: the embedded sidebar renders as a detached dark panel floating outside the demo card — constrain the demo inside a bounded container (`position: relative`, fixed height, overflow hidden) so the inner RadzenSidebar stays inside its frame.
- DataList cards (Data page): double nested borders with excessive empty padding — single border, tighter padding.

## ROOT CAUSE 5 — Runtime crashes

`/catalog/data-advanced` and `/catalog/charts-advanced` throw "Er is een onverwachte fout opgetreden". Reproduce (dotnet run, check console/logs), fix the exceptions (likely PivotDataGrid/chart parameter or data-shape issues), and add a bUnit smoke test that renders every catalog page so a crashing page fails the test suite from now on.

## Verification

1. `dotnet build -c Release` zero warnings; `dotnet test` green (including the new render-all-pages smoke tests).
2. Walk every page in BOTH modes and confirm each symptom above is gone; specifically screenshot-check: pager numbers, menu active item, SelectBar, Gantt toggles, dropdown selected item, calendar popup dark, buttons catalog, scheduler grid visible, chat readable, both crashed pages loading.
3. Update docs/A11Y-CONTRAST.md with the changed pairs (on-accent on gold, dark headings, muted text).
4. Final report: per root cause, what was changed and which files.
