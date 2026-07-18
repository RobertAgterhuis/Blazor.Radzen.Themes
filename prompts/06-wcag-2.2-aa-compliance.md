# Prompt 6b — WCAG 2.2 AA-conforme UI (accessibility compliance pass)

Requirement: "De gebruikersinterface moet voldoen aan WCAG 2.2 niveau AA, inclusief eisen voor kleurcontrast, toetsenbordnavigatie, focusweergave, semantiek en screenreader-ondersteuning." (Aligned with EN 301 549 for Dutch government contexts.)

---

Copy below into Claude Code in the repo root:

---

Make the Agterhuis.Ui design system (RCL + demo app) **WCAG 2.2 level AA conformant**. Radzen components ship with ARIA and keyboard support — our theme and wrappers must preserve and extend that, never break it. Work through the audit below, fix everything, and produce evidence.

## 1. Color contrast (SC 1.4.3, 1.4.11)

- Audit EVERY text/background token pair in both light and dark mode: body, muted, headings, nav items, group headers, placeholders, disabled states, links, button labels, badge/chip text, validation messages, chart axis labels, tooltips, notifications.
- Text ≥ 4.5:1; large text (≥ 24px or 19px bold) ≥ 3:1; UI component boundaries and states (input borders, focus rings, icons, checkbox/radio/switch states, slider tracks) ≥ 3:1 against adjacent colors.
- Known risk areas from our palette: gold `#f1ce05` NEVER carries text on light surfaces and never has white text on it; glassmorphic surfaces must be tested against their WORST-CASE backdrop (compute against the most extreme gradient stop behind them); disabled states must stay perceivable.
- Write a contrast matrix (token pair → ratio → pass/fail → fix) into `docs/A11Y-CONTRAST.md`. Adjust token values where needed (create accessible variants, e.g. a darker gold for text use) rather than one-off hex fixes.

## 2. Keyboard operability (SC 2.1.1, 2.1.2, 2.4.3, 2.4.7)

- Every interactive element reachable and operable by keyboard alone: sidebar menu (arrow keys within PanelMenu, Enter/Space to activate, Escape closes overlays), theme toggle, all Agt wrappers, DataGrid (header sort, pager, row selection), dialogs (focus trap + Escape + focus restore to trigger), dropdowns/pickers (arrow navigation, typeahead, Escape).
- Logical tab order on every demo/catalog page; no keyboard traps; no positive `tabindex`.
- Add a **skip link** ("Naar hoofdinhoud") as the first focusable element in the demo layout, styled visible on focus — and ship it as `AgtSkipLink` in the RCL.

## 3. Focus visibility — WCAG 2.2 specifics (SC 2.4.7, 2.4.11 Focus Not Obscured, 2.4.13 where feasible)

- Every interactive element gets a clearly visible `:focus-visible` indicator: ≥ 2px, ≥ 3:1 contrast against both the component and its background, offset so it's never clipped by `overflow:hidden` containers (audit cards, sidebar, grid cells).
- Focused elements must never be fully hidden behind sticky headers/sidebars (2.4.11): verify scroll-padding/scroll-margin on the layout.
- No `outline: none` anywhere without a compliant replacement.

## 4. Target size — WCAG 2.2 (SC 2.5.8)

All click/touch targets ≥ 24×24 CSS px (aim 44×44 for primary actions): icon buttons (theme toggle, sidebar collapse, dialog close, grid filter icons), pager numbers, chips, tree expanders, stepper dots. Fix via padding/min-sizes in the theme, not per-page hacks.

## 5. Labels, semantics & screenreader support (SC 1.1.1, 1.3.1, 2.4.6, 3.3.2, 4.1.2)

- Every Agt form wrapper renders a real associated `<label>` (via RadzenFormField or `for`/`id` pairing) — placeholder is never the only label. Add a `Label` parameter where missing; require either Label or AriaLabel (Debug.Assert or analyzer-style doc note).
- Validation messages programmatically linked (`aria-describedby`), errors announced (`aria-live="polite"` region in AgtFormActions or per field); required fields marked with `required`/`aria-required`, not color alone.
- Landmarks in demo layout: `<header>`, `<nav aria-label="Hoofdnavigatie">`, `<main id="main">`, `<footer>`; exactly one `<h1>` per page and a logical heading hierarchy on every demo/catalog page.
- Icon-only buttons get `aria-label`; decorative icons `aria-hidden="true"`. Images in demos get meaningful `alt` or `alt=""`.
- Loading states (AgtLoadingPanel) announce via `aria-busy`/`role="status"`; AgtEmptyState uses proper heading + text semantics; notifications use `role="alert"` where appropriate (verify Radzen defaults, supplement only if missing).
- Language: demo pages set `<html lang>` correctly; mixed NL/EN labels get `lang` attributes where needed.

## 6. Don't rely on color alone (SC 1.4.1)

Audit: validation errors (icon + text, not just red border), required markers, chart series (add distinct markers/patterns or direct labels — verify the brand series palette is distinguishable; document a color-blind-safe ordering), status badges (icon or text prefix), links inside body text (underline, not color-only), sort direction on grid headers (arrow icon).

## 7. Reflow, zoom & motion (SC 1.4.4, 1.4.10, 2.3.3)

- 200% browser zoom: no loss of content/function; 400% (≈320px width): layout reflows, sidebar becomes overlay, no horizontal scroll for text content.
- Font sizes in rem (audit for px font sizes in theme partials); respect user font scaling.
- `prefers-reduced-motion` disables all transitions/animations (verify the existing block covers new Royal Nebula animations); `prefers-reduced-transparency`/`prefers-contrast: more` fallbacks for glass surfaces.

## 8. Automated + manual verification (evidence)

1. Add an automated a11y scan: Playwright + `@axe-core/playwright` job that walks every demo/catalog route in both themes and fails on WCAG violations; wire it as `dotnet`/npm script and document in README (CI-friendly, but running locally is acceptable for now).
2. bUnit assertions for the wrapper contracts: label association, aria-label on icon buttons, aria-describedby on invalid fields, skip link present.
3. Manual pass with keyboard only through every page; document findings.
4. Produce `docs/ACCESSIBILITY.md`: conformance target (WCAG 2.2 AA / EN 301 549), the contrast matrix reference, component-by-component status table, known limitations (e.g. third-party embeds like GoogleMap/SSRSViewer marked as exceptions), and guidance for consumers (they must set page titles, lang, and heading structure in their own apps).
5. `dotnet build -c Release` zero warnings, `dotnet test` green.

Do not weaken the visual design where it already passes; where aesthetics and AA conflict, AA wins and you propose the closest compliant alternative (e.g. darker gold text token).
