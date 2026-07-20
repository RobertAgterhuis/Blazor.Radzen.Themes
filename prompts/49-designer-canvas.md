# Prompt 49 — LowCode designer, fase 2: canvas, palet en drag & drop

Vereist prompt 48 (model + registry + renderer). Deze fase bouwt de designer-UI: een canvas dat het documentmodel live toont binnen de MainLayout-structuur, een componentenpalet, drag & drop op grid-slots, selectie en een structuurboom — plus undo/redo. Nog GEEN property-panel (fase 3) en geen export (fase 4).

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Designer-shell

- Route `/designer` in de demo-app, eigen layout (deelt de App.razor-head — geleerde les): linkerzijde PALET (componenten uit de registry, gegroepeerd per categorie, met het bestaande filterveld-patroon), midden CANVAS, rechterzijde placeholder voor het property-panel (fase 3), onderin/links een STRUCTUURBOOM (RadzenTree van de node-hiërarchie, selectie synchroon met canvas).
- Entry "Designer" in de demo-sidebar (Getting started). De designer zelf volgt het actieve theme (eigen chrome token-based); het CANVAS rendert in een geïsoleerde container die ook een ANDER theme kan tonen (theme-keuze voor het ontwerp, onafhankelijk van de editor-chrome — attribuut op de canvas-container, popups binnen canvas-scope).

## 2. Canvas: slot-gebaseerd grid

- Canvas = de MainLayout-achtige structuur: pagina met rijen (12-kolomsgrid via RadzenRow/Column-slots uit het model). Lege slots tonen een subtiele dropzone-affordance; hover toont invoegpositie (boven/onder/binnen) met een accent-invoeglijn.
- Drag & drop: vanuit palet naar slot (toevoegen), binnen canvas verslepen (verplaatsen), inclusief droppen IN containers (Card/Tabs/Stack-slots). Implementatie: HTML5 DnD met dunne JS-interop (hergebruik het interop-patroon van theme-interop.js: één klein `designer-interop.js`), géén zware library. Alle mutaties lopen via het MODEL (commands), nooit directe DOM-manipulatie.
- Selectie: klik selecteert node (accent-outline + naamlabel), Escape deselecteert, Delete verwijdert (met undo), pijltjes navigeren door siblings; broodkruimel van de node-afstamming boven het canvas.

## 3. Command-laag + undo/redo

- Alle mutaties als commands (Add/Move/Remove/Duplicate) op het document; undo/redo-stack (Ctrl+Z/Ctrl+Y, knoppen in de toolbar); dirty-indicator. Document in-memory + autosave naar localStorage (echte persistentie in fase 6).
- Toolbar: nieuw/openen (localStorage-lijst)/opslaan, undo/redo, canvas-theme-keuze, viewport-schakelaar (mobiel 360 / tablet 768 / desktop) die de canvas-breedte zet.

## 4. Guardrails

Designer-UI token-only (bleed-audit meedraaien); drag & drop met toetsenbord-alternatief (geselecteerde node verplaatsen met Ctrl+pijltjes — a11y-eis); reduced-motion respecteren (geen sleep-animaties); bUnit: command-laag (add/move/remove/undo/redo-invarianten), palet filtert, boom volgt selectie. Playwright-smoke: item uit palet naar canvas slepen resulteert in gerenderd component.

## Verificatie

Build/test groen. Handmatig: component uit palet op canvas droppen, verslepen tussen rijen en in een Card, selecteren/verwijderen/undo'en, structuurboom synchroon, canvas wisselt van theme onafhankelijk van de editor-chrome, viewport-schakelaar werkt. Rapporteer de command-API en bekende beperkingen (bijv. welke containers v1 wel/niet ondersteunen).
