# LowCode Designer

The designer is a model-first editor in the demo app. It edits `DesignDocument` JSON, renders through the designer registry, and exports runnable projects client-side.

## Phases

1. Model, registry, and renderer foundation.
2. Canvas, palette, drag and drop, and selection.
3. Property panel, validation, and layout editing.
4. Code generation, Monaco preview, and export.
5. Demo data and binding.
6. Persistence, templates, and polish.

## Architecture

- `DesignDocument` is the source of truth.
- `DesignRenderer` and `DesignerNodeHost` render the model live.
- The generated registry powers palette, validation, and rendering.
- Export stays client-side and includes `design/document.json`.

## v1 limitations

- No Razor import.
- No event logic authoring.
- Single-select editing.
- Read-only code preview.
- Hybrid persistence: local plus optional server store with conflict detection.

## Roadmap

- Two-way Monaco editing.
- Multi-select.
- Logic hooks.
- Deeper shared-storage administration and collaboration features.

## Persistence (Prompt 63)

- `IDesignStore` now supports envelopes, versions, restore, and optimistic concurrency.
- `LocalDesignStore` remains available for offline and fallback behavior.
- `RemoteDesignStore` targets `/api/designs` endpoints hosted by SWA managed Functions.
- `FallbackDesignStore` auto-switches to local mode when the API is unavailable.
- Conflict flow uses ETag checks and a user choice dialog (save mine, load server, cancel).

## Data model

See [DATA.md](DATA.md) for the phase 52 data model, seed behavior, and generated service contract.
