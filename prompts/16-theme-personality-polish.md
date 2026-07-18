# Prompt 16 — Theme personality layer (van "andere verf" naar "ander karakter")

Diagnosis: the themes are color-swaps of the same flat UI. Visual appeal comes mostly from NON-color qualities: typography, depth, background atmosphere, shape language, and micro-details. This prompt adds a "personality token" layer so each theme differs in character, not just paint — while the parity test keeps enforcing completeness.

---

Copy below into Claude Code in the repo root:

---

Add a personality layer to the Agterhuis.Ui theme system and use it to make each theme visually distinctive. All new tokens must be defined by EVERY theme (extend the parity test to include them). WCAG 2.2 AA, reduced-motion/transparency support, and the accent budget remain hard constraints.

## 1. New personality tokens (define per theme family)

- **Typography**: `--agt-font-display` (headings) + `--agt-font-body`, plus `--agt-heading-weight`, `--agt-heading-tracking`. Ship 2–3 OFL-licensed fonts as subsetted woff2 in the RCL `wwwroot/fonts` (no CDN dependency; document licenses in THIRD-PARTY-NOTICES). Pairings, for example: Plum = a confident geometric display (Sora/Manrope-class) + neutral body; Dagobah = humanist/organic; Dathomir = sharp condensed; Hoth = light, wide-tracked; Tatooine = warm slab-ish display. Body stays highly readable everywhere.
- **Shape**: `--agt-radius-scale` personality — Plum 0.5rem baseline; Dagobah rounder (organic); Dathomir near-square (blade-sharp, 2px); Hoth soft-ice (large radius on cards, small on controls); Tatooine gently rounded. Applied via the existing radius tokens so components need no changes.
- **Depth**: replace single flat shadows with 2-layer shadows (`--agt-shadow-*`: tight contact layer + wide diffuse layer), tinted per theme (plum-tinted, swamp-green-tinted, red-black, ice-blue, warm umber). Elevation reads instantly but stays subtle.
- **Atmosphere**: `--agt-canvas-backdrop` — a static, near-subliminal background treatment per theme (max ~4–6% intensity, pure CSS gradients, no images): Plum = faint radial aurora top-left; Dagobah = low horizontal mist bands; Dathomir = deep vignette with faint ember glow bottom; Hoth = cold top-light with faint horizon line; Tatooine = warm heat-haze gradient with a second faint "sun" radial. Must never reduce text contrast (measure against worst-case stop) and must look clean at 4K and mobile.
- **Selection & focus flavor**: per-theme hover tint, selection tint, focus ring (color already themed; add per-theme ring style within a11y limits), and glow intensity.

## 2. Micro-interaction & finishing pass (theme-aware, shared implementation)

- Hover: interactive cards/rows lift 1–2px with the theme shadow; nav items get a 150ms background ease; buttons get pressed state (scale 0.98).
- Active indicators (tabs/steps/nav edge): animated slide/grow transition of the accent indicator.
- Skeleton/loading shimmer in AgtLoadingPanel using theme surface tones.
- Page headers: title in `--agt-font-display`, accent underline animates in once on page load (respect reduced-motion), muted breadcrumb line above.
- Empty states: give AgtEmptyState a simple generative CSS/SVG motif per theme (abstract shapes in theme colors — own work, no franchise imagery).
- Density polish: consistent vertical rhythm on demo/catalog pages (page header → toolbar → content spacing via spacing tokens), so pages stop looking like loose stacked blocks.

## 3. Guardrails

- Extend the token-parity test with all new personality tokens.
- Contrast re-check for text over `--agt-canvas-backdrop` worst-case stops; add pairs to docs/A11Y-CONTRAST.md.
- `prefers-reduced-motion` kills the animations; `prefers-reduced-transparency`/`prefers-contrast: more` simplify atmosphere and glass.
- Performance: no `backdrop-filter` beyond floating layers, no animated gradients, shadows GPU-friendly; verify no scroll jank on the DataGrid page.
- Fonts: swap-safe loading (`font-display: swap`), fallback stacks that hold layout (size-adjust if needed).

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (extended parity test included). Walk Home, Buttons, Text Inputs, DataGrid, Pickers, Navigation in all five families, both variants: each theme should now be recognizable at a glance from silhouette/typography/atmosphere alone — not just hue. Report per theme: font pairing, radius scale, shadow recipe, backdrop recipe, and the re-measured contrast pairs.
