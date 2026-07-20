# Foutafhandeling

## Wanneer gebruik je dit

Gebruik dit patroon om te bepalen welke fout op welke plek zichtbaar moet zijn.

## Anatomie

```mermaid
flowchart TD
    V[Validation] -->|field-level| F[Inline fout]
    V -->|recoverable| T[Toast]
    V -->|page-level| S[Error state]
    V -->|fatal| P[Errorpagina]
```

## Do

- Laat veldfouten naast het veld zien.
- Gebruik een toast voor een afgeronde, herstelbare actie.
- Gebruik een error state of pagina voor structurele problemen.

## Don't

- Zet technische foutcodes vooraan in de tekst.

## Live reference

- Demo: `/Error`
- Showcase: `/app/werkorders`
