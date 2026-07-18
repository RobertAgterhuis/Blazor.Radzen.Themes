# Prompt 19 — Showcase-defecten: hero niet theme-gebonden, switcher-trigger, nav-states

Screenshot audit of Home in hoth-dark, tatooine-dark and dathomir-dark shows three defects. Fix root causes, not per-theme patches.

---

Copy below into Claude Code in the repo root:

---

Fix these three defects in the Agterhuis.Ui demo showcase and theme.

## 1. Home hero is hard-coded to Dagobah and renders light-on-light

Observed: in every dark theme the "Signature Experience" hero shows a near-white panel with ghost text "Dagobah", the description "Soft low-contrast layers with misty motion and quiet depth", and three empty-looking pale chips — regardless of the active theme.

Fix:
- The hero must bind to `ThemeState.ActiveTheme`: display-font title = active family display name, description = per-family tagline (add a small dictionary in the demo: Plum Ink, Ocean, Dagobah, Dathomir, Hoth, Tatooine each get a one-line Dutch tagline), chips = actual readable values (active variant id, ambient on/off, "6 themafamilies").
- The hero SURFACE must use theme tokens (surface-1/2 + the theme's atmosphere/gradient tokens) — remove whatever hard-coded light gradient or dagobah-specific class is applied now. Text on the hero uses the theme text tokens; the big display word may be low-contrast decorative (aria-hidden) but the title/description/chips must pass AA in every theme.
- Chips: outline style with theme border token + readable text (no pale pill with invisible text).
- Re-render on ThemeChanged so switching updates the hero instantly.
- bUnit test: render Home under two different themes and assert the hero shows the active family name (not "Dagobah").

## 2. Theme switcher trigger and panel styling

Observed: the closed trigger shows the family name plus "Donkere variant" stacked, clipped and overflowing a too-small box; in Hoth it renders as a light-blue light-mode box on the dark header. In the open panel the selected item (Dathomir) is a vivid red fill with WHITE text — violates the on-accent rule — and the first item is partially clipped at the top.

Fix:
- ValueTemplate: single-line layout — family name, then variant in smaller muted text after a middot, `text-overflow: ellipsis`, fixed min-width (~11rem) and proper trigger height; trigger surface/border/text use theme tokens (audit why Hoth shows a light box — likely a missing dark-scope token on the dropdown trigger in the header context).
- Panel: selected/highlighted option = accent fill with `--agt-on-accent` DARK text (grep `.rz-state-highlight`/selected option styles for the switcher and globally); hover = subtle surface-3; no white text on vivid accent fills in ANY theme.
- Fix the top clipping of the first option (panel padding/offset).
- Swatch strips per option must show the TARGET family's colors (verify — they appeared correct, keep).

## 3. Sidebar nav idle state is filled in some themes

Observed: in hoth-dark and tatooine-dark EVERY nav item has a filled background block (blue-gray resp. mustard), so idle, hover and active are indistinguishable and the sidebar looks like a wall of buttons.

Fix: idle nav items are TRANSPARENT (text + icon only) in every theme; only hover gets the subtle alpha tint and only the active item gets the filled surface + accent left edge. This is a token audit: find the nav-item idle background variable that hoth/tatooine map to a solid color and set it to transparent in ALL theme scopes; add the rule to the token-parity/contract test if expressible (idle nav bg token must be transparent).

## Verification

`dotnet build -c Release` zero warnings; `dotnet test` green including the new Home hero test. Walk Home + open the switcher in all six families (both variants where relevant): hero follows the theme and is readable, trigger fits on one line with correct dark styling, selected option has dark text on accent, first option not clipped, sidebar shows clear idle/hover/active distinction. Report per defect the root cause found and files changed.
