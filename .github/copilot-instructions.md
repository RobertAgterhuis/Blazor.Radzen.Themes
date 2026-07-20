# Agterhuis.Ui — repository instructions

Internal Blazor design system: a Razor Class Library (`src/Agterhuis.Ui`) packaged as a NuGet package (Azure Artifacts, feed in project ICT365.NuGet) containing Radzen-based wrapper components, a multi-theme token system, and static assets. Consumers are internal Blazor Web Apps. PackageId stays `Agterhuis.Ui` (repo name `ICT365.NuGet.UI.Theme` intentionally differs).

## Project layout

- `src/Agterhuis.Ui` — the RCL (only packable project, `Sdk=Microsoft.NET.Sdk.Razor`, net10.0)
- `samples/Agterhuis.Ui.Demo` — demo/dev harness with component pages + full Radzen catalog
- `tests/Agterhuis.Ui.Tests` — xunit + bUnit v2 (`BunitContext`)
- `prompts/` — numbered work prompts (history of how this repo was built; keep numbering when adding)
- `docs/` — living evidence docs (see "Docs that must stay current" below)

## Theme system (the heart of this repo)

- Six theme FAMILIES, each with a light and dark variant: `plum` (default, "Plum Ink"), `ocean`, `dagobah`, `dathomir`, `hoth`, `tatooine`. Variant ids are `<family>-light` / `<family>-dark`.
- The active theme is the `data-agt-theme` attribute on `<html>` — never on an inner element, because Radzen renders popups/dialogs in a portal at body level and they must inherit the theme.
- **Color values exist ONLY inside `[data-agt-theme="..."]` scopes** (theme files under `wwwroot/css/themes/`). `:root` holds structural tokens only (spacing, radius, fonts, motion, z-index). This is enforced by the token-audit tests — never define a color in `:root`, a bare class, `.razor.css`, inline styles, or C# code. The only code-level exception is `AgtTheme` preview metadata (`PreviewCanvas`, `PreviewPrimary`, `PreviewAccent`) for family swatches in the switcher. New colors = new theme-scoped tokens, added to EVERY family (the parity test fails otherwise).
- Radzen is themed by mapping `--rz-*` variables to `--agt-*` tokens in `css/theme/_variables.css`; component families have partials (`_buttons.css`, `_navigation.css`, ...). Wrap, override — never fork Radzen CSS wholesale.
- **On-accent rule**: any surface filled with the theme accent (gold, lightsaber green, scarlet, pilot orange, twin-suns) gets dark text/icons via the per-theme `--agt-on-accent`. Never white text on a vivid accent fill.
- **Paired fill rule**: any fill token that can carry visible text or icons must have a matching `--agt-on-*` foreground token in every theme family. Add the foreground token before using the fill in components or theme partials.
- **Accent budget**: the accent carries meaning (active nav edge, primary CTA, focus, selection, brand mark) and stays ≤ ~5% of pixels. Neutrals carry ~80% of the interface; the family's primary color is for interaction, not wallpaper.
- Personality tokens are per-theme too: display/body fonts (bundled OFL woff2 in `wwwroot/fonts`, no CDN), radius scale, 2-layer tinted shadows, canvas atmosphere, glow, glass. Glass/backdrop-filter only on floating layers (header, dialogs, popups, notifications) with `@supports` fallbacks.

## Theme switching

- `AgtThemeState` (scoped DI) is the single source of truth; `AgtThemeSwitcher` changes family, `AgtThemeToggle` flips light/dark within the family.
- Switch flow: close all Radzen popups first (`agtTheme.closeAllPopups` — unconditional multi-sweep hide, do not weaken it), yield, then set the theme; the layout applies it via `agtTheme.setThemeWithTransition` (View Transitions API with reduced-motion + no-support fallbacks). Persist in localStorage; the anti-FOUC head snippet applies the stored theme before Blazor boots.

## Motion & ambient

