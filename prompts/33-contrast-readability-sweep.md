# Prompt 33 — Contrast- en leesbaarheids-sweep over ALLE themes (gemeten, niet op het oog)

Observed: several components have label text that is unreadable against its background in some theme/variant combinations. Instead of chasing screenshots one by one, build a MEASURED sweep: compute the actual rendered contrast of every visible text element in every theme, fail on violations, fix at the token level, and keep the checker as a guard.

---

Copy below into Claude Code in the repo root:

---

Make every text/background pair readable in every theme family and variant of this design system. Measurement first, then token-level fixes, then lock-in.

## 1. Build the contrast crawler (extends eng/screenshots / runtime probe infra)

`eng/contrast-sweep/` (Playwright + npm script, documented):
- Starts the demo; iterates ALL theme variants (every family × light/dark, incl. contrast/portal/mono families if present) × a route list covering: Home, Theme, every catalog family page, every Agt component demo page, and the showcase pages (dashboard, werkorders incl. open dialog, planning, instellingen).
- For every VISIBLE text-bearing element (walk the DOM: non-empty text nodes, plus input placeholders, labels, buttons, badges, chips, tab labels, grid headers/cells, pager, validation messages): resolve the EFFECTIVE background by walking up the ancestor chain until a non-transparent background is found (handle rgba-over-rgba compositing; for gradient/glass backgrounds sample the worst-case stop; skip elements smaller than 1px or aria-hidden decorative text).
- Compute the WCAG contrast ratio; classify with the correct threshold (4.5:1 normal, 3:1 for large text ≥24px/18.7px-bold and for disabled-exempt elements — mark disabled separately, they're exempt from SC 1.4.3 but flag anything < 2:1 as "onwaarneembaar").
- Also capture STATE variants where cheap: hover (via `page.hover`) and focus (`element.focus()`) for buttons, nav items, inputs on one representative page per theme.
- Output `docs/CONTRAST-SWEEP.md`: per theme a violations table [element (selector + visible text) | route | fg | effective bg | ratio | threshold | status], plus a summary matrix (theme × violation count). The npm script exits non-zero on violations (allowlist file with per-entry reason for justified exceptions, e.g. decorative ghost display text that is aria-hidden).

## 2. Fix — at the token source, with a small decision tree

Work the violations down to ZERO (excl. allowlisted). Per violation, in this order:
1. **Wrong token used** (e.g. muted text on a tinted surface, on-accent missing on a filled control) → point the component/partial at the correct existing token.
2. **Token value too weak in that theme** (e.g. muted step unreadable on that family's surface-2) → adjust the token VALUE in that theme's scope; re-check the token's other usages in the same theme so the fix doesn't break siblings.
3. **Missing pairing token** (a fill exists without a defined foreground — the class of bug behind earlier white-on-gold issues) → introduce the `--agt-on-<x>` token, define it in EVERY family (parity test), and use it.
Never fix with a hard-coded color or a per-page override (bleed audit must stay clean). Typical suspects from history: muted/secondary text on tinted surfaces, labels on selection fills, badge/chip text, placeholder text, validation messages on tinted panels, disabled-vs-idle distinguishability, text over glass/gradient (worst-case stop!), scheduler/gantt cell labels, chart axis labels.

## 3. Lock-in

- The sweep gets an npm script + docs section (run before releases, like the screenshot script); add a compact xunit smoke that at least asserts the allowlist file parses and the sweep script exists (full sweep stays out of `dotnet test` for speed — document that choice).
- Update docs/A11Y-CONTRAST.md from the sweep output (it becomes generated-plus-annotated rather than hand-maintained; note the generation command at the top).
- Add the "fill without paired foreground token" rule to CONTRIBUTING/copilot-instructions if not already explicit.

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green; the contrast sweep exits ZERO violations across all theme variants. Spot-check by eye in three families (one Star Wars dark, one light, high-contrast family if present) on the pages that previously had unreadable labels. Report: total violations found per theme before fixing, the fix category (1/2/3) per group, tokens changed per theme scope, and the allowlist entries with reasons.
