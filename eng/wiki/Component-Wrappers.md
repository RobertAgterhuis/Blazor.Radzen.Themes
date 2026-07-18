# Component Wrappers

> Summary page. Canonical source: docs/CONSUMING.md and .github/copilot-instructions.md.

## Wrapper policy

Use an Agt wrapper when:

- the component is reused across applications, and
- policy enforcement is needed (a11y contracts, defaults, intent semantics, migration insulation).

Use raw Radzen when:

- the component is niche or highly specialized, or
- a wrapper adds no policy value.

## Non-negotiables

- Wrap, do not inherit from Radzen components.
- Form wrappers must provide Label or AriaLabel.
- Keep wrapper CSS token-driven.
- Add demo coverage and tests with each new wrapper.

## Canonical Reference

- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/docs/CONSUMING.md
- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/.github/copilot-instructions.md
