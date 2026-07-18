# Theming Guide

> Summary page. Canonical source: docs/THEMING.md.

## Core model

- Theme colors are scoped by html[data-agt-theme="..."] variant selectors.
- Structural tokens live in :root (spacing, radius, typography, motion, z-index).
- Radzen variables map from agt tokens in shared theme partials.
- New color tokens must be added across all theme families to keep parity.

## Guardrails

- Token parity tests enforce full family coverage.
- Token bleed tests block color literals outside theme scopes.
- Accessibility guard tests keep WCAG 2.2 AA constraints visible.

## Canonical Reference

- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/docs/THEMING.md
- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/docs/TOKEN-AUDIT.md
- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/docs/THEME-COVERAGE.md
