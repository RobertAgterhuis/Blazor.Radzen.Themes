# Prompt 74 - Designer functional integrity audit (bindings, actions, toggles, interacties)

Doel: voer een volledige kwaliteitsanalyse uit op de LowCode designer en identificeer ALLE UI-acties die niet of deels werken (dead controls, ontbrekende bindings, callbacks die niets doen, toggles zonder effect, knoppen zonder state-verandering, editor-wijzigingen die niet persisteren, enz.).

Belangrijk: deze prompt is primair een **audit prompt**. Eerst inventariseren en bewijzen, daarna pas (in een vervolgronde) gericht fixen.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; bestaande infra en testharnas hergebruiken; build en tests moeten groen blijven.

---

## Waarom deze audit

We zien terugkerende issues in de designer waar UI-elementen zichtbaar zijn maar functioneel niets doen. Dit moet systematisch worden opgespoord over de hele designer, niet alleen per incident.

---

## Scope

Analyseer minimaal deze onderdelen:

- `src/Agterhuis.Ui.Designer/Components/` (o.a. `DesignerShell`, `PropertyPanel`, `DesignerCanvasNode`, `DesignerDataPanel`, `DesignerCodePanel`)
- `src/Agterhuis.Ui/Components/` wrappers die door de designer gebruikt worden (o.a. `AgtSwitch`, `AgtDropdown`, `AgtTextField`, `AgtNumericField`, `AgtDatePicker`)
- `src/Agterhuis.Ui.Designer/wwwroot/` interop (`designer-interop.js`, resize/command scripts)
- Designer registry/meta (`Registry/*.cs`, generated registry)
- Designer tests in `tests/Agterhuis.Ui.Tests` (bestaand + uitbreiding)

---

## Auditdoelen

### 1. Dead controls detecteren

Identificeer controls die klikken/toggles toestaan maar:

- geen callback uitvoeren,
- geen state muteren,
- state muteren maar niet renderen,
- renderen maar niet persisteren,
- of direct weer worden overschreven bij rerender.

Voorbeelden:

- switches (aan/uit) zonder zichtbaar functioneel effect;
- tab-wissels die niets tonen/verbergen;
- knoppen die geen command uitvoeren;
- dropdowns die selectie niet terugschrijven;
- inline edit actions die niet committen.

### 2. Binding chain valideren (end-to-end)

Controleer per interactie de volledige keten:

`UI event -> handler -> state update -> render update -> model update -> (optioneel) autosave/persist -> restore`

Markeer exact waar de keten breekt.

### 3. Action coverage matrix opstellen

Maak een matrix van alle belangrijke designer-acties met status:

- `OK`
- `Partieel`
- `Defect`
- `Niet geïmplementeerd`

### 4. Testgaten expliciet maken

Koppel elk defect aan:

- bestaande test die had moeten falen maar ontbreekt,
- of nieuw testtype dat nodig is (bUnit of Playwright/e2e).

---

## Methode (verplicht)

### Fase A - Statische code-audit

1. Inventariseer interactiepunten in Razor:
   - `@onclick`, `@onchange`, `ValueChanged`, `Change`, `@bind-Value`, keyboard handlers.
2. Traceer handlerimplementaties in code-behind.
3. Controleer op anti-patterns:
   - event gebonden maar handler leeg;
   - bool toggles die nooit gelezen worden;
   - local state die bij `OnParametersSet` onbedoeld reset;
   - wrapper-componenten die onderliggende control events niet doorgeven.

### Fase B - Testgedrag valideren

1. Draai bestaande relevante tests.
2. Voeg gerichte regressietests toe waar gedrag ontbreekt (klein en doelgericht).
3. Gebruik bUnit voor event->state assertions; gebruik Playwright alleen waar DOM/visueel gedrag nodig is.

### Fase C - Runtime sanity check

1. Start designer (indien mogelijk lokaal).
2. Doe handmatige smoke op kernflows:
   - component toevoegen,
   - properties wijzigen,
   - tab/switch toggles,
   - preview/interactie mode,
   - save/reload.
3. Leg afwijkingen vast met reproduceerbare stappen.

---

## Deliverables (in dezelfde run)

### 1. Auditrapport

Maak of update:

- `docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`

Structuur verplicht:

1. Samenvatting (aantallen OK/Partieel/Defect/Niet geïmplementeerd)
2. Bevindingen per severity (`Critical`, `High`, `Medium`, `Low`)
3. Per finding:
   - uniek ID (bijv. `DFI-001`)
   - component + bestand + lijnreferentie
   - reproduceerstappen
   - verwacht vs actueel gedrag
   - vermoedelijke root cause
   - voorgestelde fixrichting
   - testdekking (bestaat/ontbreekt)
4. Action coverage matrix
5. Prioriteitenlijst voor fixronde

### 2. Testuitbreiding (alleen audit-veilig)

Voeg alleen tests toe die defect gedrag aantonen zonder grote refactors.

- Nieuwe tests in `tests/Agterhuis.Ui.Tests` met duidelijke naamgeving:
  - `Designer*Functional*Tests`
  - of uitbreiding van bestaande suites (`DesignerPageTests`, `DesignerPropertyPanelTests`, wrapper-tests)

### 3. Build/test bewijs

Voer minimaal uit:

- `dotnet build Agterhuis.Ui.sln -c Release`
- `dotnet test Agterhuis.Ui.sln -c Release --logger "console;verbosity=minimal"`

Rapporteer in de uitkomst:

- aantal tests,
- geslaagd/mislukt,
- welke nieuwe tests zijn toegevoegd.

---

## Niet doen in deze prompt

- Geen brede UX-redesigns.
- Geen grote architectuurwijzigingen.
- Geen ongevraagde refactors buiten audit/finding context.
- Geen runtime-gedrag wijzigen tenzij nodig om een testbare auditfout te isoleren.

---

## Acceptatiecriteria

1. Er is een complete action coverage matrix van de designer.
2. Alle gevonden dead controls/broken bindings zijn gedocumenteerd met reproduceerbare stappen.
3. Elk defect heeft een duidelijke root-cause hypothese en fixrichting.
4. Minstens de hoogste prioriteit defecten hebben regressietests die het probleem aantonen.
5. Build + tests zijn groen na auditwijzigingen.
6. Geen regressie buiten de audit scope.

---

## Verwachte eindoutput van de agent

1. Korte executive summary.
2. Link naar auditrapport (`docs/designer/DESIGNER-FUNCTIONAL-INTEGRITY-AUDIT.md`).
3. Lijst met alle fix-kandidaten op impact.
4. Overzicht van nieuw toegevoegde tests.
5. Build/test status.

---

## Optionele vervolgstap

Na deze audit: voer een aparte fixprompt uit die de findings op volgorde oplost (begin met `Critical` en `High`) en per fix een regressietest toevoegt.
