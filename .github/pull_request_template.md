## Summary

Describe what changed and why.

## Validation

- [ ] `dotnet restore --locked-mode`
- [ ] `dotnet build Agterhuis.Ui.sln -c Release` (zero warnings)
- [ ] `dotnet test Agterhuis.Ui.sln -c Release`
- [ ] Guard suites pass (token parity, token bleed, accessibility)

## Theming and Accessibility

- [ ] If color tokens changed, entries updated in docs/A11Y-CONTRAST.md
- [ ] If theme families/variants changed, docs/THEME-COVERAGE.md updated
- [ ] If token model changed, docs/TOKEN-AUDIT.md updated

## Documentation

- [ ] Public docs updated in the same PR
- [ ] README/docs links checked (no broken relative links)

## Release Impact

- [ ] CHANGELOG.md updated (if user-facing change)
- [ ] Versioning impact considered (SemVer)
