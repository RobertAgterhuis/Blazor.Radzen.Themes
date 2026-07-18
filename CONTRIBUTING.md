# Contributing to Agterhuis.Ui

Thanks for contributing. This project is a multi-theme Blazor design system on top of Radzen.

## Development Setup

Prerequisites:

- .NET SDK 10
- Node.js 20+ (only required for Playwright and accessibility scans)

Clone and restore:

```bash
dotnet restore --locked-mode
```

Build and test:

```bash
dotnet build Agterhuis.Ui.sln -c Release
dotnet test Agterhuis.Ui.sln -c Release
```

Run demo:

```bash
dotnet run --project samples/Agterhuis.Ui.Demo
```

## Non-Negotiable Rules

- Colors must be token-driven: no hard-coded colors outside theme token files.
- Color tokens must be scoped per theme variant on html[data-agt-theme="..."] only.
- New tokens must be added for every theme family variant (token parity is mandatory).
- Guard suites must stay green: token parity, token bleed, and accessibility guard tests.
- Wrap Radzen components; do not inherit from Radzen components.
- Form wrappers must receive Label or AriaLabel.
- Zero-warning builds are required. Warnings are treated as errors in CI.

## Add a Theme

Follow the full process in docs/THEMING.md.

Checklist summary:

1. Add token values for light and dark variants.
2. Register the family in theme options.
3. Ensure parity and token audit tests pass.
4. Update docs/THEME-COVERAGE.md and docs/A11Y-CONTRAST.md.

## Add a Wrapper Component

Decision guide:

- Create an Agt wrapper when the component is used in multiple apps and needs policy enforcement (accessibility contracts, safe defaults, intent semantics, migration insulation).
- Use raw Radzen when the component is niche, highly specialized, or mostly passthrough.

Wrapper requirements:

1. Prefix with Agt.
2. Provide a demo page in samples/Agterhuis.Ui.Demo.
3. Add at least two bUnit tests.
4. Keep CSS token-driven.

## Pull Request Checklist

- [ ] `dotnet restore --locked-mode` succeeds.
- [ ] `dotnet build Agterhuis.Ui.sln -c Release` succeeds with zero warnings.
- [ ] `dotnet test Agterhuis.Ui.sln -c Release` succeeds.
- [ ] Token parity / token bleed / accessibility guard tests pass.
- [ ] Docs are updated in the same PR when behavior, theming, or coverage changes.
- [ ] If colors changed: contrast entries were updated in docs/A11Y-CONTRAST.md.
- [ ] If Radzen version changed: inventory and coverage docs were regenerated.

## Documentation Rule

If you change behavior, theme tokens, public APIs, or component coverage, update the relevant docs in the same PR.
