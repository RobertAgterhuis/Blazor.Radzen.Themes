# LowCode designer data model

Prompt 52 adds a domain-specific data model to the designer. The showcase domain is autoruitschade herstel.

## Model

- `DesignDataModel` holds the document-level entity catalog, default seed, and default row count.
- `DesignEntity` describes one bindable source with a name, plural name, fields, and seed settings.
- `DesignField` describes a single field with type, required flag, optional pattern, and enum values.
- `DesignFieldType` supports `String`, `Int`, `Decimal`, `Bool`, `DateTime`, and `Enum`.
- `DesignSeedSettings` controls deterministic row generation per entity.
- `DesignSeedRow` is a preview row emitted by the generator.

## Built-in entities

The default data model contains:

- Schadedossier
- Klant
- Voertuig
- Werkorder
- Factuur
- Voorraad

Each entity is seeded deterministically with Dutch-oriented demo data.

## Seed behavior

- The default seed is `42`.
- The default row count is `25` for the main entities and `30` for voorraad.
- `DesignDataModelSeeder.GeneratePreview(...)` returns the first five rows for the selected entity.
- `DesignDocument` instances start with the default data model so the designer opens with usable data immediately.

## Binding and export

- The designer exposes a data panel so the user can inspect the active entity and its seed preview.
- The exporter writes generated in-memory data-service contracts to the project zip under `Services/`.
- The generated contract is intentionally simple: record types plus a deterministic service method surface that can later be replaced with a real API-backed implementation.

## Next step

Phase 52 only covers the first binding pass. The next increment should connect the data model to the registry metadata so selected DataGrid, dropdown, list, and form parameters can point at entities and fields directly.
