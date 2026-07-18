# Prompt 35 — Hamburger/sidebar-gedrag + menu-defecten

Observed (plum-dark, Home): the hamburger button does NOTHING — the sidebar is always expanded; that defeats its purpose. Additionally: the category group headers render as grayed-out MENU ITEMS creating apparent duplicates ("De wrapper-API voor consumers" as an item, "Knoppen" above "Buttons", "Formulieren" above the form fields, "Data" above "Data Grid", "Feedback" and "Layout" duplicated), the sidebar surface renders LIGHT under the dark theme, and the page title "Agterhuis.Ui" shows an input-like outline box. Fix all four; this applies to the demo shell AND the showcase shell.

---

Copy below into Claude Code in the repo root:

---

## 1. Hamburger = sidebar-besturing (demo shell én showcase shell)

- Klik op de hamburger togglet de sidebar: desktop ≥1024px → wissel tussen volledig uitgeklapt en icon-rail (collapsed; alleen iconen met tooltip, actieve rail-glow bestaat al); <1024px → sidebar wordt een overlay-drawer boven de content met backdrop, sluit op buitenklik, Escape en navigatie.
- Persist de keuze (localStorage) per shell; standaard uitgeklapt op desktop, dicht op mobiel.
- Toegankelijkheid: `aria-expanded` + `aria-controls` op de knop, `aria-label` "Menu verbergen/tonen", focus blijft beheerd bij overlay (trap in drawer, restore naar de knop).
- De hamburger staat LINKS van de merknaam, visueel gekoppeld aan de sidebar; animatie van de sidebar-breedte via transform/width binnen de bestaande motion-regels (reduced-motion → direct).
- Gebruik de RadzenLayout/RadzenSidebar `Expanded`-mechaniek die er al is — geen parallelle eigen state.

## 2. Categoriekoppen zijn items geworden (duplicaten-bug)

De groepering uit de menu-herstructurering rendert de categorielabels ("Knoppen", "Formulieren", "Data", "Feedback", "Layout") en zelfs de groep-subtitel ("De wrapper-API voor consumers") als disabled PanelMenu-ITEMS. Fix: koppen zijn géén items — render ze als niet-interactieve headers/dividers (eigen element buiten de PanelMenu-items, of het door Radzen ondersteunde niet-klikbare template), zonder hover/focus, niet tabbaar, correct gemute-stijl. Verwijder de dubbelingen; in collapsed rail-modus verdwijnen de koppen (alleen iconen). Controleer ook de showcase-sidebar op hetzelfde patroon.

## 3. Sidebar licht in dark theme

De sidebar-achtergrond oogt licht onder plum-dark — token-bleed of een verkeerde surface-token op de nieuwe geherstructureerde sidebar. Fix aan de token-bron; de runtime-probe bevat de sidebar al — draai hem en bevestig dat sidebar-bg + idle/hover/active nav in alle families correct schakelen.

## 4. Outline-box om de paginatitel

De h1 "Agterhuis.Ui" toont een rechthoekige outline alsof het een tekstveld is. Onderzoek: skip-link/focus die op de titel landt met een input-achtige focusstijl, een per ongeluk toegepaste `contenteditable`/inputclass, of een debug-outline. De titel moet een gewone heading zijn; programmatische focus (na skip-link) mag een SUBTIELE focusstijl tonen die bij headings past, niet de input-box. Fix generiek (alle paginatitels).

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen; bleed/contrast-guards ongewijzigd groen. Handmatig: hamburger togglet correct op 1920px (rail) en 800px (overlay-drawer met backdrop/Escape) in demo én showcase; menu toont koppen als koppen zonder duplicaten, tab-volgorde slaat koppen over; sidebar donker in plum-dark en correct in twee andere families; geen outline-box op titels. bUnit: test dat de toggle `aria-expanded` wisselt en dat categoriekoppen geen focusable items zijn. Rapporteer per punt de gevonden oorzaak.
