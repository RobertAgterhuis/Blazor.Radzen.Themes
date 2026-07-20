# Releasing

[Docs Index](README.md)

## Public Release Flow

Public release automation runs from GitHub Actions on pushed tags that match v*.

1. Update CHANGELOG.md with the release section.
2. Run `npm run example:scan` and confirm `docs/EXAMPLE-SCAN.md` reports zero `LEEG` and zero `ERROR` examples.
3. Commit to main.
4. Create and push a tag, for example v1.2.3.
5. GitHub Actions workflow release.yml will:
   - restore, build, and test in Release mode,
   - pack Agterhuis.Ui using the tag version,
   - create a GitHub Release,
   - attach nupkg and snupkg artifacts,
   - publish to nuget.org only when NUGET_API_KEY exists.

## NuGet Publish Behavior

Two paths are supported:

- Secret present: if repository secret NUGET_API_KEY is configured, the package is pushed to nuget.org with --skip-duplicate.
- Secret missing: package push is skipped and the workflow logs a clear notice.

## Versioning Rules

- Tags must be SemVer with v prefix (v1.2.3).
- The workflow strips the v prefix for PackageVersion.
- Breaking changes require a major version bump.

## Azure Pipelines

azure-pipelines.yml remains available for internal Azure DevOps package publishing.
GitHub Actions is the public CI and release path for open-source distribution.

## Wiki Sync Caveat

The optional wiki sync workflow (`.github/workflows/wiki.yml`) pushes markdown pages from `eng/wiki` to the GitHub wiki repository.

- The wiki must be initialized once in the GitHub UI by clicking "Create the first page".
- Repository Actions permissions must allow `GITHUB_TOKEN` write access to repository contents.
- If the wiki repo does not exist yet (`*.wiki.git` clone fails), initialize in UI first and rerun publish.
