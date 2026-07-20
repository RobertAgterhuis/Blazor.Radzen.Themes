# Prompt 38 — Versie-placeholder, dichtheidsknop en Instellingen-pagina

Observed (showcase /app/instellingen, autotaalglas-light): the sidebar footer literally shows "Agterhuis.Ui v@PackageVersion" (unsubstituted placeholder), the density toggle renders as a giant full-width blue button "COMPACTE GRIDDICHTHEID" in the middle of the settings form, and the Beveiliging (demo) fieldset shows four bare unlabeled boxes (SecurityCode without label/context). Plus general form-layout polish on this page. Fix properly.

---

Copy below into Claude Code in the repo root:

---

## 1. Versienummer in de sidebar-footer (beide shells)

"v@PackageVersion" is a literal string — the substitution never happens. Implement it properly: read the informational version at runtime via `typeof(AgtThemeState).Assembly` (`AssemblyInformationalVersionAttribute`, fallback `AssemblyFileVersion`), expose it via a small `AgtVersionInfo` helper in the RCL, and render "Agterhuis.Ui v{x.y.z}" in both sidebar footers. Strip any `+commithash` suffix for display. bUnit-test: footer renders a semver-achtige string, nooit een "@"-placeholder. Grep de hele solution op andere niet-gesubstitueerde `@Package`/`@Version`-placeholders.

## 2. Dichtheids-toggle is een schreeuwende knop

The density control does not belong as a full-width primary button inside the settings form:
- In het formulier: vervang door een nette instelling-rij consistent met de rest — label "Griddichtheid" + AgtSwitch of een compacte SelectBar (Comfortabel/Compact), gebonden aan dezelfde density-state als de topbar-toggle (één bron, geen dubbele state).
- De topbar "COMFORTABEL"-knop blijft de snelle toggle; controleer dat beide dezelfde localStorage-state lezen/schrijven en elkaar live volgen.
- Nooit ButtonStyle.Primary/volle breedte voor een voorkeursinstelling; audit de Instellingen-pagina op andere knoppen die per ongeluk primary/full-width zijn (er is er precies één primaire actie: OPSLAAN).

## 3. Beveiliging (demo) fieldset

- De vier kale vakjes zijn de SecurityCode zonder label: geef het geheel een label ("Verificatiecode (demo)"), helptext ("Voer de 4-cijferige code in — demo, geen echte 2FA"), en correcte grootte/spacing binnen het fieldset; AriaLabel verplicht (guard).
- Wachtwoordvelden: gebruik AgtPassword (toon/verberg-toggle) met de CompareValidator zichtbaar gedemonstreerd (foutmelding bij mismatch), labels boven de velden zoals de rest van het formulier.
- Het fieldset moet in beide varianten (licht/donker) correct ogen — randen/legend via tokens.

## 4. Instellingen-pagina layout-polish

- Twee-koloms layout gedraagt zich nu ongelijk (rechterkolom zweeft hoog, actieknoppen los rechtsonder in het niets): breng structuur — secties als AgtCards met koppen, gelijke kolombreedtes met nette gutter, HERSTELLEN/OPSLAAN als AgtFormActions ONDER de secties (rechts uitgelijnd, sticky niet nodig), "Actieve instellingen"-samenvatting als aparte muted card in plaats van losse tekstregels.
- Slider "Standaard reistijd" toont zijn huidige waarde naast het label ("30 min"); checkboxen krijgen consistente rij-hoogtes.
- Density compact/comfortable blijft overal correct (de pagina zelf test dit visueel — screenshot in beide dichtheden bij de verificatie).

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (nieuwe versie-footer test; bestaande guards ongewijzigd). Contrast-sweep over de aangepaste pagina — nul violaties. Handmatig: footer toont echt versienummer in beide shells; dichtheid wisselen via formulier én topbar blijft synchroon (ook na refresh); beveiligingsblok gelabeld en werkend; Instellingen oogt als een nette enterprise-instellingenpagina in licht en donker, comfortabel en compact. Rapporteer per punt de oorzaak (vooral: waarom de versie-substitutie faalde).
