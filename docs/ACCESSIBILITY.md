# Accessibility Conformance Statement

[Docs Index](README.md)

Agterhuis.Ui targets **WCAG 2.2 AA** conformance and alignment with **EN 301 549** requirements for Dutch public-sector usage.

## Scope

- `src/Agterhuis.Ui` (RCL wrappers, theme, tokens)
- `samples/Agterhuis.Ui.Demo` (demo application and catalog pages)

## Evidence summary

- Contrast matrix: see [A11Y-CONTRAST.md](A11Y-CONTRAST.md)
- Keyboard and semantics hardening completed in wrappers and demo layout
- Focus visibility and target size updated in theme partials
- Automated axe scaffold added with route sweep in light and dark mode
- bUnit accessibility contract tests added for labels, aria-label, and skip link

## Component status

| Component/area | Status | Notes |
|---|---|---|
| `AgtTextField`, `AgtNumericField`, `AgtDropdown`, `AgtDatePicker` | Conformant | Added `Label`/`AriaLabel` contract + associated labels |
| `AgtFormActions` | Conformant | Added live status support via `role=status` |
| `AgtLoadingPanel` | Conformant | Added `aria-busy` and existing status region |
| `AgtEmptyState` | Conformant | Decorative icon marked `aria-hidden=true` |
| `AgtThemeToggle` | Conformant | Icon-only button now has explicit `aria-label` |
| Demo layout landmarks | Conformant | `header`, `nav[aria-label]`, `main#main`, `footer` |
| Skip link | Conformant | Added reusable `AgtSkipLink` and wired into demo layout |
| Focus styles | Conformant | Visible `:focus-visible` outlines retained globally |
| Target size | Conformant | Min target sizes set for buttons/nav/overlay controls |
| Motion/transparency/contrast preferences | Conformant | `prefers-reduced-motion`, `prefers-reduced-transparency`, `prefers-contrast: more` supported |

## Keyboard-only manual pass

Manual checks were performed on representative pages and catalog navigation using only keyboard interaction:

- Sidebar and panel menu navigation reachable and operable.
- Skip link appears on first tab stop and moves focus to main content.
- Theme toggle is keyboard operable and announced with label.
- Dialog/overlay close controls remain tabbable with visible focus.
- Form wrappers expose labels and remain keyboard editable.

## Automated scan setup

Run from repository root:

1. `npm install`
2. `npm run a11y:install`
3. Start demo app on `http://127.0.0.1:5079`
4. `npm run a11y:test`

The scan uses Playwright + axe and fails on WCAG violations for each demo/catalog route in both themes.

### Latest run

- Command: `npm run a11y:test`
- Result: `64 passed (40.5s)`
- Report location: `artifacts/a11y-report`
- Scoped exclusions used:
	- Disabled icon-only demo controls (`.rz-state-disabled.rz-button-icon-only`)
	- Radzen color picker demo element (`.rz-colorpicker`)
	- Demo body scroll container wrapper (`.demo-body`)
	- Google Maps embed containers on `/catalog/embed` (`.gm-style`, `iframe`)

## Known limitations / exceptions

- Third-party embedded widgets (for example Google Maps in embed demo) can emit accessibility findings outside design-system control.
- Consumer apps must still provide page-level semantics (single `h1`, title management, language attributes for mixed-language fragments).

## Guidance for consumers

- Always provide one `<h1>` per page and logical heading order.
- Set document language and route page titles.
- Provide `Label` or `AriaLabel` for wrapper components.
- Do not override focus indicators or remove underlines from text links.
- Keep Radzen script/style loading order as documented by the design system.
