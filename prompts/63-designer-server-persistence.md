# Prompt 63 — Server-side persistentie via SWA managed Functions + storage

De designer slaat ontwerpen alleen op in localStorage (`LocalDesignStore`). Dat betekent: geen cross-device toegang, geen versiegeschiedenis, geen gedeelde ontwerpen, en verlies als de browser-storage wordt gewist. Implementeer een server-side `IDesignStore` op basis van Azure Static Web Apps managed Functions + Azure Blob Storage, met versiegeschiedenis en conflict-detectie. De localStorage-implementatie blijft als offline/fallback modus.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — API-project (SWA managed Functions)

### Project opzetten
- Maak een nieuw project `api/Agterhuis.Ui.Designer.Api` als Azure Functions (isolated worker, .NET 10, in-process is deprecated).
- Configureer het als de SWA managed API: update `staticwebapp.config.json` met de `/api/*`-route naar de functions.
- Het API-project is OPTIONEEL voor de designer — als de API niet beschikbaar is (geen SWA, lokaal draaien), valt de designer terug op localStorage. Geen harde dependency.

### Endpoints

| Methode | Route | Body/Query | Retour | Toelichting |
|---|---|---|---|---|
| GET | `/api/designs` | — | `DesignListItem[]` | Lijst van opgeslagen ontwerpen (naam, laatst gewijzigd, versienummer) |
| GET | `/api/designs/{name}` | `?version=N` (optioneel) | `DesignDocumentEnvelope` | Haal een ontwerp op (laatste versie tenzij version meegegeven) |
| PUT | `/api/designs/{name}` | `DesignDocumentEnvelope` + `If-Match: etag` | `DesignDocumentEnvelope` | Sla op met optimistic concurrency. Retourneert nieuwe etag. |
| DELETE | `/api/designs/{name}` | — | 204 | Verwijder een ontwerp (soft-delete: markeer als verwijderd, behoud versies) |
| GET | `/api/designs/{name}/versions` | — | `DesignVersionInfo[]` | Lijst van versies (versienummer, datum, grootte) |
| POST | `/api/designs/{name}/restore/{version}` | — | `DesignDocumentEnvelope` | Herstel een eerdere versie als nieuwe huidige versie |

### Data-model

```csharp
public record DesignDocumentEnvelope(
    string Name,
    int Version,
    string ETag,
    DateTimeOffset LastModified,
    DesignDocument Document);

public record DesignListItem(
    string Name,
    DateTimeOffset LastModified,
    int CurrentVersion);

public record DesignVersionInfo(
    int Version,
    DateTimeOffset Created,
    long SizeBytes);
```

### Storage
- Gebruik **Azure Blob Storage** met een container `designs`.
- Blobpad: `{name}/v{version}.json` (één blob per versie).
- Metadata-blob: `{name}/_meta.json` met de huidige versienummer, etag, en soft-delete-vlag.
- Etag: gebruik de blob-etag van `_meta.json` voor optimistic concurrency.
- Versielimiet: bewaar de laatste **20 versies** per ontwerp. Bij het opslaan van versie 21: verwijder versie 1 (FIFO).
- Configuratie via `local.settings.json` / SWA environment variables: `AzureWebJobsStorage` connection string.

## Fase 2 — Client-side `IDesignStore`-implementatie

### `RemoteDesignStore`
- Implementeert `IDesignStore` met dezelfde interface als `LocalDesignStore`.
- Communicatie via `HttpClient` naar de SWA API-endpoints.
- Etag-tracking: bij laden van een document onthoudt de store de etag. Bij opslaan stuurt hij `If-Match`. Bij 409 Conflict: toon een conflict-dialoog (zie fase 3).
- Foutafhandeling: bij netwerkfouten → fallback naar localStorage met een waarschuwing "Offline modus — wijzigingen worden lokaal opgeslagen".

### Store-selectie
- Voeg een `DesignerPersistenceMode` enum toe: `Local`, `Remote`, `Auto`.
- `Auto` (standaard): probeer remote, val terug op local als de API niet bereikbaar is (health-check op `/api/designs` bij startup).
- Configureerbaar via `DesignerShell` parameter of DI-registratie.

