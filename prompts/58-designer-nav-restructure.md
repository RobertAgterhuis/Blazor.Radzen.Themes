# Prompt 58 — Navigatie: Design System menu + standaard-state

De navigatie heeft twee problemen: (1) alle categorieën zijn standaard ingeklapt waardoor de gebruiker niets ziet bij het laden, en (2) de design-system-items (Patronen, Schrijfwijzer, Starter template, Token-export, Visuele regressie) staan als losse categorie "Ontwerpstelsel" terwijl ze logisch bij de Designer horen. Fix beide.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Standaard-state navigatiemenu

- Bij het laden van de applicatie zijn de categorieën **Getting Started** en **Design System** standaard uitgeklapt. Alle overige categorieën (Agt Componenten, Radzen Catalogus – QA, etc.) blijven standaard ingeklapt.
- Als de gebruiker handmatig een categorie in- of uitklapt, wordt die keuze onthouden in localStorage en heeft die voorrang boven de standaard-state.
- Bij eerste bezoek (geen localStorage) geldt de standaard: Getting Started en Design System open.

## 2. Hernoem en herstructureer

- De categorie "Ontwerpstelsel" wordt hernoemd naar **Design System**.
- De Designer (WYSIWYG-editor op `/designer`) wordt een item BINNEN deze categorie, niet een los menu-item elders.

De categorie **Design System** bevat de volgende items in deze volgorde:

| Item | Route | Toelichting |
|---|---|---|
| Overzicht | `/design-system` | Bestaande overzichtspagina (was "Ontwerpstelsel > Overzicht") |
| Designer | `/designer` | De WYSIWYG-editor (startscherm) |
| Patronen | `/design-system/patterns` | Patroonbibliotheek |
| Schrijfwijzer | `/design-system/writing` | Tone-of-voice en schrijfregels |
| Starter template | `/design-system/starter` | dotnet new template-documentatie |
| Token-export | `/design-system/tokens` | Design token export/overzicht |
| Visuele regressie | `/design-system/visual-regression` | VR-testresultaten en baselines |

- Routes mogen afwijken als ze al bestaan — pas de navigatie-items aan op de bestaande routes, hernoem geen routes tenzij nodig.
- Het Designer-item krijgt een subtiel accent (bijv. een badge "WYSIWYG" of een onderscheidend icoon) zodat het opvalt als het primaire ontwerp-tool.

## 3. Verwijder de oude categorie

- De losse categorie "Ontwerpstelsel" verdwijnt volledig — alle items zitten nu onder "Design System".
- Als er ergens anders in de navigatie een los "Designer"-item staat (bijv. onder "Getting Started"), verwijder dat duplicaat.
- Controleer dat er geen dode links of verweesde menu-items overblijven (grep op de oude routepaden en categorienamen).

## 4. Navigatie-icoon

- De categorie "Design System" krijgt een passend icoon dat het onderscheidt van de componentcategorieën. Gebruik een bestaand Radzen-icoon (bijv. `palette`, `brush`, `design_services`, of `architecture`) — kies het icoon dat het beste past bij de rest van de navigatie-iconen.

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- Bij eerste bezoek (localStorage leeg of incognito): Getting Started en Design System zijn uitgeklapt, overige categorieën ingeklapt
- "Design System" bevat alle 7 items in de juiste volgorde, inclusief Designer
- Geen duplicaat "Designer" of "Ontwerpstelsel" items in de navigatie
- Alle links werken (geen 404's)
- Na handmatig inklappen van Design System + pagina-refresh: de categorie blijft ingeklapt (localStorage-persistentie)
- Rapporteer: de gekozen iconen, eventuele route-aanpassingen, en of er verweesde items zijn gevonden en opgeruimd
