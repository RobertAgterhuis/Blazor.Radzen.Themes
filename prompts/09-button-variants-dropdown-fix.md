# Prompt 9 — Button variant foregrounds + dropdown trigger repair

Observed: on the Buttons catalog page (dark mode) the Light variant is white-on-white and the Danger variant shows no readable label at all. The dropdown trigger renders as a malformed purple blob: clipped pill shape, detached chevron in its own rounded blob, broken height/padding.

---

Copy below into Claude Code in the repo root:

---

Two targeted fixes in the Agterhuis.Ui theme. Root-cause fixes in the theme partials, no per-page patches.

## 1. Complete the ButtonStyle foreground matrix

Radzen buttons have 8 styles (Primary, Secondary, Base, Light, Dark, Info, Success, Warning, Danger) × 4 variants (Filled, Flat, Outlined, Text). Our theme only defines foregrounds for some, so several fall back to white text regardless of fill.

Fix: define an explicit on-color per style in agt-tokens.css and map ALL of them in the buttons partial for BOTH modes:

- Primary (purple fill) → white.
- Secondary (gold fill) → `--agt-on-accent` (#241a00). Also hover/active/focus states.
- Light (light fill) → gray-900 dark text, in BOTH modes (a light fill stays light in dark mode, so its text stays dark).
- Base/Dark → explicit fg per mode, never inherit.
- Danger/Success/Warning/Info → check contrast of white on our semantic fills; where white fails AA (e.g. #e74c3c is borderline, warning likely fails), darken the fill variant (danger ≈ #c62828-class token) so white passes, or use dark text. The Danger button currently renders with NO visible label — verify the label is present in markup and its color differs from the fill.
- Outlined/Flat/Text variants: fg = the style's fill color (readable on canvas), verify per mode.

Add all pairs to docs/A11Y-CONTRAST.md. The Buttons catalog page must show every style × variant combination so gaps are visible from now on.

## 2. Rebuild the dropdown trigger styling

The `.rz-dropdown` trigger is visually broken: clipped rounded shape, chevron floating in a separate dark blob, inconsistent height. In the inputs/pickers partial:

- Trigger container: `min-height: 2.5rem`, full available width, `display:flex; align-items:center`, single consistent `border-radius: var(--agt-border-radius-md)`, 1px border (hairline token), surface-1 background, padding 0 0.75rem; remove any pseudo-element or `.rz-dropdown-trigger`-icon background that creates the second blob — the chevron sits inline at the right, same background as the trigger, muted foreground.
- No `overflow` clipping of the rounded corners; label text vertically centered with ellipsis overflow.
- States: hover = border strong; focus = gold ring (dark) / purple ring (light); disabled = reduced opacity surface with readable text; open state = chevron rotated, border accent.
- Apply the same treatment to every trigger-style control so they're consistent: DropDown, DropDownDataGrid, DropDownTree, MultiSelect dropdown, AutoComplete, DatePicker/TimeSpanPicker inputs, Numeric (spinner buttons inside the field, not detached).
- Verify in both modes on the Forms, Pickers, and Selection Inputs catalog pages.

## Verification

`dotnet build -c Release` zero warnings; `dotnet test` green. Screenshot-walk the Buttons page (all style × variant combos readable) and every dropdown/picker trigger in both modes. Report the contrast ratios added and which classes caused the blob.
