# Releasing

> Summary page. Canonical source: docs/RELEASING.md.

## Public release path

1. Update CHANGELOG.md.
2. Push to main.
3. Tag a version as vX.Y.Z.
4. GitHub Actions release workflow builds, tests, packs, and publishes a GitHub Release.

NuGet publish is optional and runs only if NUGET_API_KEY is configured.

## Canonical Reference

- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/docs/RELEASING.md
- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/CHANGELOG.md
