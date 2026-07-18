# Prompt 4b — Visual excellence pass ("Royal Nebula" design language)

Current problems (observed in the running demo): the sidebar nav is a raw list of default-styled hyperlinks (underlined, browser link colors, no icons/grouping/active state, poor contrast on the dark purple background), surfaces are flat monotone purple with no depth or hierarchy, and the pager/grid look unfinished. This prompt is a full visual upgrade.

---

Copy below into Claude Code in the repo root:

---

Perform a complete visual upgrade of the Agterhuis.Ui theme and demo app. The target is a design language we call **"Royal Nebula"**: deep royal purple, luminous gold, glassmorphic surfaces, and subtle light effects — inspired by blog.agterhuis.net (dark purple stage with spotlight gradients and gold chevrons). Follow Radzen's own theming recommendations: style via `--rz-*` CSS variables first, `.rz-*` class overrides second, and never modify component markup assumptions. All colors derive from the existing `--agt-*` tokens; you may ADD token variations (alpha variants, gradients, glows) in `agt-tokens.css`, but never hard-code hex values elsewhere.

## 1. New token layers (add to agt-tokens.css)

- **Elevation**: `--agt-surface-0..3` — layered surfaces. Dark mode: 0 = primary-950, 1 = primary-900, 2 = mix toward primary-800, 3 = primary-800; light mode: white → gray-50 → gray-100 with purple-tinted shadows.
- **Glass**: `--agt-glass-bg` (e.g. `rgb(56 8 88 / 55%)` dark, `rgb(255 255 255 / 65%)` light), `--agt-glass-border` (1px, `rgb(241 206 5 / 15%)` dark / `rgb(104 8 152 / 15%)` light), `--agt-glass-blur: 14px`.
- **Glow**: `--agt-glow-primary` (`0 0 24px rgb(104 8 152 / 45%)`), `--agt-glow-accent` (`0 0 16px rgb(241 206 5 / 35%)`).
- **Gradients**: `--agt-gradient-hero` (radial spotlight: primary-800 → primary-950, like the blog hero), `--agt-gradient-accent` (gold accent-300 → accent-500), `--agt-gradient-surface` (subtle 145deg primary-900 → primary-950).
- **Alpha scale** for primary and accent (5/10/20/40%) for hovers, focus rings, selection.

## 2. Sidebar navigation — rebuild properly (this is the worst offender)

Replace the demo's plain link list with a real `RadzenLayout` + `RadzenSidebar` + `RadzenPanelMenu`:

- Group items: "Getting started" (Home, Theme), "Components" (the Agt wrapper demos), "Catalog" (one child per catalog family) — collapsible groups, Material icon per item.
- Sidebar surface: glass panel (`backdrop-filter: blur(var(--agt-glass-blur))`, glass bg/border) over the `--agt-gradient-hero` app background.
- Item states: default = high-contrast readable text (NEVER default link blue/purple, no underlines); hover = primary alpha-10 bg + 150ms transition; active = 3px gold left edge + accent alpha-10 bg + gold icon; focus-visible = gold ring. Ensure this styling ships in the RCL theme (`_navigation.css`), not only in the demo, so every consumer gets it.
- Header bar: slim glass bar with the app name in gold (like the blog's ICT365 logo), sidebar toggle, and the theme switcher as an icon button (sun/moon), not a labeled outline button.
- Collapsed (icon-only) sidebar mode + mobile overlay behavior must both look intentional.

## 3. Surface & depth system (theme-wide)

- App background: `--agt-gradient-hero` (dark) / soft gray with faint purple radial (light). No more flat single-color voids.
- Cards/panels: surface-1 with 1px glass border, radius-lg, shadow-sm; hover (interactive cards only) lifts to shadow-md + translateY(-2px).
- Dialogs & dropdown/picker popups: glassmorphic (blur + glass bg), radius-lg, shadow-md + subtle `--agt-glow-primary`.
- Every `backdrop-filter` usage needs an opaque fallback via `@supports not (backdrop-filter: blur(1px))`, and respect `prefers-reduced-transparency`.

## 4. Component polish

- **DataGrid**: header = gradient-surface with gold bottom border (2px accent alpha-40), uppercase-tracked header text; row hover = primary alpha-5; selected row = accent alpha-10 + gold left edge; striped rows barely visible (alpha-5); rounded outer corners; empty state uses AgtEmptyState.
- **Pager**: current page = filled gold circle with dark text + accent glow; other pages readable muted text with hover ring (currently they're near-invisible).
- **Buttons**: primary = subtle primary-500→primary-600 gradient + glow-primary on hover; secondary = gold outline, fills gold with dark text on hover; add pressed (scale 0.98) states.
- **Inputs**: surface-1 bg, 1px border primary alpha-20; focus = gold border + accent alpha-20 ring, smooth transition; floating labels styled in both modes.
- **Tabs/Steps/Accordion**: gold indicator for active, animated indicator transition.
- **Notifications/Alerts/Tooltips**: glass surfaces, semantic left edge, gold close-hover.
- **Charts**: series use the brand palette; gridlines primary alpha-10; tooltips glass.
- **Scrollbars**: thin custom scrollbars (primary-700 thumb, transparent track) in dark mode.

## 5. Motion & finish

- Global micro-interactions: 120–200ms ease transitions on interactive elements only (never layout-shifting), gentle fade/slide for dialogs and dropdown panels.
- Keep and extend the `prefers-reduced-motion` block to kill all of it.
- Typography rhythm: page titles use clamp() sizing with tight leading and a faint gold underline accent bar; establish consistent heading/body/muted hierarchy via tokens.

## 6. Accessibility gates (non-negotiable)

WCAG AA contrast in BOTH modes for text and interactive states (dark text on gold, white on purple ≥ primary-500); visible focus for every interactive element; nav readable at a glance — no low-contrast purple-on-purple text anywhere.

## 7. Verification

- `dotnet build -c Release` zero warnings, `dotnet test` green; update affected bUnit tests.
- Update the demo Theme page to showcase the new elevation/glass/glow/gradient tokens as swatches.
- Walk every catalog + component demo page in light AND dark mode and fix anything still flat, unreadable, or default-styled (especially raw `<a>` links). List the pages you checked in your final report.
