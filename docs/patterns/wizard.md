# Wizard

## Wanneer gebruik je dit

Gebruik dit patroon voor flows met duidelijke stappen en beperkte terugval per stap.

## Anatomie

```mermaid
flowchart LR
    S1[Step 1] --> S2[Step 2]
    S2 --> S3[Step 3]
    S3 --> R[Review]
```

## Do

- Valideer per stap.
- Laat duidelijk zien waar de gebruiker is.
- Bewaar terug-gedrag voorspelbaar.

## Don't

- Verberg validatiefouten tot aan het einde van de flow.

## Live reference

- Demo: `/components/layout/tabs`
- Showcase: `/app/werkorders`
