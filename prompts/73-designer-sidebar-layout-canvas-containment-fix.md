# Prompt 73 - Designer bugfix: AgtSidebarLayout blijft binnen canvas

Bij het toevoegen van AgtSidebarLayout in de designer lijkt de sidebar buiten het canvas te vallen (zie screenshot van de gebruiker). Dit voelt alsof de component op viewport-niveau rendert in plaats van binnen de canvas-preview.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root D:\repositories\Blazor.Radzen.Themes; .github/copilot-instructions.md bindend; bestaande infra hergebruiken; build en tests moeten groen blijven.

---

## Probleem

### Huidig gedrag
- Voeg in de designer een AgtSidebarLayout toe.
- De sidebar-presentatie lijkt visueel buiten het canvas te positioneren of over de canvas-rand heen te lopen.
- De layout gedraagt zich niet als een ingesloten component-preview.

### Verwacht gedrag
- AgtSidebarLayout blijft volledig binnen de canvas-node van de designer.
- Geen viewport-achtig gedrag (fixed/overlay-effect) binnen de designer-canvas.
- Header, sidebar en content van de layout blijven netjes geknipt binnen de componentgrenzen.

---

## Waarschijnlijke oorzaak

In runtime is AgtSidebarLayout correct, maar in de designer-preview moeten layout-componenten extra containment krijgen.

Relevante plekken:
- src/Agterhuis.Ui/Components/Layout/AgtSidebarLayout.razor
- src/Agterhuis.Ui/Components/Layout/AgtSidebarLayout.razor.css
- src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor
- src/Agterhuis.Ui.Designer/wwwroot/css/designer.css

Waarschijnlijk ontbreekt een designer-specifieke override die Radzen sidebar-positionering en overflow-gedrag in de canvas forceert naar in-flow gedrag.

---

## Oplossingsrichting

### Fase 1 - Reproduceer en isoleer
1. Open de designer pagina in de demo-app.
2. Voeg AgtSidebarLayout toe op een lege canvas.
3. Inspecteer DOM rond de node met data-agt-design-component="AgtSidebarLayout".
4. Controleer of .rz-sidebar of child wrappers position/transform/height gebruiken die buiten de node treden.

### Fase 2 - Voeg containment contract toe in designer CSS

Voeg in designer.css een gerichte, veilige override toe die alleen in de designer-canvas actief is:

```css
/* Alleen in designer-canvas: forceer layout containment */
.designer-canvas .agt-sidebar-layout {
    max-width: 100%;
    overflow: hidden;
}

.designer-canvas .agt-sidebar-layout__main {
    min-width: 0;
    overflow: hidden;
}

/* Neutraliseer mogelijk viewport-gedrag van Radzen sidebar in preview */
.designer-canvas .agt-sidebar-layout .rz-sidebar {
    height: auto;
    left: auto;
    max-width: 100%;
    position: relative;
    top: auto;
    transform: none;
    width: 100%;
}
```

Belangrijk:
- Houd deze overrides zo lokaal mogelijk (.designer-canvas prefix) zodat runtime-gedrag buiten de designer niet verandert.
- Gebruik alleen token-based styling waar kleur of spacing relevant is.

### Fase 3 - Controleer AgtSidebarLayout defaults
1. Bevestig dat AgtSidebarLayout Responsive="false" blijft.
2. Voeg alleen wijzigingen toe in AgtSidebarLayout zelf als CSS-only fix niet voldoende is.
3. Als nodig: voeg optionele parameter toe voor preview-mode, maar alleen als minimale CSS fix onvoldoende blijkt.

### Fase 4 - Testdekking uitbreiden
Voeg regressietests toe die het designer-gedrag bewaken:
- In tests/Agterhuis.Ui.Tests/DesignerPageTests.cs:
  - scenario: voeg AgtSidebarLayout toe via designer-flow;
  - assert dat markup binnen .designer-canvas een .agt-sidebar-layout bevat;
  - assert dat geen ongewenste root-level sidebar container buiten de canvas-node verschijnt.

Als bUnit de layout-positionering niet visueel kan valideren, voeg een gerichte markup-contracttest toe plus handmatige visuele checklist.

---

## Acceptatiecriteria

1. AgtSidebarLayout blijft volledig binnen de canvas in designer preview.
2. Geen element van de sidebar steekt buiten de canvas-node.
3. Drag/select gedrag van de node blijft intact.
4. Geen regressie in andere layout-componenten.
5. Runtime gedrag van AgtSidebarLayout buiten designer blijft ongewijzigd.
6. dotnet build Agterhuis.Ui.sln -c Release is groen.
7. dotnet test Agterhuis.Ui.sln -c Release is groen.

---

## Handmatige verificatie checklist

1. Start de demo en open de designer.
2. Voeg AgtSidebarLayout toe op lege canvas.
3. Controleer dat sidebar + content binnen de componentrand blijven.
4. Schakel viewport (desktop/tablet/mobile) in de designer en controleer opnieuw.
5. Voeg daarnaast een tweede layout/component toe en controleer dat deze niet door de sidebar overlap krijgt.

---

## Definition of done

- Bug is reproduceerbaar opgelost in designer.
- Fix is lokaal (designer-specifiek) en raakt runtime niet.
- Build + tests groen.
- Eventuele nieuwe test(s) toegevoegd voor regressiepreventie.