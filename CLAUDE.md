# CLAUDE.md

Follow the repository conventions in `.github/copilot-instructions.md` — they are the single source of truth for component naming (`Agt` prefix, wrap-don't-inherit), design tokens (`--agt-*`, purple `#680898` / gold `#f1ce05`, no hard-coded colors), Central Package Management (no versions in csproj, no wildcards, committed lock files), zero-warning builds, testing requirements (bUnit + demo page per component), and the release/versioning policy.

Common commands:

```
dotnet restore
dotnet build Agterhuis.Ui.sln -c Release
dotnet test Agterhuis.Ui.sln -c Release
dotnet pack src/Agterhuis.Ui -c Release -o artifacts/packages -p:PackageVersion=<x.y.z>
dotnet run --project samples/Agterhuis.Ui.Demo
```
