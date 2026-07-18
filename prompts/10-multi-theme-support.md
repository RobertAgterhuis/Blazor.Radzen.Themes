# Prompt 10 — Multi-theme support + theme switcher

The token architecture makes extra themes cheap: components read `--agt-*` tokens only, so a new theme is a token-value file — no component CSS. This prompt generalizes light/dark into named themes and adds a switcher.

---

Copy below into Claude Code in the repo root:

---

Add multi-theme support to Agterhuis.Ui. Components must not change — themes are token overrides only.

## 1. Theme architecture

- Generalize the current mode attribute into `data-agt-theme="<name>"` on `<html>` with these themes: `plum-light` (current light), `plum-dark` (current dark, DEFAULT), and one new theme pair as proof: `ocean-light`/`ocean-dark` (deep teal `#0b6e6e`-family primary, warm amber `#e8a13d` accent — derive a full 50–950 scale + on-accent + surfaces + plum-equivalent neutrals with the same token names).
- File layout: `wwwroot/css/themes/agt-theme.plum.css` and `agt-theme.ocean.css`, each containing ONLY token values scoped to their `[data-agt-theme="..."]` selectors (light + dark variant per file). `agt-tokens.css` keeps structural tokens (spacing, radii, fonts, motion) that are theme-invariant. All component partials keep reading tokens — zero hex in partials stays enforced.
- Backwards compat: `data-agt-theme="dark"`/absent attribute keep working (map to plum-dark/plum-light via selector aliases), so existing consumers don't break. Semantic colors, focus rings, and chart series palettes come from tokens too — verify each theme defines the full set (list missing tokens as build-time doc check in a script or test).
- Contrast: each theme must pass the same WCAG 2.2 AA gates; add the ocean pairs to docs/A11Y-CONTRAST.md.

## 2. RCL API

- `AgtTheme` record/enum in the RCL: name, display name, dark/light variant ids.
- Extend `AgtUiOptions` with `DefaultTheme` (default `plum-dark`) and `AvailableThemes`.
- `AgtThemeSwitcher` component (replaces/extends AgtThemeToggle): dropdown with theme names + a light/dark toggle that switches within the current theme family; sets the attribute on `<html>` via JS interop; persists in localStorage; no FOUC (apply persisted theme in an inline head script before Blazor boots — document the snippet in docs/CONSUMING.md).
- Keep AgtThemeToggle working as the simple light/dark flip within the active family.

## 3. Demo app

- Header: AgtThemeSwitcher next to the existing toggle position.
- Theme page: show the active theme's full token swatches and a side-by-side comparison of the theme families.
- Verify portaled overlays (dialogs, dropdown panels, calendar) follow the switched theme instantly — attribute must be on `<html>` (prompt 8's fix) and no cached colors.

## 4. Docs & tests

- docs/THEMING.md: how a consumer creates their OWN theme (copy a theme css, replace token values, register the name in options) — this is the recipe for future customer/app-specific themes.
- bUnit: switcher renders all configured themes, sets attribute, persists choice; smoke test that every catalog page renders under ocean-dark.
- `dotnet build -c Release` zero warnings; `dotnet test` green. Walk key pages in all four themes; report anything that only looks right in plum (that indicates a hard-coded value — fix at the source).
