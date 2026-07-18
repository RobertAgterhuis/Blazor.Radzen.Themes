# FAQ

> Summary page. Canonical source: repository docs and root README.

## Why wrappers and raw Radzen together?

Wrappers are used where policy and consistency matter. Raw Radzen remains for specialized surfaces where wrappers do not add value.

## How do themes stay consistent?

Token parity tests enforce that all families define all required tokens.

## Why did a PR fail for styling?

Token-bleed guards block hard-coded color values outside approved theme scopes.

## Why might screenshot captures fail?

The screenshot pipeline intentionally fails on near-blank images (>95% near-identical luminance) and oversize PNG outputs.

## Where is the full reference documentation?

- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/docs/README.md
