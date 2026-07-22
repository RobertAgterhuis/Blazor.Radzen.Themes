# Prompt 75 - Designer findings fixronde (op basis van functional integrity audit)

Doel: los de bevindingen uit de designer-audit gericht op, in prioriteitsvolgorde, met minimale regressierisico's en verplichte regressietests per fix.

Deze prompt is de vervolgstap op:

- `prompts/74-designer-functional-integrity-audit.md`
- `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`

---

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; bestaande infra hergebruiken; build en tests moeten groen blijven.

---

## Verplichte input voor deze run

1. Lees het auditrapport:
   - `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`
2. Extraheer alle findings met status:
   - `Critical`
   - `High`
   - `Medium`
   - `Low`
3. Maak een uitvoerbare fix-backlog voor deze run.

Als het auditrapport ontbreekt of leeg is: stop en rapporteer dit expliciet, zonder codewijzigingen.

---

## Scope

Fix alleen designer-relevante functionele defects:

- dode toggles/switches
- ontbrekende bindings (`ValueChanged`, `Change`, callbacks)
- handlers zonder effect
- state die direct reset bij rerender
- acties die niet persisteren
- interop-acties die niet worden aangeroepen of fout afvangen zonder fallback

Niet in scope (tenzij auditfinding dit expliciet vereist):

- brede UX-redesign
- theming-refactor buiten functionele bugfix
- niet-gerelateerde cleanup

---

## Prioriteitsstrategie (hard gate)

Los in deze volgorde op:

1. `Critical`
2. `High`
3. `Medium` (alleen als tijd over is)
4. `Low` (alleen met expliciete ruimte)

Regel: nooit een `Medium` fixen terwijl er nog `Critical/High` open staat.

---

## Werkwijze per finding

Voor elke finding (bijv. `DFI-00X`):

1. Reproduceer lokaal (test of minimale handmatige flow).
2. Identificeer root cause in code.
3. Implementeer minimale fix (smallest safe change).
4. Voeg regressietest toe die zonder fix faalt en met fix slaagt.
5. Koppel in commentaar/rapport:
   - finding ID
   - gewijzigde bestanden
   - testnaam

---

## Testvereisten

Per gefixte finding minimaal 1 test:

- bUnit voor event/state/binding defects
- Playwright alleen voor echte runtime/DOM-interactie die bUnit niet dekt

Testnaamconventie:

- `Designer*Finding<Id>*Tests`
- of uitbreiding van bestaande suites met duidelijke finding-verwijzing

Voorbeeld:

- `DesignerPropertyPanel_FindingDfi007_AdvancedToggleUpdatesVisibleGroups`

---

## Rapportage-update (verplicht)

Update tijdens deze fixronde het auditdocument:

- `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`

Per aangepakte finding:

- status van `Open` -> `Fixed`
- commit-/wijzigingssamenvatting
- testreferentie
- eventuele resterende risico's

Voeg ook een korte sectie toe:

- `Fixronde YYYY-MM-DD`
- lijst van gefixte IDs
- lijst van nog open IDs

---

## Verificatie (verplicht)

Voer minimaal uit:

- `dotnet build Agterhuis.Ui.sln -c Release`
- `dotnet test Agterhuis.Ui.sln -c Release --logger "console;verbosity=minimal"`

Als een test faalt:

- eerst de regression testen op eigen fix valideren
- daarna pas extra fixes toevoegen
- geen brede wijzigingen om alleen groen te krijgen

---

## Acceptatiecriteria

1. Alle `Critical` en `High` findings uit het auditrapport zijn afgehandeld of gemotiveerd geblokkeerd.
2. Elke gefixte finding heeft regressietestdekking.
3. Auditrapport is bijgewerkt met nieuwe status.
4. Build + tests zijn groen.
5. Geen regressie buiten designer-scope.

---

## Verwachte eindoutput van de agent

1. Samenvatting van gefixte finding IDs.
2. Overzicht van gewijzigde bestanden per finding.
3. Overzicht van nieuw toegevoegde tests.
4. Build/test resultaat.
5. Openstaande findings (met reden).

---

## Praktische uitvoeringstip

Werk in kleine batches (2-4 findings per batch), run tests na elke batch, en voorkom grote gecombineerde patches. Dit houdt regressierisico laag en maakt review/audit eenvoudiger.
