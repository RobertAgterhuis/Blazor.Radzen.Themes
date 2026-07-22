# Blunt UX Audit — Blazor.Radzen.Themes Designer

**Datum:** juli 2026  
**Scope:** volledige designer UX, vergeleken met industrie-standaarden  
**Toon:** ongezouten eerlijk, zoals gevraagd

---

## Executive Summary

De designer is technisch indrukwekkend qua architectuur (model-first JSON, command-stack, undo/redo, export naar werkend project). Maar als product voor een niet-technische gebruiker **valt hij door de mand**. Het voelt als een developer-tool die pretendeert een visuele editor te zijn. Hieronder een eerlijke vergelijking met wat de markt biedt.

---

## Vergelijking met industriestandaarden

### Wat de concurrentie doet

| Aspect | Retool / Budibase / Tooljet | Webflow / Framer | Huidige designer |
|--------|----------------------------|------------------|-----------------|
| **Canvas look** | Je ziet de app zoals de eindgebruiker die ziet. Geen wireframe-borders, geen technische labels. | Pixel-perfect WYSIWYG met live data. | Wireframe met dashed borders, technische slotnamen (LOGO, HEADERACTIONS, CHILDCONTENT), "Leeg slot" knoppen. Lijkt op een XML-structuur-editor. |
| **Component toevoegen** | Drag naar canvas, component verschijnt direct met dummy-data en correcte styling. | Idem, plus inline editing van tekst. | Component verschijnt als lege wireframe-box. Geen data, geen visuele hint van wat het doet. |
| **Templates** | 20-50+ templates, elk volledig gestyled met echte data, meerdere pagina's, navigatie. | Honderden templates, elk een volledige website. | 6 templates, elk 2-3 lege form fields en een "Leeg slot" knop. Dashboard-template toont letterlijk "Toon hier KPI's of grafieken." als tekst. |
| **Seeded data** | Altijd aanwezig. Tabel toont rijen, grafiek toont lijnen, form toont ingevulde velden. | Live CMS-data of placeholder-content dat er echt uitziet. | Seed data bestaat in het model maar wordt NIET getoond op de canvas of in preview. Components zijn leeg. |
| **Theme switching** | Instant, alle componenten reageren. Dark/light mode toggle altijd zichtbaar. | CSS variables, instant. | `data-agt-theme` attribuut op canvas div, theme dropdown in Instellingen submenu (verborgen). Werkt technisch maar is verstopt. |
| **Multi-screen** | Tabbladen met routing, navigatie-component dat automatisch linkt. | Pages panel, link-componenten. | Page tabs bestaan, maar templates zijn single-page. Geen navigatie-component, geen inter-page linking. |
| **Export** | Werkend project met data, auth, API-endpoints. | Publiceer direct, of export als clean code. | Export genereert project ZONDER seeded data in de UI. `DesignDataService` genereert `random.Next()` waarden — geen realistische preview-data. |
| **Sidebar/Layout** | Sidebar rendert gewoon binnen de canvas. | N.v.t. (geen fixed positioning issues). | `RadzenSidebar` gebruikt `position: fixed` → vliegt uit de canvas. Bug geïdentificeerd in prompt 73, maar dit is symptomatisch voor een dieper probleem: layout-componenten zijn niet getest in designer-context. |

### Wat specifiek mis is

**1. De canvas is geen WYSIWYG — het is een structuur-editor**

De resting state van `designer-canvas-node` toont:
- Transparante border (goed, verbeterd t.o.v. eerder)
- Maar: technische labels ("AgtTextField", "RadzenRow"), slot-namen ("CHILDCONTENT", "LOGO"), en "Leeg slot" dropzones die de hele layout domineren

Een niet-technische gebruiker ziet dit en denkt: "Dit is een code-tool, niet voor mij."

**Vergelijk met Retool:** de canvas toont een TextInput met een label, een placeholder, en styling. Je ZIET een formulierveld. In onze designer zie je een dashed box met "AgtTextField" erboven.

