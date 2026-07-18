# Prompt 18 — Signature-experience completion audit (prompt 17 nalanden)

A code inspection shows prompt 17 landed partially. Confirmed present: theme-interop.js with startViewTransition, spring easing tokens, bundled woff2 fonts, switcher swatch previews, Home hero with ambient layers + count-up + sparkline + stagger, visibilitychange handling, nav indicator transitions. Suspected missing or incomplete: shell-wide atmosphere (ambient exists ONLY on Home/Theme pages), header brand sheen, DataGrid row-hover accent edge, toast overshoot, tabs/steps spring indicator + content crossfade, featured cards on the catalog index, reduced-motion bUnit coverage. This prompt is an audit: verify every item, finish the gaps, prove it.

---

Copy below into Claude Code in the repo root:

---

Audit the implementation of the signature-experience layer against the checklist below. For EACH item: locate the implementing code (file + selector/method), test it, and mark it LANDED or MISSING in a table in your final report. Implement everything MISSING. Do not re-implement what already works.

## Checklist

1. **Theme crossfade**: `startViewTransition` wraps the attribute swap; CSS fallback path actually triggers in a browser without the API (verify the feature-detect branch); no flash of unthemed content; switcher popup closes on select.
2. **Switcher previews**: swatch strips render the TARGET theme's colors (not the active theme's — check how the swatch colors are sourced; hard-coded per option or resolved from the theme file?), current theme marked with accent edge.
3. **Shell-wide atmosphere**: the per-theme canvas backdrop must live on the APP SHELL (layout canvas), not only inside the Home/Theme hero. Data-dense routes (datagrid, pivot, scheduler, gantt, data-advanced) get the static gradient only — no motion. Currently ambient markup exists only in Home.razor/Theme.razor — move/extend it to the layout with the route-based calm rule, `EnableAmbientEffects` option respected, paused on hidden tab.
4. **Header brand sheen**: one-time sheen sweep on app load, once per session (sessionStorage flag), reduced-motion off.
5. **Nav active indicator**: verify it visually SLIDES between items (transform-based), not just fades; collapsed-rail active glow present.
6. **Dialogs**: scale+fade with spring easing; backdrop fade; verify on ConfirmDialog and a catalog dialog.
7. **Toasts/notifications**: slide-in with slight overshoot + accent progress hairline.
8. **Tabs/Steps**: animated indicator movement + 150ms content crossfade.
9. **Featured cards**: gradient border + hover lift + staggered entrance on Home AND the catalog index tiles (currently only Home has stagger).
10. **DataGrid micro**: row hover slides a 2px accent edge in (transform only); sort arrow rotates 150ms; NOTHING else animates in the grid.
11. **Loading shimmer**: AgtLoadingPanel shimmer uses theme surface tones (verify in dagobah + hoth, not just plum).
12. **Reduced motion**: `prefers-reduced-motion` kills crossfade, ambient, sheen, stagger, count-up (values render final immediately), shimmer becomes static; add/verify a bUnit test asserting the animation-disabling hook (class/attribute) is applied.
13. **Performance**: DevTools-style check — animations only on transform/opacity (grep the new CSS for animated width/height/top/left/margin/box-shadow transitions and replace), ambient is a single composited layer per page.
14. **Options plumbing**: `EnableAmbientEffects=false` in AgtUiOptions actually removes ambient markup (not just hides it), and this is documented in docs/CONSUMING.md.

## Verification

`dotnet build -c Release` zero warnings; `dotnet test` green including the reduced-motion test. Walk all five themes: Home (full signature), a data-dense page (calm), theme switch crossfade, dialog + toast + tabs animations. Final report: the LANDED/MISSING table with file references per item, plus what you changed.
