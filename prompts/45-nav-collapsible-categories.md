# Prompt 45 — Catalogus-nav: uitklapbare categorieën i.p.v. vlakke lijst + indexpagina's

Observed: the catalog nav is now one huge FLAT list — every component a top-level item with a noisy "(Radzen)" suffix, category index pages as separate entries, child components (Body, Footer, GridRow, SplitterPane, DataListRow, DataFilterItem...) with their own pages, and one glitched item rendering as "~O__TH@der (Radzen)". Restructure to the blazor.radzen.com pattern: collapsible category groups with the components as submenu items.

---

Copy below into Claude Code in the repo root:

---

## 1. Nav-structuur: categorie → uitklapbare subitems

- De Radzen-catalogus in de sidebar wordt een boom: één uitklapbaar item per categorie (DataGrid, Data, Forms, Validators, Navigation, Layout, Overlays, Feedback, Data Visualization, Display, ...— volg de categorie-indeling van de inventaris), met daaronder de componentpagina's als subitems. Gebruik het bestaande PanelMenu-nesting-gedrag (chevron, indent-niveau, keyboard).
- **Weg met de "(Radzen)"-suffix op elk item**: de sectiekop "Radzen catalogus" draagt die context al; subitems heten gewoon "TextBox", "AutoComplete", "RequiredValidator". De Agt-componentengroep blijft zoals hij is.
- Indexpagina's: de categorie-rij zelf navigeert bij klik op de LABEL naar de (dunne) categoriepagina en klapt uit bij klik op de chevron (of: uitklappen bij klik, indexpagina bereikbaar als eerste subitem "Overzicht" — kies het patroon dat PanelMenu a11y-correct ondersteunt en documenteer de keuze). De indexpagina's blijven bestaan voor deep-links maar zijn geen losse nav-items meer.
- Gedrag: actieve component houdt zijn categorie open; expand-state per categorie gepersisteerd (localStorage); "Filter navigatie" zoekt óók door subitems en klapt categorieën met treffers open (en weer dicht bij wissen); collapsed rail toont alleen categorie-iconen, flyout-submenu bij klik/hover met de subitems.

## 2. Opruimen wat de vlakke lijst blootlegde

- **Child-componenten verwijderen als eigen pagina's/nav-items**: alles wat in docs/RADZEN-COMPONENT-INVENTORY.md als "child of X" staat (Body/Footer/Header binnen Layout, GridRow, SplitterPane, DataListRow, DataFilterItem, ColumnOptions, ...) verliest zijn losse route en wordt gedemonstreerd op de parent-pagina (redirect van oude route naar parent-anker zodat links niet breken). Werk de inventaris-kolom bij.
- **Het kapotte item** ("~O__TH@der"): vind de oorzaak (corrupte icon-naam/ligature, encoding, of een mislukte template-substitutie in de nav-generator) en fix generiek — als de nav uit de inventaris gegenereerd wordt, hoort een test elke itemnaam tegen een geldige-naam-patroon te checken.
- Dubbelingen: categorieën die nu twee keer voorkomen (als index-item én als groep) ontdubbelen.

## 3. Guardrails

Nav-styling blijft binnen de bestaande tokens (nav-radius 0, idle transparant, pill-active); nesting krijgt een duidelijk maar rustig indent (geen extra lijnen); contrast-sweep over de nieuwe subitem-states; toetsenbord: pijltjes door zichtbare items, links/rechts klapt categorie dicht/open (PanelMenu-standaard), filter → eerste treffer bereikbaar met pijl-omlaag. bUnit: categorie klapt uit/dicht met persistentie, filter opent categorieën met treffers, child-routes redirecten.

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (incl. de nieuwe nav-tests; smoke tests bijgewerkt op verwijderde child-routes). Handmatig in twee families: boom klapt soepel, geen "(Radzen)"-ruis, geen child-items, geen kapotte labels, filter werkt door de boom heen, rail-flyout werkt, actieve pagina houdt zijn categorie open na refresh. Rapporteer: de oorzaak van het kapotte item, het gekozen indexpagina-patroon, en het aantal nav-items vóór/na (moet fors dalen).
