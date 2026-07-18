# Prompt 17 — Signature experience: smooth én indrukwekkend

Goal: push beyond "correct + distinctive" to "memorable". Add a small number of high-impact signature moments per theme — silky smooth (transform/opacity only, 60fps) and impressive, never gimmicky. Data-dense screens stay calm; the wow lives in the shell, the transitions, and the landing page.

---

Copy below into Claude Code in the repo root:

---

Add a signature-experience layer to Agterhuis.Ui. Hard constraints throughout: animations use transform/opacity only (no layout-shifting properties), 60fps on a mid-range laptop, `prefers-reduced-motion` disables ALL of it, contrast and the accent budget stay intact, and data-dense pages (DataGrid, PivotGrid, Scheduler, Gantt) receive NO ambient motion — calm is a feature there.

## 1. Buttery theme switching (the showpiece)

Switching themes must feel cinematic instead of instant-jarring:
- Preferred: View Transitions API (`document.startViewTransition`) around the attribute swap — a 350ms crossfade of the whole viewport; feature-detect and fall back to a CSS `transition` on background/color for browsers without support.
- The switcher dropdown itself: theme options render as mini-previews (small swatch strip: canvas + primary + accent per option), current theme has the accent edge. Selecting closes the popup (per prompt 13) and the crossfade carries the change.

## 2. Ambient atmosphere v2 (alive, but barely)

Upgrade the static backdrops from prompt 16 with ONE ultra-subtle motion layer per theme, implemented as a single GPU-cheap element (CSS animation on transform/opacity, or one low-DPI canvas with ≤ 30 particles):
- Plum: aurora gradient drifts imperceptibly (120s loop).
- Dagobah: two mist bands slowly translating in opposite directions.
- Dathomir: ember glow breathes (slow opacity pulse, 8s).
- Hoth: sparse snow drift (≤ 20 particles, slow, only on Home/Theme pages).
- Tatooine: heat-shimmer only on the hero panel (SVG turbulence at very low amplitude).
Controls: `AgtUiOptions.EnableAmbientEffects` (default true), automatically off for reduced-motion, off on data-dense routes, paused when the tab is hidden (`visibilitychange`).

## 3. Shell jewelry (shared implementation, theme-flavored)

- Header brand: one-time sheen sweep across the brand name on app load (1s, once per session).
- Nav: active-edge indicator SLIDES between items (FLIP or transform-based), hover background eases in 150ms; collapsed-rail icons get a soft glow on active.
- Dialogs: scale(0.96→1) + fade over 200ms with `--agt-ease-spring` (add spring-flavored easing tokens `--agt-ease-spring`, `--agt-ease-out-expo`); backdrop fades with blur ramp.
- Toasts/notifications: slide-in with slight overshoot, progress hairline in accent.
- Tabs/Steps: indicator moves with spring physics; content crossfades 150ms.
- Featured cards (Home + catalog index tiles): 1px gradient border (theme primary→accent, subtle), hover lifts with the 2-layer shadow + border brightens; card grids get a one-time staggered entrance (30ms stagger, ≤ 8 items, once per navigation).
- DataGrid (allowed micro-only): row hover slides a 2px accent edge in from the left (transform), sort-arrow rotation animates 150ms. Nothing else.

## 4. Landing page as showcase

Redesign the demo Home into a per-theme showpiece: hero panel with the theme's atmosphere + display typography + one-line theme description; metric cards with count-up animation (600ms, once, formatted numbers); a sparkline strip in theme series colors; quick links to the catalog as featured cards. This page is where each theme gets to show off — screenshot-worthy in all five families.

## 5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (parity + smoke; add a bUnit test that reduced-motion classes/knob disable animation hooks). Manual: switch through all five themes with the crossfade (smooth, no flash of unthemed content), check 60fps on Home and Buttons (DevTools performance trace — no long tasks from animation), verify DataGrid/Scheduler pages are motion-calm, ambient pauses on hidden tab, and everything is dead-still under `prefers-reduced-motion`. Report: per theme the signature moments implemented, the fallback path used for View Transitions, and the performance trace summary.
