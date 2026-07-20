# LowCode designer model

Prompt 48 phase 1 introduces a model-first foundation for the LowCode designer in `Agterhuis.Ui.Designer`.

## Schema

```text
DesignDocument
  Name: string
  Version: string
  SchemaVersion: int
  Pages: DesignPage[]

DesignPage
  Route: string
  Title: string
  Nodes: DesignNode[]

DesignNode
  Id: string
  ComponentType: string
  Parameters: Dictionary<string, DesignParameterValue>
  Children: Dictionary<string, DesignNode[]>
  LayoutSlot: DesignLayoutSlot?

DesignParameterValue
  Literal: JsonNode?
  Expression: string?

DesignLayoutSlot
  Row: int
  Column: int
  RowSpan: int
  ColumnSpan: int
```

## Runtime behavior

- Serialization uses `System.Text.Json` with `SchemaVersion` and a migration hook (`DesignDocumentMigrator`) that normalizes collections and fills deterministic node ids.
- Validation is non-throwing and reports model errors with stable paths such as `Pages[0]/Nodes[1]/Parameters/Label`.
- The component registry is generated at build time by `eng/designer-registry-generator`, which emits `src/Agterhuis.Ui.Designer/Registry/DesignerComponentRegistry.g.cs` from the current wrapper and Radzen assemblies plus `docs/RADZEN-COMPONENT-INVENTORY.md`. The generated registry is the trimmer root for WASM because it references every component type explicitly and bakes XML summaries into literals.
- Palette visibility is enabled for all wrappers plus the curated raw set `Row`, `Column`, `Stack`, `Card`, `Tabs`, `Accordion`, and `DataGrid`.
- The renderer resolves component types through the registry, binds JSON literals to real parameter types, parks event callbacks as no-op handlers, and isolates per-node failures behind node-level error frames.

## Registry counts

The generated registry currently exposes 358 component descriptors and groups them by category as follows:

- Data & Scheduling: 44
- Data Visualization: 79
- Feedback & Overlays: 30
- Forms & Inputs: 55
- Layout & Display: 79
- Misc: 131
- Navigation & Actions: 35