# Prompt 32 — GitHub-afwerking: wiki vullen, screenshots goed, README-littekens weg

Observed on github.com/RobertAgterhuis/Blazor.Radzen.Themes: the Wiki tab is empty ("Create the first page"), the README screenshot strip shows tiny near-BLANK captures, the CI badge renders as a broken image, and the literal instruction line "Replace OWNER/REPO in badge links with your repository path." is still in the README. Fix all three areas properly.

---

Copy below into Claude Code in the repo root:

---

Repo: `https://github.com/RobertAgterhuis/Blazor.Radzen.Themes` — use this real path everywhere; no OWNER/REPO placeholders may remain anywhere in the repo (grep to verify).

## 1. Screenshots — automated, verified, and actually showing content

Build `eng/screenshots/` (Playwright script, npm-run + documented):
- Starts the demo (`dotnet run`, wait for the port), viewport 1600×900, waits for network-idle AND a page-specific selector to be visible (e.g. the hero title, the datagrid rows) plus a 500ms settle for count-up/fonts — the current blanks are captures taken before render.
- Capture set: Home in `plum-dark` and `hoth-light` (hero + metrics visible), Catalog Buttons in `imperial-dark`, Werkorders dashboard in `autotaalglas-light` (of plum-dark als die familie er nog niet is), Werkorders grid with the detail dialog open, Planning scheduler. Set the theme via localStorage before navigation so captures are deterministic.
- **Blank-guard**: after each capture, verify pixel variance (fail the script if >95% of pixels are near-identical — a blank shot must be an ERROR, not a commit).
- Output to `docs/assets/` as optimized PNG (≤300KB each, consistent size); regenerate ALL existing bad captures.
- README presentation: drop the cramped 2-column tables — full-width images stacked, each with a one-line caption; max 4 in the README, link to a `docs/GALLERY.md` with the complete set per theme family.

## 2. README-littekens

- Remove the literal "Replace OWNER/REPO..." line; fill the real path into every badge/link and verify each badge URL resolves (CI badge: correct workflow filename + branch — the workflow must exist and have run; if the badge can't work yet, use the correct URL anyway and note it turns green after the first main build).
- Verify the numbers claimed in the intro against the code (theme family count, component counts — "420 installed Radzen components" vs the inventory; correct anything inflated or stale).
- Preview-render the README (grip of GitHub-preview) and check every image + relative link.

## 3. GitHub Wiki vullen

The wiki is a SEPARATE git repo: `https://github.com/RobertAgterhuis/Blazor.Radzen.Themes.wiki.git`.
- Generate wiki content in `eng/wiki/` (tracked in the main repo as the source of truth): `Home.md` (project overview + navigation), `_Sidebar.md`, `Getting-Started.md` (install, AddAgterhuisUi, CSS/JS order, anti-FOUC), `Theming-Guide.md` (token architecture, scopes, parity — condensed from docs/THEMING.md with links back), `Theme-Gallery.md` (families table + screenshots), `Component-Wrappers.md` (Agt API + wrap-vs-raw decision guide), `Accessibility.md` (WCAG 2.2 AA approach + evidence links), `Showcase-App.md` (Werkorders tour), `Releasing.md`, `FAQ.md`. Wiki pages are SUMMARIES that link to the canonical `docs/` files — no duplicated maintenance burden; note this at the top of each page.
- Publish flow: `eng/wiki/publish.ps1` (and .sh) that clones the wiki repo, syncs `eng/wiki/*` into it, commits and pushes. NOTE: GitHub requires the wiki to be initialized once via the UI — instruct me to click "Create the first page" (any placeholder) before the first publish if the clone fails; the script overwrites it.
- Optional but preferred: a GitHub Action (`.github/workflows/wiki.yml`) that runs the sync on pushes to `main` touching `eng/wiki/**` (uses the default GITHUB_TOKEN with `contents: write`; document any permission caveat found).

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green. Run the screenshot script — all captures pass the blank-guard; preview README locally with images. Dry-run the wiki publish (clone + sync locally, show the file list). Report: captures taken (page/theme/size), README fixes, wiki pages generated, and the exact commands/steps I must run once (wiki init click + first publish).
