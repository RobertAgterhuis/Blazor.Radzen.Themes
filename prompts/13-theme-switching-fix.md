# Prompt 13 — Theme switching: volledige doorwerking + switcher-bugs

Observed with Dagobah selected: only SOME elements turn green (active nav items, theme dropdown) while the header bar, sidebar, all text inputs, and most components stay plum-purple. Also: the demo page does not re-theme until something else forces it, and the theme picker dropdown stays open after selecting a theme. This is not "add more colors to dagobah" — it is a scoping/parity defect in the theme system itself. Fix it structurally so EVERY current and future theme switches completely.

---

Copy below into Claude Code in the repo root:

---

Fix the multi-theme system in Agterhuis.Ui so a theme switch re-themes 100% of the UI, and fix the switcher UX. Root causes first.

## 1. Diagnose why most components stay plum

Check in this order and report what you find:

a) **Color tokens defined outside theme scopes.** Grep all theme/partial CSS: every COLOR-bearing `--agt-*` and `--rz-*` value must be defined ONLY inside `[data-agt-theme="..."]` scopes. `:root` may contain structural tokens only (spacing, radius, fonts, motion, z-index). Any color found in `:root` or on bare selectors is the bug — move it into every theme scope.

b) **Legacy alias selectors that always win.** The backwards-compat aliases (`[data-agt-theme="dark"]`, attribute-absent fallback) must not apply when a named theme is active. Implement aliasing explicitly: `[data-agt-theme="dark"]` and `[data-agt-theme="plum-dark"]` share one selector list; the default (no attribute) maps to plum-light via `:root:not([data-agt-theme])`-style scoping — never via unconditional `:root` colors.

c) **Hard-coded hex in partials or components.** Grep component partials, `.razor.css` files, and inline styles in the RCL and demo for hex values / rgb() colors. Everything found becomes a token reference. (Header bar and sidebar staying purple strongly suggests these use hard-coded or base-scope values.)

d) **Load order/specificity.** Theme scope selectors must have equal-or-higher specificity than any base rule and load after them.

## 2. Enforce token parity mechanically

Add a build-time/test-time check (script + xunit test) that: parses the theme CSS files, extracts the set of custom properties defined in the `plum-dark` scope, and fails with a named list if any other theme scope (plum-light, ocean-*, dagobah-*) is missing one. Run it now and complete the dagobah/ocean token sets until the test passes. This makes "incomplete theme" a build failure instead of a visual surprise.

## 3. Switcher UX fixes

- **Dropdown stays open after selection:** the AgtThemeSwitcher popup must close on select (wire the Radzen DropDown `Change` to close, or use the component's close API); focus returns to the trigger; Escape also closes.
- **Page doesn't re-theme on switch:** the attribute must be swapped on `<html>` synchronously in the same interop call; because all styling is CSS-variable based, no Blazor re-render should be needed — if parts only update after navigation, they are reading theme values from C# (parameters/inline styles) instead of CSS; find and eliminate those.
- Selected theme shows in the trigger, persists (localStorage), and the anti-FOUC head snippet applies it before first paint.
- The light/dark toggle keeps working within the active family after switching families.

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green including the new token-parity test and catalog smoke tests per theme. Manual walk: switch Plum → Ocean → Dagobah on the Text Inputs, Buttons, DataGrid, Pickers (open the popups!), and Navigation pages — header, sidebar, canvas, inputs, popups must all switch instantly with zero leftover purple/gold. Then switch while a dialog is open: the dialog re-themes too. Report per root cause (a–d) what was found and fixed.
