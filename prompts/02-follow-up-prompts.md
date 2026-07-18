# Follow-up prompts — run in order, one per session, after Prompt 1

Each block is a standalone prompt. Verify the build is green before moving to the next.

---

## Prompt 2 — Theme polish + dark mode parity

```
In this repo (Agterhuis.Ui, a Blazor RCL design system), refine the theme in
src/Agterhuis.Ui/wwwroot/css/agt-theme.css:

1. Complete the dark theme ([data-agt-theme="dark"]) so every Radzen surface reads
   correctly on --agt-color-primary-950: dialogs, dropdown panels, datagrid rows/stripes,
   tooltips, notifications, inputs (background/border/placeholder), disabled states.
2. Ensure WCAG AA contrast: white text on primary-500 buttons, gray-900 on gold accent
   surfaces (gold #f1ce05 fails with white text — always use dark text on gold).
3. Add hover/active/focus states for primary (purple) and secondary (gold) buttons using
   the token scale (hover = one step darker, active = two steps).
4. Add an AgtThemeToggle component that persists choice via Radzen ThemeService or a
   simple JS-free cascading parameter, and demo it in the sample app.
5. Extend the demo app with a /components/theme page showing all tokens as swatches.
Verify: dotnet build -c Release (zero warnings) and dotnet test green.
```

## Prompt 3 — Component set expansion

```
Extend the Agterhuis.Ui RCL with these wrapper components, following the existing
AgtPrimaryButton conventions (Agt prefix, wrapper-not-inheritance, CSS isolation,
tokens only — no hard-coded colors):

- Forms: AgtTextField, AgtNumericField, AgtDropdown<TValue>, AgtDatePicker,
  AgtFormActions (right-aligned save/cancel row)
- Data: AgtDataGrid<TItem> with sensible defaults (paging 20, sorting, empty-state slot)
- Feedback: AgtEmptyState (icon + title + description + action slot), AgtLoadingPanel,
  AgtConfirmDialog service wrapper around Radzen DialogService
- Layout: AgtSidebarLayout (Radzen sidebar + header using brand purple header bar with
  gold logo slot, like blog.agterhuis.net)

For each: demo page in samples app + at least 2 bUnit tests. Expose Radzen types for
low-level params (ButtonSize etc.) but use own enums for intent (AgtIntent:
Primary/Secondary/Danger). Verify build+tests green.
```

## Prompt 4 — Azure DevOps wiring (repo bestaat: ICT365.NuGet.UI.Theme)

```
The Azure DevOps repo exists. Wire this local repo to it and fill in the real values:
- Organization: ragterhuis
- Project: ICT365.NuGet
- Repo: ICT365.NuGet.UI.Theme
- Repo URL: https://dev.azure.com/ragterhuis/ICT365.NuGet/_git/ICT365.NuGet.UI.Theme
- Feed name: ict365-nuget    (project-scoped; if a feed with a different name already
  exists in the project, ask me before proceeding)
- PackageId stays Agterhuis.Ui (repo name and package name intentionally differ).

Tasks:
1. Git wiring (repo is currently local-only):
   - Verify a clean working tree; commit pending changes with a sensible message.
   - Ensure the branch is named main (git branch -M main).
   - git remote add origin https://dev.azure.com/ragterhuis/ICT365.NuGet/_git/ICT365.NuGet.UI.Theme
     (if origin exists, update it with set-url).
   - Verify .gitignore covers artifacts/, bin/, obj/, .vs/ BEFORE the first push.
   - Push: git push -u origin main. Authentication is interactive on my machine
     (Git Credential Manager) — if the push cannot authenticate from your shell,
     give me the exact commands to run myself. Never store a PAT in the repo or config.
2. Replace all ORGANIZATION/PROJECT/feed placeholders in NuGet.config,
   azure-pipelines.yml (publishVstsFeed: ICT365.NuGet/ict365-nuget) and the csproj
   RepositoryUrl (use the repo URL above).
3. Regenerate lock files (dotnet restore --force-evaluate) and verify --locked-mode works.
4. Add docs/CONSUMING.md: how a consumer app adds the feed (NuGet.config with
   packageSourceMapping: Agterhuis.* -> ict365-nuget feed; Microsoft.*/System.*/Radzen.*
   -> nuget.org), installs Agterhuis.Ui, registers AddAgterhuisUi(), and the exact
   <head> CSS/JS order (Radzen material-base.css → agt-theme.css → agt-utilities.css →
   app css; Radzen.Blazor.min.js).
5. List the manual steps I must do in the Azure DevOps UI:
   - Artifacts → create feed ict365-nuget (project-scoped) if it does not exist yet;
   - grant "ICT365.NuGet Build Service (ragterhuis)" the Feed Publisher role;
   - branch policies on main: min 1 reviewer, build validation with this pipeline,
     squash-only, block direct pushes;
   - Pipelines → New pipeline → Azure Repos Git → ICT365.NuGet.UI.Theme →
     existing azure-pipelines.yml.
Do not store any PAT or secret anywhere in the repo.
```

## Prompt 5 — Local pack + consumer smoke test

```
Validate the Agterhuis.Ui package end-to-end locally:
1. dotnet pack src/Agterhuis.Ui -c Release -o artifacts/packages -p:PackageVersion=0.1.0-local
2. Set up a local folder feed (artifacts/local-feed), push the nupkg there.
3. Create a throwaway Blazor Web App (Server interactivity) under /tmp or a scratch dir
   (NOT committed), install Agterhuis.Ui 0.1.0-local from the local feed, wire Program.cs
   and App.razor per docs/CONSUMING.md, and confirm:
   - restore/build succeeds,
   - _content/Agterhuis.Ui/css assets resolve,
   - Radzen.Blazor came in transitively,
   - an AgtPrimaryButton + AgtPageHeader page compiles.
4. Report findings; fix any packaging issue found (e.g. missing staticwebassets).
```

## Prompt 6 — Release hygiene

```
Prepare Agterhuis.Ui for its first stable release:
1. Fill CHANGELOG.md (Keep a Changelog format) for 1.0.0.
2. Confirm the pipeline versioning: tag v1.0.0 must produce package 1.0.0; document the
   release flow in README (checkout main → tag → push tag → pipeline publishes).
3. Add a breaking-change policy section to README: renames of components/parameters/CSS
   vars/token names, Radzen major bumps, and TFM changes are MAJOR.
4. Double-check no Radzen premium theme assets are included anywhere (we only use the
   free material-base.css from the Radzen.Blazor package at runtime).
5. Suggest whether GitVersion/MinVer would improve on the current script-based versioning,
   with a concrete diff if you recommend switching.
```
