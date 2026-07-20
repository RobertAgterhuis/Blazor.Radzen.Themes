# Prompt 50 — LowCode designer, fase 3: property-panel (reflectie-gedreven)

Vereist prompts 48–49. Deze fase vult het rechterpaneel: eigenschappen van de geselecteerde node bewerken, volledig gegenereerd uit de registry-metadata — geen handgeschreven formulier per component.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Editor-mapping per parametertype

Eén `PropertyPanel`-component dat uit de registry-metadata het juiste veld kiest (gebruik de eigen Agt-wrappers — de designer is dogfood):
- `string` → AgtTextField; `int/decimal/double` → AgtNumericField; `bool` → AgtSwitch; `enum` → AgtDropdown met de enum-waarden; `DateTime` → AgtDatePicker; kleuren-strings → tokenbewuste picker (dropdown met de theme-tokens als benoemde opties + vrij veld, met de waarschuwing dat vrije hexwaarden de bleed-regels schenden bij export); `EventCallback` → read-only vermelding "logica in geëxporteerde code" (bewuste v1-scope); onbekende typen → read-only JSON-veld.
- Parameters gegroepeerd (Algemeen / Layout / Data / Toegankelijkheid / Overig — categorie in registry-metadata), verplichte a11y-parameters (Label/AriaLabel) bovenaan met een niet-wegklikbare waarschuwing zolang leeg (de bestaande guard als UI).
- LayoutSlot-bewerking: rij/kolom/span als compacte steppers met live-preview op het canvas.

## 2. Gedrag

- Wijziging = command (dus undo-baar), canvas rendert direct mee (model-notificatie, geen volledige re-render van de hele pagina — alleen de node).
- Multi-select v1: niet — één node; het paneel toont bij geen selectie de PAGINA-eigenschappen (route, titel, canvas-theme).
- Reset-per-parameter naar default (registry kent de default); afwijkende waarden visueel gemarkeerd (zoals DevTools).
- Validatie inline: verkeerd type, ongeldige enum, lege verplichte waarde → veldfout, model bewaart de laatste geldige waarde.

## 3. Kwaliteit

Panel volledig token-based en density-aware; toetsenbord volledig (het paneel is een formulier — de eigen patronen gelden); contrast-sweep over het panel. bUnit: juiste editor per type (parametrized test over de mapping), wijziging produceert command + undo herstelt, a11y-waarschuwing verschijnt/verdwijnt, reset naar default. Registry-metadata-test uit 48 uitbreiden: elke parameter heeft een editor-mapping of een bewuste read-only markering (geen stille gaten).

## Verificatie

Build/test groen. Handmatig: AgtTextField-node selecteren → Label zetten → canvas toont het direct; enum-parameter wisselen op een RadzenButton (ButtonStyle); LayoutSlot-span aanpassen met live grid-preview; undo herstelt alles; leeg Label toont de a11y-waarschuwing. Rapporteer de editor-mappingtabel en welke parametertypen bewust read-only zijn.
