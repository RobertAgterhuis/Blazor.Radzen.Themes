# Prompt 7 — "Plum Ink" redesign + 100% Radzen component coverage

Two goals in one pass: (1) replace the all-purple look with the calmer, higher-contrast "Plum Ink" design language; (2) close the component gap — EVERY component in the installed Radzen.Blazor package gets a themed catalog demo. No hand-picked subsets this time: the installed assembly is the source of truth.

---

Copy below into Claude Code in the repo root:

---

Redesign the Agterhuis.Ui theme to the "Plum Ink" design language AND bring the catalog to 100% coverage of the installed Radzen.Blazor package.

# PART A — 100% component coverage (do this first)

## A1. Enumerate — assembly is the source of truth

Programmatically list every public component in the installed package. Write a small script or throwaway console step that reflects over the Radzen.Blazor assembly (`~/.nuget/packages/radzen.blazor/<version>/lib/net*/Radzen.Blazor.dll`) and outputs every public non-abstract type deriving from `Microsoft.AspNetCore.Components.ComponentBase` (skip internal helpers like column/item/child types that cannot stand alone — list those as "child of X" instead). Save the result to `docs/RADZEN-COMPONENT-INVENTORY.md` with: component name, category, standalone/child, demo page (link), theme status.

## A2. Gap analysis

Diff the inventory against the existing demo/catalog pages. Everything without a demo goes on the worklist. Expected gaps include (verify against the inventory — this list is indicative, not authoritative): Gantt, PivotDataGrid, Chat, AIChat, Carousel, Chip, ChipList, Dropzone, TOC, FormField, TemplateForm + all validators (Required/Length/NumericRange/Compare/Email/Regex/DataAnnotation/Custom), DropDownTree, DropDownDataGrid, ListBox, TimeSpanPicker, ColorPicker, Rating, SecurityCode, SpeechToTextButton, Fab, FabMenu, SplitButton, SelectBar, Login, ProfileMenu, Breadcrumb, PanelMenu, ContextMenu, Popup, Splitter, CardGroup, PickList, DataFilter, DataList, Table, Tree, Scheduler, all chart types (Area/Bar/Column/Donut/Line/Pie/Scatter/Bubble/Stacked/Sparkline/Spider/Sankey), gauges (Arc/Radial/Linear), Timeline, QRCode, Barcode, HtmlEditor, Markdown, Image, Gravatar, Icon, Badge, Steps, Tabs, Accordion, Upload, FileInput.

## A3. Build the missing demos

One catalog page per family, every standalone component demonstrated with realistic sample data (Dutch labels where user-facing), showing default/disabled/validation states where applicable. Components requiring external services (GoogleMap key, SSRSViewer server, speech APIs) get a demo page with a static configured example and a note — never omit them from the catalog or inventory. Child components (e.g. RadzenDataGridColumn) are demonstrated inside their parent's demo and marked "child of X" in the inventory.

## A4. Definition of done for Part A

`docs/RADZEN-COMPONENT-INVENTORY.md` has zero rows without a demo link. The catalog index page groups all families and links every page.

# PART B — "Plum Ink" design language

Philosophy: the current theme drowns in mid-purple. Purple is the IDENTITY, not the wallpaper. Neutrals carry 80% of the interface, purple carries interaction and brand, gold is a scarce reward color (≤5% of pixels: active indicators, primary CTA, focus, brand mark). Calm surfaces for data-dense screens.

## B1. Token rework (agt-tokens.css — keep names, change values; add new ones)

Dark mode ("Plum Ink"):
- Canvas `--agt-surface-0: #17101f` (plum-black, NOT purple); `--agt-surface-1: #1a1224`; `--agt-surface-2: #1e1528`; `--agt-surface-3: #2c1f3c` (hover/active surfaces).
- Plum neutrals for text: primary text `#f2edf7`, secondary `#cfc3dd`, muted `#8f7ba6`, borders `#32243f` (hairline) / `#4a3760` (strong).
- Purple: interactive only — primary actions `#7a1fb0` (brighter than brand base for dark contrast), hover `#8a2bc4`, links/lavender accents `#c9a3e8`.
- Gold `#f1ce05`: active nav edge (3px), selected row edge, primary "new/create" CTA fill (dark text `#241a00`), focus rings, brand mark. Muted gold for lines: `rgb(241 206 5 / 35%)`.

Light mode ("Paper & Ink"):
- Canvas `#fbfafd`, surfaces white, borders `#e9e3f1`/`#cdbedd`, headings aubergine `#3d2557`, body `#2e2438`, muted `#6f6386`.
- Interaction: `#680898`, hover `#560a7f`; nav active = `#f3ebfa` bg + gold left edge + `#33204a` text.
- Gold in light mode: edges and fills only, gold-as-text uses `#9e8715`+.

Remove/repurpose Royal Nebula tokens that conflict: hero gradients become near-invisible (canvas with ≤4% radial tint) or are deleted; the glow tokens are reserved for focus/brand moments only.

## B2. Glass discipline

Glass (backdrop blur) ONLY on floating layers: header bar, dialogs, dropdown/picker popups, notifications. Cards, panels, grids, sidebar become SOLID surfaces with hairline borders. Keep `@supports` fallbacks and `prefers-reduced-transparency`.

## B3. Re-skin pass

With the new tokens, walk all theme partials: sidebar (solid surface-1, plum-neutral item text, gold active edge, lavender icons), datagrid (surface headers with muted-gold 2px bottom border, hover surface-3, selected = surface-3 + gold edge), buttons (primary purple fill; "create"-class CTA gold fill dark text; secondary hairline outline), inputs (surface-1 bg, hairline border, gold focus ring dark / purple focus ring light), tabs/steps/accordion (gold indicator), charts (series: #7a1fb0, #f1ce05, #c9a3e8, #560a7f, #9e8715, semantic info; gridlines = border hairline color), notifications/alerts (solid tinted surfaces, semantic edge).

## B4. Guardrails

- WCAG 2.2 AA stays mandatory (contrast matrix in docs/A11Y-CONTRAST.md updated; all pairs listed with ratios).
- Gold budget: grep the theme for accent token usage and justify each — if gold appears as decoration (borders everywhere, large fills, body text), remove it.
- No hard-coded hex outside agt-tokens.css; dark/light scoping audit (no dark values in `:root`).
- `prefers-reduced-motion` intact.

## B5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green; update bUnit tests broken by markup changes. Walk EVERY catalog page (now 100% coverage) in both modes; fix flat/unreadable/off-brand results. Final report: component inventory totals (n components, n demos), pages checked, contrast pairs, and where the gold budget went.
