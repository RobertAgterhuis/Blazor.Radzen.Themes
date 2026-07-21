# Designer Persistence

This document describes server-side persistence for the LowCode designer, with offline fallback.

## Architecture

- Client contract: `IDesignStore` with envelope, version list, restore, and delete operations.
- Local store: `LocalDesignStore` persists documents and short history in browser storage.
- Remote store: `RemoteDesignStore` calls SWA managed Functions at `/api/designs`.
- Auto mode: `FallbackDesignStore` probes remote availability and falls back to local when needed.

## API contracts

### GET `/api/designs`
Returns `DesignListItem[]`:

```csharp
public sealed record DesignListItem(string Name, DateTimeOffset LastModified, int CurrentVersion);
```

### GET `/api/designs/{name}?version=N`
Returns latest or requested version as `DesignDocumentEnvelope`:

```csharp
public sealed record DesignDocumentEnvelope(
    string Name,
    int Version,
    string ETag,
    DateTimeOffset LastModified,
    DesignDocument Document);
```

### PUT `/api/designs/{name}`
Body: `DesignDocumentEnvelope` (Document is used as payload)
Headers:
- `If-Match: <etag>` for optimistic concurrency
- `X-Force-Save: true` for conflict override path

Returns updated `DesignDocumentEnvelope` with new version and ETag.
On conflict returns `409` and latest server envelope.

### DELETE `/api/designs/{name}`
Soft-deletes a design (metadata flag), keeps version blobs.
Returns `204` when deleted.

### GET `/api/designs/{name}/versions`
Returns `DesignVersionInfo[]`:

```csharp
public sealed record DesignVersionInfo(int Version, DateTimeOffset Created, long SizeBytes);
```

### POST `/api/designs/{name}/restore/{version}`
Restores historical version as a new latest version.
Returns resulting `DesignDocumentEnvelope`.

## Blob storage layout

Container: `designs`

- Version blob: `{name}/v{version}.json`
- Metadata blob: `{name}/_meta.json`

Metadata includes current version, last modified, soft-delete state, and effective ETag.
Optimistic concurrency uses `_meta.json` ETag.

## Version retention

- Server keeps at most 20 versions per design.
- On save beyond 20 versions, oldest versions are deleted (FIFO).
- Local fallback keeps at most 5 versions per design.

## SWA and configuration

- SWA route pass-through is enabled with `/api/*` route in `samples/Agterhuis.Ui.Demo/wwwroot/staticwebapp.config.json`.
- Functions use `AzureWebJobsStorage` from:
  - `api/Agterhuis.Ui.Designer.Api/local.settings.json` for local dev
  - SWA environment variables in Azure for hosted environments

## Offline fallback behavior

- In `Auto` mode, designer attempts remote access first.
- If remote API is unavailable or times out, store switches to local fallback.
- UI shows warning: `Offline modus - wijzigingen worden lokaal opgeslagen.`
- Local draft recovery remains active and compares draft recency against server version when available.

## Conflict handling

When server returns `409 Conflict`:

1. **Mijn versie opslaan**: force-save creates a new server version.
2. **Server-versie laden**: discard local unsaved state and load latest server copy.
3. **Annuleren**: keep working locally without overwrite.