**2. Templates zijn beschamend mager**

6 templates, allemaal gebouwd met dezelfde 4 helper-methods (`CreateTextField`, `CreateNumericField`, `CreateSwitch`, `CreateFormActions`). Het Dashboard-template — het visitekaartje van elke designer — toont letterlijk een `AgtEmptyState` met tekst "Toon hier KPI's of grafieken."

Budibase biedt een CRM-template met sidebar navigatie, datatabel met 25+ rijen, detail-panel, status-badges, avatar-afbeeldingen. Dat is wat een gebruiker verwacht als startpunt.

**3. Seeded data bestaat maar wordt nergens getoond**

`DesignDataModelSeeder` is uitstekend — 6 entities, 80+ velden, realistische Nederlandse data (kentekens, dossiernummers, verzekeraars). Maar:
- Canvas toont lege componenten
- Preview mode toont lege componenten
- DataGrid/DataList componenten tonen niets zonder handmatige binding
- De preview is functioneel identiek aan de canvas minus de selection-chrome

**4. Export mist de seeded-data switch**

`ProjectExporter` genereert een `DesignDataService` die `random.Next()` gebruikt voor strings — resultaat: "string.Empty" voor de meeste tekstvelden. De rijke, domein-specifieke data uit `DesignDataModelSeeder` (kentekens als "AB-123-C", klantnamen, factuurdetails) wordt NIET meegeëxporteerd. Er is geen toggle om seeded data in/uit te schakelen.

**5. Theme switching werkt maar is verstopt**

Theme dropdown zit in Instellingen → submenu. Geen live preview van het thema-effect. Geen theme-thumbnail. Geen dark/light toggle op de toolbar. De gebruiker moet weten dat het bestaat.

**6. Multi-screen is rudimentair**

Page tabs bestaan en werken. Maar:
- Templates zijn single-page
- Geen navigatie-component (`AgtNavLink` of vergelijkbaar)
- Geen inter-page linking in het model
- Geen sidebar-layout template dat meerdere pagina's koppelt
- Geen "Add page from template" — alleen lege pagina's toevoegen

**7. Layout-componenten zijn niet designer-proof**

AgtSidebarLayout is het duidelijkste voorbeeld, maar het probleem is breder:
- RadzenSidebar gebruikt `position: fixed` → ontsnapt uit canvas
- RadzenDialog/Modal zou hetzelfde probleem hebben
- Geen `contain: layout` of vergelijkbare CSS-containment op canvas-nodes
- Layout-componenten zijn geregistreerd maar nooit getest in designer-context

---

## Wat WEL goed is (credit where due)

- **Architectuur is solide**: model-first JSON, command-stack, serialize→deserialize→apply is een correcte aanpak
- **Undo/redo werkt** met toasts en keyboard shortcuts
- **Command palette** (Ctrl+Shift+P) is een power-user feature die de concurrentie vaak mist
- **Panel consolidatie** (prompt 72) is een verbetering — 3 panels i.p.v. 5
- **Palette als visual grid** met iconen werkt
- **Keyboard shortcuts** (Ctrl+C/V/Z, Delete, arrows) zijn geïmplementeerd
- **Export naar werkend .NET project** is uniek — geen enkele concurrent doet dit voor Blazor
- **Data model editor** met entities, velden, seed-instellingen is een sterke basis
- **Breadcrumb navigatie** werkt
- **Drag & drop** met insertion lines werkt

---

## Conclusie

De designer is een **technisch sterke basis** die visueel en functioneel **niet de belofte waarmaakt** van een tool voor niet-technische gebruikers. De gap met Retool/Budibase/Tooljet is niet in architectuur maar in **polish, data-integratie, en templates**. De goede architectuur maakt het mogelijk om die gap te dichten, maar het vereist een fundamentele verschuiving van "structuur-editor" naar "live-app-editor".
