# Prompt 20 — Header theme-bleed + switcher panel restpunten

Observed in hoth-light: the app header bar renders PLUM PURPLE (gradient) while the active theme is Hoth — the header does not follow the theme. Plus four smaller defects. Fix root causes.

---

Copy below into Claude Code in the repo root:

---

Fix the following in the Agterhuis.Ui demo shell and switcher.

## 1. Header bar ignores the active theme (the bleed)

The top bar stays plum purple in every theme. Find where the header background comes from (MainLayout css, `_layout.css` partial, or a `--agt-*` header/brand token) — it is either hard-coded or defined in `:root` instead of per theme scope. Fix per the scoping rule: the header surface/gradient/border and its text ("ICT365 · AGTERHUIS.UI", the "Thema" label, toggle icon) must come from theme-scoped tokens. In hoth-light that means the ice/white glass header with primary-700 brand text per the theme spec; verify every family (plum keeps its purple header — as a THEME value, not a global). Add the header background token to the token-parity test so a theme cannot omit it.

## 2. "Thema" label contrast

The label next to the switcher is barely visible (light text on the light block behind the trigger). Label color = theme text-secondary token; ensure the trigger's backdrop doesn't wash it out in any family/variant.

## 3. Switcher panel restpunten

- First option is still partially clipped at the top of the panel — fix panel padding/scroll offset so option 1 renders fully.
- Variant label inconsistency: three options show "Lichte variant" while the highlighted one shows "Donkere variant". All options must show the SAME label (the variant that selecting the family will keep = the current variant). Find why the highlighted row differs (stale render? separate template path?) and make it consistent.

## 4. AgtSidebarLayout demo card

- There is STILL a detached white panel floating at the right inside/outside the demo card (prompt-18 item was not fully landed): constrain the embedded RadzenSidebar within the bounded demo container (position:relative + overflow:hidden on the frame; the inner sidebar must dock inside it, both expanded and collapsed).
- The demo description still says "Sidebar layout met paarse header en goud logo slot" — outdated now themes vary; rewrite to a theme-neutral description and make the embedded example use the ACTIVE theme's tokens (it currently shows a hard-coded navy/orange header).

## Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (parity test now includes the header token). Walk all six families, both variants: header follows the theme everywhere, "Thema" label readable, switcher panel shows option 1 fully with consistent variant labels, sidebar-layout demo contained with no floating panels. Report root cause per item.