### Breid `IDesignStore` uit
De huidige interface is minimaal. Breid uit met:

```csharp
public interface IDesignStore
{
    Task<IReadOnlyList<DesignListItem>> GetRecentAsync();
    Task<DesignDocumentEnvelope?> LoadAsync(string name);
    Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag);
    Task RemoveAsync(string name);
    Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name);
    Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version);
}
```

- `LocalDesignStore` implementeert dezelfde interface met versiegeschiedenis in localStorage (laatste 5 versies, beperkt door storage-limiet). `expectedETag` wordt genegeerd (geen concurrency lokaal).
- Update alle bestaande code die `IDesignStore` gebruikt naar de nieuwe interface.

## Fase 3 — Conflict-afhandeling en versie-UI

### Conflict-dialoog
- Bij een 409 Conflict (iemand anders — of een ander tabblad — heeft het document gewijzigd sinds het laden):
  - Toon een dialoog met drie opties:
    1. "Mijn versie opslaan" (force-save, nieuwe versie)
    2. "Server-versie laden" (verwerp lokale wijzigingen)
    3. "Annuleren" (terug naar de editor met de lokale versie)
  - De dialoog toont de laatst-gewijzigd-datum van de server-versie.

### Versiegeschiedenis-paneel
- Voeg een "Versiegeschiedenis"-knop toe aan de toolbar (of als menu-optie bij "Openen").
- Toont een lijst van versies (nummer, datum, grootte).
- Klik op een versie: preview (read-only rendering van die versie in een dialoog).
- "Herstellen"-knop: herstelt de geselecteerde versie als nieuwe huidige versie (via de restore-endpoint).
- Terugdraaien gaat via de command-stack (het laden van een versie is een nieuwe document-state).

### Autosave-verbetering
- Autosave naar de server: elke 30 seconden als er wijzigingen zijn (debounced).
- Autosave naar localStorage: elke 5 seconden (de huidige draft-logica, als offline fallback).
- Bij terugkeer naar de designer na een crash: als de localStorage-draft NIEUWER is dan de server-versie, toon de bestaande herstel-banner met de keuze om de draft of de server-versie te gebruiken.

## Fase 4 — Tests

- Unit-tests voor de API-functies: save/load/versions/restore/delete met een in-memory blob-mock.
- Unit-tests voor `RemoteDesignStore`: happy path + 409 conflict + netwerk-timeout (HttpClient mock).
- Unit-tests voor `LocalDesignStore`: verifieer dat de nieuwe interface backward-compatible is met bestaande opgeslagen data.
- Integratie-test: save via API → load via API → vergelijk documenten (roundtrip).
- bUnit: conflict-dialoog verschijnt bij een 409; versiegeschiedenis-paneel toont versies.

## Fase 5 — Documentatie

- Update `docs/designer/README.md` met de persistentie-architectuur (lokaal + remote).
- Voeg `docs/designer/PERSISTENCE.md` toe met:
  - De API-endpoints en hun contracten
  - Hoe je de Azure-resources aanmaakt (Blob Storage container, SWA configuratie)
  - Hoe je de connection string configureert
  - Conflict-afhandeling uitleg
  - Versie-retentiebeleid
  - Offline-fallback gedrag

## Verificatie

- `dotnet build -c Release` zero warnings (inclusief het nieuwe API-project)
- `dotnet test` groen
- Handmatig (lokaal): designer start in Auto-modus → API niet beschikbaar → fallback naar localStorage → waarschuwing zichtbaar
- Handmatig (met API): opslaan → versiegeschiedenis toont versie 1 → wijzigen → opslaan → versie 2 → herstellen van versie 1 → canvas toont de oude versie
- Handmatig: open hetzelfde document in twee tabbladen → wijzig in tab 1 → opslaan → wijzig in tab 2 → opslaan → conflict-dialoog verschijnt
- Rapporteer: de API-projectstructuur, de storage-schema, de fallback-logica, en de versie-retentie-implementatie
