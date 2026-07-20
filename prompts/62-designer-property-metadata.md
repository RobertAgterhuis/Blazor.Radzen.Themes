# Prompt 62 — Property panel metadata-verrijking

Het property panel toont raw C#-parameternamen als labels ("ButtonStyle", "TextProperty", "AllowFiltering"). Dat is werkbaar voor ervaren Blazor-developers maar onnodig cryptisch. Verrijk de registry-metadata met gebruikersvriendelijke labels, beschrijvingen en categorieën, en verbeter de property panel UX met progressive disclosure.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Metadata-uitbreiding in de registry

Breid `ComponentParameterDescriptor` uit met:

| Property | Type | Toelichting |
|---|---|---|
| `DisplayLabel` | string | Gebruikersvriendelijk label, bijv. "Knopstijl" voor `ButtonStyle`. Nederlandstalig. |
| `Description` | string | Korte beschrijving van wat de parameter doet, bijv. "Bepaalt de visuele stijl van de knop (primair, secundair, etc.)." |
| `ExampleValue` | string? | Voorbeeldwaarde die als placeholder in het invoerveld getoond kan worden. |
| `IsAdvanced` | bool | `true` = standaard verborgen in de "Geavanceerd"-sectie. |
| `DisplayOrder` | int | Sorteervolgorde binnen de categorie (lagere waarden eerst). |

### Vulling
- De build-time registry-generator moet deze metadata vullen. Bron: de XML-summaries die al in de gegenereerde code gebakken zijn (voor `Description`), plus een handmatig onderhouden mapping-bestand voor `DisplayLabel`, `IsAdvanced` en `DisplayOrder`.
- Maak een `designer-metadata.json` (of `.cs` constants-bestand) in de Designer-assembly met de mapping per component+parameter. Dit bestand is het enige dat handmatig onderhouden wordt — de rest komt uit de generator.
- Fallback: als er geen `DisplayLabel` is, gebruik de parameternaam met spaties ingevoegd voor hoofdletters (PascalCase → "Pascal Case"). Nooit een leeg label.

### Scope
- Vul metadata voor ALLE Agt-wrapper parameters (die zijn de primaire doelgroep).
- Vul metadata voor de gecureerde Radzen-set (Row, Column, Card, DataGrid, Tabs, Accordion, Stack) — de meest gebruikte parameters.
- Overige Radzen-componenten: fallback naar auto-generated labels.

## Fase 2 — Property panel UX-verbetering

### Progressive disclosure
- Parameters zijn verdeeld in secties:
  1. **Toegankelijkheid** (altijd bovenaan, altijd open): Label, AriaLabel, etc.
  2. **Algemeen**: de meest gebruikte parameters (Title, Text, Placeholder, Icon, Disabled, Visible, etc.)
  3. **Data**: bindbare parameters (Data, Value, TextProperty, ValueProperty, etc.)
  4. **Layout**: LayoutSlot, Size, Style-gerelateerde parameters
  5. **Geavanceerd** (standaard ingeklapt): alle parameters met `IsAdvanced=true`
- Secties zijn inklapbaar. Ingeklapte staat wordt NIET persistent (reset bij wisseling van node-selectie).
- Lege secties (geen parameters in die categorie) worden niet getoond.

### Labels en hulp
- Het property panel toont `DisplayLabel` als veldlabel (niet de parameternaam).
- De originele parameternaam is zichtbaar als tooltip op het label (voor developers die de C#-naam nodig hebben).
- `Description` verschijnt als een info-icoon (ℹ) naast het label. Klik/hover toont de beschrijving als tooltip.
- `ExampleValue` verschijnt als placeholder in tekstvelden.

### Zoeken
- Voeg een zoekveld toe bovenaan het property panel.
- Zoeken filtert op `DisplayLabel`, parameternaam, en `Description`.
- Bij een actief zoekfilter: alle secties zijn open en alleen matching parameters zijn zichtbaar.

### Afwijkende waarden
- Parameters met een waarde die afwijkt van de registry-default krijgen een visuele indicator: een accent-stip (●) links van het label in `--agt-primary`.
- De "reset naar default"-knop is alleen zichtbaar bij afwijkende waarden.

## Fase 3 — Tests

- bUnit: verifieer dat `DisplayLabel` wordt getoond in plaats van de parameternaam.
- bUnit: verifieer dat de "Geavanceerd"-sectie standaard ingeklapt is en na klik uitklapt.
- bUnit: verifieer dat zoeken filtert op labels en beschrijvingen.
- Registry-test: verifieer dat alle Agt-wrapper parameters een niet-lege `DisplayLabel` hebben.
- Registry-test: verifieer dat de `DisplayOrder` geen duplicaten bevat binnen dezelfde component+categorie.

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- Handmatig: selecteer een AgtTextField op het canvas → property panel toont "Label" als friendly label (niet "Label" — dat is toevallig al goed), "Plaatshouder" voor "Placeholder", "Verplicht" voor "Required", etc.
- Handmatig: klik op het ℹ-icoon naast een label → beschrijving verschijnt
- Handmatig: klik op "Geavanceerd" → extra parameters verschijnen
- Handmatig: typ in het zoekveld → parameters filteren live
- Handmatig: wijzig een parameter → accent-stip verschijnt → reset-knop zichtbaar → klik reset → stip verdwijnt
- Rapporteer: het aantal verrijkte parameters (per component), het percentage met handmatige vs. auto-generated labels, en de totale metadata-bestandsgrootte