- Animations use transform/opacity only; `prefers-reduced-motion` disables everything (crossfade, ambient, sheen, stagger, count-up, shimmer).
- Ambient atmosphere is controlled by `AgtUiOptions.EnableAmbientEffects`, pauses on hidden tabs, and is OFF on calm (data-dense) routes: datagrid, pivot, scheduler, gantt pages stay motion-still.

## Component rules

- Prefix `Agt`; **wrap Radzen components, never inherit**; CSS isolation per component; tokens only — zero hex outside token/theme files.
- Form wrappers REQUIRE `Label` or `AriaLabel` (a `Debug.Fail` guard fires otherwise — tests must pass one too).
- Intent via own enums (`AgtIntent`); low-level passthrough may expose Radzen types.
- Every component: demo page in samples + ≥2 bUnit tests. The catalog must cover EVERY component of the installed Radzen.Blazor version — `docs/RADZEN-COMPONENT-INVENTORY.md` may have zero rows without a demo link; regenerate it after a Radzen upgrade.
- Demo examples are capability-driven: provide as many examples as materially distinct capabilities, minimum 1. Never duplicate equivalent examples just to hit a numeric quota.
- Demo and showcase copy must follow docs/CONTENT-GUIDELINES.md: professional-direct tone, sentence case, verb-first actions, nl-NL as the default language, and no exclamation marks in system text.
- All 8 ButtonStyles × 4 Variants have explicit foregrounds per theme; the Buttons catalog page shows the full matrix.

## Accessibility (WCAG 2.2 AA — hard gate)

Contrast ≥ 4.5:1 (text) / 3:1 (large text, UI boundaries) in every theme and variant; visible `:focus-visible` everywhere (never obscured); targets ≥ 24×24px; real labels (`aria-describedby` for validation, `aria-live` announcements); information never by color alone (icons/text accompany semantic color); reflow at 400% zoom. Where aesthetics and AA conflict, AA wins.

## Testing conventions

- bUnit v2: `BunitContext`, `ctx.Services.AddRadzenComponents()`, `ctx.JSInterop.Mode = JSRuntimeMode.Loose`; JS handlers need `SetupVoid(...).SetVoidResult()` or async handlers hang.
- Guard tests that must stay green: token parity + completeness, token-bleed audits, a11y contract tests, per-theme catalog smoke tests (a crashing catalog page fails the suite), theme-switcher popup-close test.
- `TreatWarningsAsErrors` is on: zero-warning builds, always.

## Dependencies & build

- Central Package Management (`Directory.Packages.props`), no versions in csproj, no wildcards; lock files committed (`dotnet restore --force-evaluate` after dependency changes); `--locked-mode` in CI.
- `Radzen.Blazor` is a plain `PackageReference` in the RCL (never `PrivateAssets="all"` — it must flow to consumers). Treat a Radzen upgrade as a controlled change: rerun the component inventory, theme-coverage check, and visual regression walk.
- Never copy/embed Radzen premium theme assets; no secrets/PATs anywhere.
- Consumer CSS order: Radzen `material-base.css` → `agt-theme.css` → `agt-utilities.css` → app css; plus `theme-interop.js` and the anti-FOUC snippet (see `docs/CONSUMING.md`).

## Docs that must stay current (update in the same PR as the change)

`docs/THEMING.md` (how to add a theme), `docs/THEME-COVERAGE.md`, `docs/RADZEN-COMPONENT-INVENTORY.md`, `docs/A11Y-CONTRAST.md`, `docs/TOKEN-AUDIT.md`, `docs/CONSUMING.md`, `CHANGELOG.md` (Keep a Changelog).

## What does NOT belong in this library

Business logic, API clients, EF models, app-specific pages/routes/authorization, environment configuration.

## Versioning & releases

SemVer. Breaking = renamed/removed components, parameters, CSS classes, **tokens or theme names/variant ids**, Radzen major bump, TFM change → major version. Stable releases only via git tag `v*` on `main`; the Azure Pipeline (project ICT365.NuGet) publishes to Azure Artifacts. Never publish from a local machine.
