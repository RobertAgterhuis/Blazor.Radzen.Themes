# Prompt 46 — Voorbeelden-audit: identieke voorbeelden zijn geen voorbeelden

Observed: some component pages carry three examples that are IDENTICAL — the "≥3 examples" rule was satisfied by duplication instead of substance. Audit every component page, decide per component what genuinely distinct examples exist, and fix: either one honest example, or truly differentiated ones that represent the component's capabilities.

---

Copy below into Claude Code in the repo root:

---

## 1. Mechanische detectie eerst

The example sources are embedded per file (DemoExample framework) — exploit that:
- Script/xunit-test in `eng/`: per componentpagina alle voorbeeld-bronnen vergelijken; normaliseer (whitespace, parameternamen van sample-data) en bereken gelijkenis. Twee voorbeelden op één pagina met >85% identieke genormaliseerde bron = violatie. Output: `docs/EXAMPLE-AUDIT.md` met per pagina [aantal voorbeelden | gelijkenis-paren | oordeel].
- Flag ook: voorbeelden waarvan de TITEL iets anders belooft dan de code toont (heuristiek: titelwoorden als "validatie", "disabled", "template", "sorteren" die niet in de bron voorkomen).
- Deze check wordt een blijvende xunit-guard (met allowlist + reden voor bewuste uitzonderingen).

## 2. Per component: wat zijn de ECHTE mogelijkheden?

Voor elke gevlagde pagina bepaal je de werkelijke capability-set — niet gokken: reflecteer over de publieke parameters/events van het (Radzen- of Agt-)component en raadpleeg de Radzen-demopagina-structuur van het equivalent (welke voorbeelden voert Radzen zelf op — dat is de maat voor "wat valt er te tonen").
- **Componenten met één wezenlijke verschijningsvorm** (bijv. Gravatar, Icon, een simpele validator in isolatie): terugbrengen naar ÉÉN goed voorbeeld; de pagina blijft het sjabloon volgen (intro + voorbeeld + API-tabel). De "≥3"-regel wordt daarmee officieel herzien: vervang hem in prompt-/docs-teksten door "zoveel voorbeelden als er wezenlijk verschillende mogelijkheden zijn, minimaal 1" — werk docs/CONTRIBUTING/copilot-instructions hierop bij.
- **Componenten met echte breedte**: differentieer de voorbeelden langs de capability-assen: states (disabled/busy/validatie-fout), varianten (stijlen, oriëntaties, sizes), data-gedrag (binding, LoadData, templates, filtering), events/interactie, en integratie (in een formulier met validator, in een grid-cel). Elk voorbeeld een eigen titel + 1–3 zinnen uitleg die het verschil benoemt, en code die dat verschil daadwerkelijk bevat.
- Sample-data per voorbeeld variëren (niet drie keer Alfa/Bravo/Charlie) zodat de code-tabs ook visueel verschillen.

## 3. Werk de inventaris bij

`docs/RADZEN-COMPONENT-INVENTORY.md` krijgt een kolom "voorbeelden (n)"; de EXAMPLE-AUDIT moet op nul violaties staan. Pagina's die naar 1 voorbeeld zijn teruggebracht worden gemarkeerd "single-capability" met één regel motivatie.

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen incl. de nieuwe gelijkenis-guard (nul on-geallowliste violaties). Steekproef handmatig: vijf eerder-gevlagde pagina's — de voorbeelden verschillen nu zichtbaar in demo én code en dekken de kernmogelijkheden; twee single-capability-pagina's ogen compleet met één voorbeeld. Rapporteer: aantal gevlagde pagina's, hoeveel naar 1 voorbeeld zijn gegaan (met motivatie), hoeveel gedifferentieerd zijn, en de bijgewerkte regel-tekst.
