# Prompt 26 — Publieke GitHub-inrichting (README, contributing, license, releases, docs)

The solution is now on a public GitHub repository. Turn it into a professional open-source project: sparkling README, contribution guidelines, license, release automation via GitHub Actions, and docs that read well ON GitHub itself.

---

Copy below into Claude Code in the repo root (use the real GitHub repo values):

---

Prepare this repository for public GitHub consumption. Repo: `https://github.com/RobertAgterhuis/Blazor.Radzen.Themes`. All public-facing docs in English (code comments/UI blijven zoals ze zijn); keep everything accurate to the actual codebase — verify claims against the code before writing them.

## 1. README.md (the shop window)

Rewrite the root README with: hero section (project name, one-line pitch: multi-theme Blazor design system on Radzen — 7+ theme families incl. Star Wars-inspired ones, WCAG 2.2 AA, 145+ components themed), badges (build workflow, license, .NET version, Radzen version), a screenshot strip (take/collect 3–4 shots: Home showcase in two themes + the Werkorders showcase app; store under `docs/assets/`, reference with relative paths so they render on GitHub), quickstart (install package, `AddAgterhuisUi()`, CSS/JS order, anti-FOUC snippet), theme gallery table (family, palette swatch image or color chips, default variant), links to key docs (THEMING, CONSUMING, ACCESSIBILITY, THEME-COVERAGE), architecture-in-one-paragraph (token scopes, parity/bleed guard tests), and a "run the demo" section (`dotnet run --project samples/...`). Keep it scannable — no walls of text.

## 2. Community files

- `LICENSE` — MIT (consistent with Radzen.Blazor's MIT dependency).
- `THIRD-PARTY-NOTICES.md` — bundled OFL fonts (Sora, Bitter, Barlow Condensed) with their licenses, Radzen.Blazor MIT.
- `CONTRIBUTING.md` — dev setup (SDK version, build/test commands), the non-negotiable house rules distilled from `.github/copilot-instructions.md` (tokens-only colors + theme scopes, parity/bleed/a11y guard tests must pass, wrap-don't-inherit, Label/AriaLabel guard, zero-warning builds), how to add a theme (link THEMING.md), how to add a wrapper (decision guide), PR checklist, and the docs-in-same-PR rule.
- `CODE_OF_CONDUCT.md` (Contributor Covenant), `SECURITY.md` (private reporting via GitHub security advisories).
- `.github/ISSUE_TEMPLATE/` — bug report (with theme family + variant + screenshot fields), feature request, theme proposal; `.github/pull_request_template.md` with the checklist (tests green incl. guards, docs updated, contrast pairs added if colors changed).

## 3. GitHub Actions (CI + release)

- `.github/workflows/ci.yml`: on push/PR to main — setup .NET 10, `dotnet restore --locked-mode`, build Release (warnings=errors), test (all guard suites), upload test results; pack as artifact on main.
- `.github/workflows/release.yml`: on tag `v*` — build/test/pack with the tag version, create a GitHub Release with the nupkg/snupkg attached and auto-generated notes from CHANGELOG.md section; IF a `NUGET_API_KEY` secret exists, push to nuget.org (`--skip-duplicate`), otherwise skip that step with a notice. Document both paths in a `docs/RELEASING.md`.
- Keep `azure-pipelines.yml` working but add a comment noting GitHub Actions is the public CI; do not break the Azure DevOps flow.

## 4. Docs readable ON GitHub

- `docs/README.md` as index: table of every doc with one-line description (THEMING, CONSUMING, RELEASING, ACCESSIBILITY, A11Y-CONTRAST, THEME-COVERAGE, TOKEN-AUDIT, RADZEN-COMPONENT-INVENTORY, plus prompts/ as "build history").
- Sweep all docs for: broken/absolute local paths → relative links; long unstructured sections → headings + tables; add a breadcrumb link back to the docs index at the top of each doc. Mermaid diagrams are fine on GitHub — add one architecture diagram (token flow: theme scope → tokens → rz-vars → components) to THEMING.md.
- Root files GitHub surfaces automatically (README, LICENSE, CONTRIBUTING, CODE_OF_CONDUCT, SECURITY) must be at the expected paths.
- Add repo metadata suggestions in the report: description line, topics (blazor, radzen, design-system, theming, wcag, dotnet), social preview image (generate one into docs/assets).

## 5. Hygiene sweep for public visibility

Verify NO secrets/PATs/org-internal URLs remain (grep for dev.azure.com org paths — replace with placeholders or the public GitHub URL in package metadata `RepositoryUrl`); `.gitignore` covers artifacts/bin/obj/.vs; NuGet.config works for a public clone (nuget.org only, or document the optional internal feed separately).

## Verification

`dotnet build -c Release` zero warnings, `dotnet test` green. Push-ready check: render README locally/preview, click every relative link in README + docs index (no 404s), CI workflow YAML validates. Report: files created/changed, screenshots taken, and the suggested repo description + topics.
