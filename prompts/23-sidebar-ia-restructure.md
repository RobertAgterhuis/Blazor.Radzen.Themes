# Prompt 23 — Sidebar-informatiearchitectuur: Components vs Catalog verduidelijken

Observed: the sidebar has two neutral groups ("Components", "Catalog") with overlapping names (Buttons and Layout appear in both), and nothing explains the difference. They serve different audiences — Components = the Agt wrapper API for consumers, Catalog = raw-Radzen theme-QA coverage — so do NOT merge them; make the distinction visible and link the two per domain.

---

Copy below into Claude Code in the repo root:

---

Restructure the demo sidebar navigation and page cross-linking.

## 1. Rename and annotate the groups

- "Getting started" stays (Home, Theme).
- "Components" → **"Agt componenten"** with a one-line muted subtitle under the group header: "De wrapper-API voor consumers".
- "Catalog" → **"Radzen catalogus"** with subtitle "Theme-QA: alle Radzen-componenten in het actieve theme". Give this group a subtle visual de-emphasis (slightly muted item text or a small QA badge on the group header) so it reads as secondary/internal.

## 2. Restructure the item lists

- Agt componenten: group the flat list into the RCL's folder categories with small non-collapsing headers or dividers: Knoppen (Buttons), Formulieren (Text Field, Numeric Field, Dropdown, Date Picker, Form Actions), Data (Data Grid), Feedback (Feedback, Empty State, Loading Panel, Confirm Dialog), Layout (Layout, Sidebar Layout). Keeps scanability as the wrapper set grows.
- Radzen catalogus: rename ambiguous duplicates so they can't be confused with the Agt pages: "Buttons" → "Buttons (Radzen)", "Layout" → "Layout (Radzen)" — or prefix all catalog items consistently; pick one convention and apply it everywhere including page titles and `<PageTitle>`.
- Collapse state: Radzen catalogus starts COLLAPSED by default (persist expand state in localStorage); Agt componenten starts expanded.

## 3. Cross-link the two worlds per domain

- On each Agt component demo page: a muted link "Bekijk de rauwe Radzen-variant in het theme →" to the matching catalog page (buttons → catalog/buttons, etc.).
- On each catalog page: a banner-line "Dit is de theme-QA-weergave. Voor gebruik in applicaties: zie de Agt-wrappers →" linking back where a wrapper exists.
- The catalog Index page gets a short intro paragraph explaining its QA purpose and linking to docs/THEME-COVERAGE.md.

## 4. Guardrails

- Nav idle/hover/active states keep following the theme tokens (no regressions on the transparent-idle rule).
- Routes stay unchanged (no broken links); only labels, grouping, and cross-links change.
- Update the smoke tests if they assert on nav labels; `dotnet build -c Release` zero warnings, `dotnet test` green.

## 5. Verification

Walk the sidebar in two themes: groups clearly communicate their purpose, no ambiguous duplicate names, catalog collapsed by default, cross-links work both ways on at least Buttons and Layout. Report the final nav structure.
