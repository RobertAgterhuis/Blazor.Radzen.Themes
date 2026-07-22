# Prompt 76 - Designer fixfase 2 (Medium/Low tail cleanup)

Doel: werk de resterende `Medium` en `Low` findings uit de designer functional integrity audit af, nadat `Critical` en `High` zijn gesloten of expliciet geblokkeerd.

Deze prompt is opvolger van:

- `prompts/74-designer-functional-integrity-audit.md`
- `prompts/75-designer-fix-findings-from-functional-audit.md`
- `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`

---

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; bestaande infra hergebruiken; build en tests moeten groen blijven.

---

## Hard precheck (verplicht)

1. Lees auditrapport:
   - `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`
2. Verifieer dat:
   - alle `Critical` findings status `Fixed` of `Blocked` hebben,
   - alle `High` findings status `Fixed` of `Blocked` hebben.

Als dit niet waar is: stop de run, rapporteer welke IDs nog open staan, en voer geen codewijzigingen uit.

---

## Scope

Pak alleen resterende `Medium` en `Low` findings aan die functioneel relevant zijn voor de designer:

- bindings die deels werken
- inconsistent stategedrag bij rerender
- toggles/knoppen met onduidelijke of partiële effecten
- ontbrekende guardrails/fallbacks in interop paden
- kleine persist-/restore-inconsistenties
- testgaten voor bekende functionele edge cases

Niet in scope:

- grote UX-redesigns
- nieuwe features buiten bestaande findings
- architecturale refactors zonder directe link met finding

---

## Uitvoeringsstrategie

Werk in batches van 3-6 findings:

1. Reproduceer finding.
2. Bevestig root cause in code.
3. Implementeer minimale fix.
4. Voeg regressietest toe.
5. Draai gerichte tests.
6. Ga door naar volgende batch.

Na elke batch:

- update auditrapport status,
- noteer resterend risico,
- run minimaal designer-gerelateerde tests.

---

## Testbeleid

Per gefixte finding minimaal 1 test:

- bUnit voor component-/bindinggedrag
- Playwright alleen waar visuele runtime-interactie nodig is

Naamconventie:

- `Designer*Finding<Id>*Tests`
- of uitbreiding bestaande suite met finding-ID in testnaam/comment

Voorbeeld:

- `DesignerShell_FindingDfi021_InteractionTogglePreservesSelectionState`

---

## Rapportage-update (verplicht)

Update:

- `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`

Per finding:

- `Open` -> `Fixed` / `Blocked`
- korte samenvatting van oplossing
- testreferentie
- eventuele follow-up opmerking

Voeg sectie toe:

- `Fixfase 2 (Medium/Low) - YYYY-MM-DD`
- gefixte IDs
- geblokkeerde IDs + reden
- resterende backlog (indien aanwezig)

---

## Verificatie (verplicht)

Minimaal uitvoeren:

- `dotnet build Agterhuis.Ui.sln -c Release`
- `dotnet test Agterhuis.Ui.sln -c Release --logger "console;verbosity=minimal"`

Aanvullend aanbevolen:

- gerichte run van designer-tests (`DesignerPageTests`, `DesignerPropertyPanelTests`, wrapper-tests)

---

## Acceptatiecriteria

1. Geen open `Medium` findings die reproduceerbaar en fixbaar waren binnen scope.
2. `Low` findings zijn gefixt of expliciet gemotiveerd doorgeschoven.
3. Iedere gefixte finding heeft regressietestdekking.
4. Auditrapport is volledig bijgewerkt.
5. Build + tests zijn groen.
6. Geen regressie buiten designer-scope.

---

## Verwachte eindoutput van de agent

1. Korte summary van gefixte `Medium/Low` finding IDs.
2. Overzicht gewijzigde bestanden per finding.
3. Overzicht nieuw toegevoegde tests.
4. Build/test resultaten.
5. Eventuele resterende backlog met rationale.

---

## Stopcriteria

Stop wanneer een van de volgende geldt:

- alle resterende `Medium` en `Low` findings zijn afgehandeld,
- of alleen geblokkeerde items overblijven met duidelijke motivatie.

In beide gevallen: lever volledige eindrapportage op met statusmatrix.
