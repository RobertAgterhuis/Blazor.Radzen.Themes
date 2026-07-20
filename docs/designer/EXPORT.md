# LowCode designer export

Prompt 51 adds client-side project export for the designer.

## What export produces

- A zip archive with a runnable Blazor project layout.
- One `.razor` file per exported page.
- The serialized design document at `design/document.json`.
- Generated data-service contracts for the autoruitschade demo model.

## Current implementation notes

- Export runs entirely in the browser-side demo app.
- The archive is created with `System.IO.Compression.ZipArchive`.
- Page code is generated from the current design model and registry metadata.
- The browser fallback uses the File System Access API when available, otherwise a blob download.

## Known v1 limitations

- Monaco-based editing is not yet part of the export surface.
- Template assets are currently generated from code rather than loaded from a separate embedded template pack.
- The exported project is intentionally minimal and meant as a starting point.

## Verification target

The next hardening step is a CI smoke test that exports a project, unpacks the zip, and runs `dotnet build` on the result.
