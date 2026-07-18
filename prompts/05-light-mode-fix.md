# Prompt 5b — Light mode overhaul

Observed in the running demo (light mode): sidebar group headers and nav items render white/near-white on a white sidebar (invisible), active nav items are white text on pale lavender pills (fails contrast), the gold logo text sits on a white header bar (unreadable), the page background is a washed-out pink radial fading to white, and the sidebar scrollbar is a harsh dark-purple full-height bar. Dark-mode foreground values are leaking into light mode.

---

Copy below into Claude Code in the repo root:

---

Fix and redesign light mode for the Agterhuis.Ui theme. Dark mode is the reference experience and must not change visually.

## 1. Root cause first — variable scoping audit

Audit `agt-theme.css` (and partials): every color variable must be defined TWICE — light values in `:root`, dark values in `[data-agt-theme="dark"]`. Find every place where a dark-mode value (white/light text, glass-dark backgrounds, glow colors, gradient stops) is defined only in `:root` or hard-applied on a class without a light counterpart. The white-on-white sidebar means nav/group-header text colors are dark-mode values in shared scope. Fix the scoping so each mode is complete and self-consistent, then apply the design below.

## 2. Light mode design spec — "Royal Nebula — Day"

Light mode is NOT inverted dark mode. It is a bright, airy, paper-like UI where purple is the protagonist color and gold is a scarce accent:

- **App background**: near-white with the faintest purple tint (`#faf8fc`-class token, define as `--agt-surface-0` light) and a very subtle radial highlight top-left in primary alpha-4 max. No pink washes, no visible gradient banding.
- **Surfaces**: white cards (surface-1) with primary alpha-8 borders and soft purple-tinted shadows (`0 2px 8px rgb(56 8 88 / 8%)`). Elevation = shadow strength, not background darkening.
- **Text**: body `--agt-color-gray-900`; muted `--agt-color-gray-500`; headings primary-950. NEVER white text on any light surface.
- **Interactive/link color**: primary-600; hover primary-700. 
- **Gold usage in light mode**: gold is for EDGES and FILLS ONLY (active indicators, underline accents, filled pager circle with gray-900 text, secondary button borders). Never gold text on white — where dark mode uses gold text, light mode uses accent-700 (`#9e8715`) at minimum or primary-600 instead.

## 3. Sidebar & header (the broken parts)

- Sidebar: white glass (`rgb(255 255 255 / 78%)` + blur) over the app background, right border primary alpha-10.
- Group headers: uppercase, 11px, letter-spaced, `--agt-color-gray-500` — clearly visible.
- Nav items: gray-700 text + primary-600 icons; hover = primary alpha-6 bg, text primary-700; active = primary-100 background, primary-800 text, 3px gold left edge, gold icon; focus-visible = 2px primary-600 ring. No white text anywhere in the light sidebar.
- Header bar: white glass, bottom border primary alpha-10. Brand: "ICT365 · AGTERHUIS.UI" in primary-700 bold, with a small gold underline bar or gold dot separator for brand recognition (gold text on white is forbidden). Theme toggle icon gray-700, hover primary-600.
- Scrollbars (light): 8px, thumb primary alpha-25 (hover alpha-40), transparent track — no solid dark bars.

## 4. Component sweep in light mode

Re-check each family with the new tokens: DataGrid header = primary-50 bg, primary-900 text, gold 2px bottom border; row hover primary alpha-4; selected = primary-100 + gold edge. Inputs: white bg, gray-300-ish border (primary alpha-15), focus = primary-600 border + primary alpha-15 ring (gold ring only in dark mode). Dialogs/popups: white glass, shadow-md purple-tinted. Buttons: primary filled unchanged (white text on primary-500/600 passes); secondary = primary-600 outline, hover primary alpha-8 fill (not gold fill in light). Alerts/badges/notifications: pastel semantic backgrounds (alpha-12) with dark semantic text. Charts: same series palette but gridlines gray-200 and axis text gray-500 on white.

## 5. Contrast gates

Automated sanity pass: list every text/background pair you introduce with its contrast ratio in the final report; all body/nav/control text ≥ 4.5:1, large headings ≥ 3:1, in BOTH modes. The theme toggle transition must not flash unstyled content.

## 6. Verification

`dotnet build -c Release` zero warnings, `dotnet test` green. Walk EVERY demo + catalog page in light mode (then spot-check dark mode unchanged) and fix any remaining white-on-white, gold-on-white, or washed-out gradient. List the pages checked and the variables you re-scoped.
