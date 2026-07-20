# Design Kit Export

The theme CSS in this repository is the source of truth for designer handoff. The export pipeline turns theme tokens into files that Figma Variables and Tokens Studio can import.

## What gets exported

- Color tokens
- Typography scale
- Spacing
- Radius
- Shadows
- Motion durations

The export keeps light and dark variants as separate modes for each theme family.

## Naming map

- `--agt-color-primary-500` -> `color/primary/500`
- `--agt-color-surface-100` -> `color/surface/100`
- `--agt-space-4` -> `space/4`
- `--agt-radius-lg` -> `radius/lg`
- `--agt-shadow-2` -> `shadow/2`
- `--agt-motion-duration-fast` -> `motion/duration/fast`

## Import flow

1. Export the JSON from the repository token pipeline.
2. Import the `design-tokens.<family>.json` file into Figma Variables or Tokens Studio.
3. Map the light and dark modes to the corresponding theme variants.
4. Treat the repository token files as the source of truth.

## Repository command

- Run `npm run token:export` to regenerate the family exports in `eng/token-export/output`.
- The exporter uses the same token scan as the repository token audit, so the design-kit output and the audit stay aligned.

## Operating rule

- Designers consume the export.
- Changes to tokens flow through a token PR.
- The export must stay aligned with token parity checks.
