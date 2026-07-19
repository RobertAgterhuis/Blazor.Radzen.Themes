# Prompt 37 — Navigatie-polish (van tekstlijst naar enterprise-nav)

Observed: the sidebar works structurally (groups, headers, toggle) but is visually basic — no icons, the active state is a sharp full-bleed rectangle, no visible hover affordance, hard divider lines, flat typography. Bring it to the level of the rest of the system (Linear/Vercel-class nav). Ship the styling in the RCL (`_navigation.css` + nav components) so consumers get it too — not demo-only. No regressions on prompt 35/36 behavior (toggle, rail, drawer, headers-not-items).

---

Copy below into Claude Code in the repo root:

---

Polish the sidebar navigation of both shells (catalog + showcase). Token-driven; parity/bleed/contrast guards stay green; density-aware (comfortable/compact).

## 1. Item-anatomie

- **Iconen**: elk nav-item krijgt een icoon (Material Symbols, 18px, optisch uitgelijnd in een vaste 24px kolom): huis, componenten-categorieën, catalog-familie-iconen, showcase-items hebben ze al — trek gelijk. Idle icoon = muted; hover = secondary; actief = accent.
- **Actieve staat**: geen full-bleed rechthoek maar een afgeronde pill (radius-md token) met 8px inset-marge links/rechts, accent-linkerrand 3px (blijft), zachte accent-alpha vulling, tekst op hoog contrast; de bestaande slide-animatie van de indicator moet zichtbaar tussen pills bewegen.
- **Hover**: subtiele alpha-tint op dezelfde inset-pill-vorm, 150ms ease; pressed = alpha iets sterker. Focus-visible: ring op de pill, niet op de rij.
- **Hoogtes**: 36px comfortable / 30px compact (structurele tokens), label 13px, verticaal gecentreerd.

## 2. Groepen & ritme

- Vervang de harde divider-lijnen door RITME: 20px ruimte boven een groepskop, 6px eronder; alleen tussen de hoofdsecties (Getting started / Agt componenten / Radzen catalogus) een hairline op 50% alpha.
- Groepskoppen: 10.5px caps, letter-spacing token, muted; uitgelijnd met de LABELS (niet met de iconenkolom). Subtitel ("De wrapper-API voor consumers") direct onder de sectiekop, 11px muted, italic-vrij.
- Collapse-chevrons van secties rechts uitgelijnd op één as met de kop.

## 3. Sidebar-frame

- Bovenin een compact filterveld "Filter navigatie…" (typen filtert items client-side, Escape wist; toont "Geen items" leegtekst) — dit is de enterprise-touch die lange menu's tembaar maakt.
- Onderin een vast footer-blok: versienummer van het package (klein, muted) + link naar de GitHub-repo; in de showcase-shell ook het profiel-avatar-blok hierheen.
- Scroll-affordance: zachte fade/schaduw boven- en onderaan wanneer de lijst scrollt (mask-image of pseudo-element, token-kleuren).
- Collapsed rail: iconen gecentreerd, actieve pill wordt een vierkantje met accent-rand, tooltips met labelnaam; filterveld en subtitels verborgen.

## 4. Toetsenbord

Pijltjes omhoog/omlaag lopen door de items (headers overgeslagen), Enter activeert, Home/End springen; werkt samen met het filterveld (pijl-omlaag vanuit het veld naar het eerste resultaat). Controleer dat PanelMenu's eigen keyboard-gedrag niet dubbelt of breekt.

## 5. Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (bestaande nav-tests + nieuw: filter filtert, footer rendert versie). Contrast-sweep over de nieuwe states (pill-vulling, filterveld, footer) — nul violaties. Walk in drie families (plum-dark, hoth-light, imperial-dark), beide dichtheden, expanded + rail + mobiele drawer: iconen uitgelijnd, actieve pill met sliding indicator, hover voelbaar, ritme rustig, filter werkt. Rapporteer vóór/na-beschrijving per onderdeel.
