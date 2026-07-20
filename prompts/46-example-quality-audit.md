# Prompt 46 — Voorbeelden-remediatie: identiek, leeg en gevuld-om-te-vullen

Observed at SCALE: many component pages carry three examples that are IDENTICAL, and many of those are also EMPTY (no rendered demo at all — title + tabs, blank demo area). The "≥3 examples" rule was gamed by duplication and stubbing. This is NOT a spot-fix: assume a large share of the inventory is affected and remediate the FULL set systematically, category by category, until both guards (similarity + emptiness) stand at zero.

---

Copy below into Claude Code in the repo root:

---

## 1. Mechanische detectie eerst — BEIDE assen, over de VOLLEDIGE inventaris

The example sources are embedded per file (DemoExample framework) — exploit that:
- **Gelijkenis**: script/xunit-test in `eng/`: per componentpagina alle voorbeeld-bronnen vergelijken; normaliseer (whitespace, parameternamen van sample-data) en bereken gelijkenis. Twee voorbeelden op één pagina met >85% identieke genormaliseerde bron = violatie.
- **Leegte**: combineer met de example-scan uit prompt 47 (Playwright: component-rootclass aanwezig, gerenderde hoogte ≥40px, geen console-error; popups openen echt). Als die scan nog niet bestaat: bouw hem als onderdeel van DEZE prompt — leegte-detectie is een voorwaarde, geen optie.
- **Titel-belofte**: flag voorbeelden waarvan de TITEL iets anders belooft dan de code toont (heuristiek: titelwoorden als "validatie", "disabled", "template", "iconen", "sorteren" die niet in de bron voorkomen).
- Eén gecombineerd rapport `docs/EXAMPLE-AUDIT.md`: per pagina [aantal voorbeelden | gelijkenis-paren | leeg/error | titel-mismatch | oordeel] plus een totaaltelling per categorie — draai dit EERST zodat de werkelijke omvang op tafel ligt vóór er gefixt wordt.
- Beide checks worden blijvende guards (xunit + scan-script, allowlist met reden per bewuste uitzondering).

## 2. Remediatie: categorie voor categorie, met teller

Werk de volledige inventaris af per categorie (Forms → Data → Navigation → Overlays → Feedback → Data Visualization → Display → overig), en werk na elke categorie de voortgangsteller in docs/EXAMPLE-AUDIT.md bij (gefixt/totaal). Geen categorie half afronden. Lege voorbeelden volgen de diagnose-volgorde van prompt 47 (framework-koppeling kapot → ontbrekende children/data → stille exception → CSS): vind eerst de STRUCTURELE oorzaak — als tientallen pagina's leeg zijn is dat vrijwel zeker één framework- of generatorfout, fix die éérst en scan opnieuw voordat je pagina's individueel vult.

Per gevlagde pagina bepaal je daarna de werkelijke capability-set — niet gokken: reflecteer over de publieke parameters/events van het (Radzen- of Agt-)component en raadpleeg de Radzen-demopagina-structuur van het equivalent (welke voorbeelden voert Radzen zelf op — dat is de maat voor "wat valt er te tonen").
- **Componenten met één wezenlijke verschijningsvorm** (bijv. Gravatar, Icon, een simpele validator in isolatie): terugbrengen naar ÉÉN goed voorbeeld; de pagina blijft het sjabloon volgen (intro + voorbeeld + API-tabel). De "≥3"-regel wordt daarmee officieel herzien: vervang hem in prompt-/docs-teksten door "zoveel voorbeelden als er wezenlijk verschillende mogelijkheden zijn, minimaal 1" — werk docs/CONTRIBUTING/copilot-instructions hierop bij.
- **Componenten met echte breedte**: differentieer de voorbeelden langs de capability-assen: states (disabled/busy/validatie-fout), varianten (stijlen, oriëntaties, sizes), data-gedrag (binding, LoadData, templates, filtering), events/interactie, en integratie (in een formulier met validator, in een grid-cel). Elk voorbeeld een eigen titel + 1–3 zinnen uitleg die het verschil benoemt, en code die dat verschil daadwerkelijk bevat.
- Sample-data per voorbeeld variëren (niet drie keer Alfa/Bravo/Charlie) zodat de code-tabs ook visueel verschillen.

## 3. Werk de inventaris bij

`docs/RADZEN-COMPONENT-INVENTORY.md` krijgt een kolom "voorbeelden (n)"; de EXAMPLE-AUDIT moet op nul violaties staan. Pagina's die naar 1 voorbeeld zijn teruggebracht worden gemarkeerd "single-capability" met één regel motivatie.

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen incl. de gelijkenis-guard én de versterkte render-smoke (exception in voorbeeld = fail). De gecombineerde audit staat op NUL over de volledige inventaris: geen identieke paren, geen lege voorbeelden, geen titel-mismatches (buiten gemotiveerde allowlist). Steekproef handmatig in twee families: vijf eerder-gevlagde pagina's — voorbeelden zichtbaar verschillend in demo én code en dekken de kernmogelijkheden; Accordion-klasse pagina's tonen werkende demo's; twee single-capability-pagina's compleet met één voorbeeld. Rapporteer: de omvang uit de eerste scan (aantal pagina's identiek/leeg per categorie), de gevonden structurele oorzaak van de lege voorbeelden, hoeveel pagina's naar 1 voorbeeld gingen (met motivatie), hoeveel gedifferentieerd zijn, en de eindtellers per categorie (overal 100%).
