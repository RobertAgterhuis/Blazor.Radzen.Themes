# Prompt 21 — Volledige token-bleed audit + structurele fix

Suspicion: theme colors leak from multiple places (header was one symptom; there will be more). This prompt builds a MECHANICAL detector for bleed, fixes everything it finds, and wires the detector into the test suite so bleed can never silently return. No hand-picked fixes without the detector proving the class of problem is gone.

---

Copy below into Claude Code in the repo root:

---

Eliminate all theme/token bleed in Agterhuis.Ui. "Bleed" = any color that does not flow from a theme-scoped token: hard-coded colors, colors defined outside `[data-agt-theme="..."]` scopes, legacy selectors that override theme scopes, or components computing colors in C#. Build detection first, then fix, then lock it in.

## 1. Build the detector (script + tests)

Create `eng/token-audit/` with a script (Node or C# — your choice, runnable via `dotnet test` integration) that produces `docs/TOKEN-AUDIT.md` and backs three xunit tests:

a) **Scope audit**: parse ALL css shipped by the RCL and demo (theme files, partials, `.razor.css`, `agt-utilities.css`, Home/Theme page css). Every custom property whose value contains a color (hex, rgb/rgba, hsl, oklch, named colors except transparent/currentColor/inherit) must be DEFINED only inside a `[data-agt-theme="..."]` scope. Violations: any color-bearing `--agt-*`/`--rz-*` defined in `:root`, `html`, `body`, or bare class scope. Structural tokens (spacing, radius, fonts, durations, z-index) are exempt.

b) **Literal audit**: find every raw color literal OUTSIDE `agt-tokens.css` + theme files: in css partials, `.razor.css` isolation files, inline `style="..."` attributes in `.razor`, string literals in `.cs`/`.razor` code (`"#`, `rgb(`, chart series arrays, hero gradients). Allowlist file for justified exceptions (e.g. `transparent`, svg `fill="none"`) — each entry needs a one-line reason.

c) **Parity audit**: every theme scope defines exactly the same set of custom properties (the existing parity test, extended with any token added during this work — header, hero, nav idle, chips, chart series, glow, glass, scrollbars, on-accent, focus).

## 2. Runtime bleed probe (the strongest signal)

Add a Playwright script (`eng/token-audit/runtime-probe`) that starts the demo, walks a fixed element list — header bar, "Thema" label, sidebar container + idle/active nav item, page title, card surface + border, primary/secondary/danger buttons, input + its border, dropdown trigger, open dropdown panel, dialog surface, datagrid header/row, pager active, chart svg series stroke, notification — and records `getComputedStyle` colors per element under ALL six families (dark variants + light spot-checks). Analysis: any element whose computed color is IDENTICAL across all families is a bleed suspect (it isn't reading theme tokens). Output the matrix in `docs/TOKEN-AUDIT.md`. (Exempt truly theme-invariant values via the allowlist.)

## 3. Fix everything found

Work through the three static audits + the runtime matrix until all are clean:
- Move misscoped color tokens into every theme scope (parity test enforces completeness).
- Replace literals with token references; new tokens where no suitable one exists (then parity-add them).
- Kill legacy overrides: `[data-agt-theme="dark"]`-era selectors, `!important` color rules, inline styles in demo pages, C#-computed colors (chart palettes must come from tokens read at render, not hard-coded arrays).
- Known suspects to verify explicitly: header/topbar, hero panel, ambient layers, switcher trigger + panel, sidebar idle/hover/active, chips, badges, scrollbars, focus rings, chart series + gridlines, loading shimmer, validation messages, dialogs/popups (portaled!), toast progress bar.

## 4. Lock it in

- The three static audits run as xunit facts in the normal test suite (fail with the violation list).
- The Playwright probe gets a `npm`/script entry + README section; document how to run it before a release (CI-optional for now).
- Add a rule to `.github/copilot-instructions.md`: new colors ONLY as theme-scoped tokens; the audit tests enforce this.

## 5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green — including the three new audit facts with ZERO unallowlisted violations. Run the runtime probe: no identical-across-themes suspects left except allowlisted ones. Include the final `docs/TOKEN-AUDIT.md` summary (counts per category: found → fixed → allowlisted with reasons) in your report.
