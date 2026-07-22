# Prompt 77 - Designer closure run (completeness, rapportage, release-samenvatting)

Doel: voer een finale afsluitrun uit op de designer-verbetertrajecten. Deze run doet geen nieuwe featureontwikkeling, maar valideert volledigheid, werkt documentatie af, en levert een heldere release-ready samenvatting op.

Deze prompt sluit aan op:

- `prompts/74-designer-functional-integrity-audit.md`
- `prompts/75-designer-fix-findings-from-functional-audit.md`
- `prompts/76-designer-fix-medium-low-tail.md`
- `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`

---

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; build en tests moeten groen zijn.

---

## Scope

In scope:

- Finale statuscontrole van alle audit-findings
- Completeness check van tests + rapportage
- Bijwerken van docs/changelog met wat feitelijk geleverd is
- Release-ready overzicht maken

Niet in scope:

- Nieuwe features
- Grote refactors
- Niet-noodzakelijke UX/herontwerp wijzigingen

---

## Fase 1 - Finding closure matrix (verplicht)

1. Lees `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`.
2. Bouw een closure matrix met per finding:
   - ID
   - severity
   - status (`Fixed`, `Blocked`, `Open`)
   - testreferentie(s)
   - bestand(en)
3. Controleer dat:
   - geen `Critical`/`High` finding nog `Open` staat
   - elke `Fixed` finding minimaal 1 regressietestreferentie heeft

Als bovenstaande niet klopt:

- markeer als release-blocker
- rapporteer exact welke IDs ontbreken

---

## Fase 2 - Testcompleteness check

Valideer dat de relevante testsuites de functionele gebieden afdekken:

- Designer shell interacties
- Property panel bindings/toggles
- Wrapper controls (zoals `AgtSwitch`)
- Persist/restore kernflow
- Preview/interactie mode kernflow

Voeg alleen tests toe wanneer er aantoonbaar een gat zit in closure-bewijs.

---

## Fase 3 - Documentatie afronden

### 1. Auditrapport bijwerken

Update `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md` met:

- finale closure matrix
- expliciete sectie `Release Readiness`
- lijst van geblokkeerde findings met rationale + vervolgstap

### 2. Changelog bijwerken

Update `CHANGELOG.md` (Keep a Changelog stijl) met een sectie die de designer-fixes samenvat:

- `Fixed` items (functioneel)
- testdekking/highlights
- eventuele bekende beperkingen

### 3. (Optioneel) designer readme/notities

Indien aanwezig en relevant, update `docs/designer/README.md` met:

- verwijzing naar auditrapport
- korte uitleg van de closure status

---

## Fase 4 - Verificatie (hard gate)

Voer minimaal uit:

- `dotnet build Agterhuis.Ui.sln -c Release`
- `dotnet test Agterhuis.Ui.sln -c Release --logger "console;verbosity=minimal"`

Rapporteer:

- totaal aantal tests
- pass/fail
- eventuele flaky of geblokkeerde cases

---

## Acceptatiecriteria

1. Closure matrix is aanwezig en volledig.
2. Geen open `Critical/High` findings.
3. Elke `Fixed` finding heeft testbewijs.
4. Auditrapport is bijgewerkt met `Release Readiness`.
5. `CHANGELOG.md` bevat een correcte designer-fix samenvatting.
6. Build + tests zijn groen.

---

## Verwachte eindoutput van de agent

1. Executive summary (release-ready of niet).
2. Closure matrix samenvatting (aantallen Fixed/Blocked/Open per severity).
3. Lijst van bijgewerkte documentatiebestanden.
4. Test/build resultaten.
5. Eventuele resterende blockers met concrete next step.

---

## Stopregel

Stop pas wanneer alle bovenstaande controles zijn uitgevoerd en gedocumenteerd, of wanneer een expliciete blocker dit onmogelijk maakt (met bewijs en vervolgadvies).