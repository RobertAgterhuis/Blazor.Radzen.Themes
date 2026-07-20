# Prompt 52 — LowCode designer, fase 5: demo-datamodel en databinding (autoruitschade herstel)

Vereist prompts 48–51. Deze fase geeft ontwerpen ECHTE inhoud: een datamodel-ontwerper (entiteiten + velden + seed), binding van datacomponenten (grid, dropdown, lijsten, formulieren) aan die entiteiten, en export van een gegenereerde in-memory dataservice volgens het bestaande ShowcaseDataService-patroon. Demo-data komt uit een MODEL — Monaco is alleen de geavanceerde bewerkroute. **Het domein is autoruitschade herstel** — alle entiteiten, velden en seed-data gebruiken dit vakgebied als showcase.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## 1. Datamodel in het designdocument

- `DesignEntity` (naam, meervoudsnaam) met `DesignField`s (naam, type: string/int/decimal/bool/DateTime/enum-met-waarden, verplicht, voorbeeldpatroon) en een seed-instelling (aantal rijen, vaste seed voor determinisme — huisregel).
- UI: nieuw "Data"-paneel in de designer (tab naast het palet): entiteiten aanmaken/bewerken via de eigen form-wrappers; seed-PREVIEW als tabel (eerste 5 rijen). Seed-generatie: deterministische, Nederlandstalige realistische waarden per veldtype/patroon (hergebruik/extraheer de generatorlogica van ShowcaseDataService naar een deelbare helper).
- Monaco JSON-tab (uit 51) toont het datamodel mee; bewerken met schema-validatie blijft de power-route.

### Voorgedefinieerde domeinentiteiten (autoruitschade herstel)

De designer bevat de volgende entiteiten als **ingebouwde startset** (de gebruiker kan ze aanpassen/verwijderen, maar ze zijn standaard aanwezig zodat de demo direct bruikbaar is):

#### Schadedossier
Centraal dossier per ruitschadegeval.

| Veld | Type | Verplicht | Toelichting |
|---|---|---|---|
| Dossiernummer | string | ja | Formaat: `ATG-2024-NNNNN` |
| Status | enum(Nieuw, Ingepland, InBehandeling, Gereed, Gefactureerd, Gesloten) | ja | |
| Schadedatum | DateTime | ja | |
| AanmaakDatum | DateTime | ja | |
| Schadesoort | enum(Sterretje, Ster, Barst, TotaalBreuk) | ja | |
| GlasType | enum(Voorruit, Achterruit, Zijruit, Dakraam) | ja | |
| Actie | enum(Reparatie, Vervanging) | ja | Reparatie bij sterretje/ster, vervanging bij barst/totaalbreuk |
| VoorexpertiseNodig | bool | ja | Verzekeraar vereist voorexpertise vóór uitvoering |
| VoorexpertiseStatus | enum(NietNodig, Aangevraagd, Goedgekeurd, Afgewezen) | ja | |
| AdasHerkalibratie | bool | ja | Voertuig heeft ADAS-sensoren achter de ruit |
| AdasStatus | enum(NietNodig, Ingepland, Uitgevoerd, NietMogelijkDoorverwijzing) | nee | |
| Opmerkingen | string | nee | Vrij tekstveld |

#### Klant
De eigenaar/bestuurder of leasemaatschappij.

| Veld | Type | Verplicht | Toelichting |
|---|---|---|---|
| KlantId | int | ja | Auto-increment |
| Klantnaam | string | ja | Persoonsnaam of bedrijfsnaam |
| KlantType | enum(Particulier, Lease, Wagenparkbeheerder, Dealer) | ja | |
| Telefoonnummer | string | ja | NL-formaat |
| Email | string | nee | |
| Verzekeraar | string | nee | Bijv. Centraal Beheer, FBTO, Interpolis |
| Polisnummer | string | nee | |
| Eigenrisico | decimal | nee | Eigen-risicobedrag in euro |
| LeasemaatschappijNaam | string | nee | Alleen gevuld bij KlantType=Lease |
| Adres | string | nee | |
| Postcode | string | nee | NL-formaat `1234 AB` |
| Woonplaats | string | nee | |

#### Voertuig
Het beschadigde voertuig.

| Veld | Type | Verplicht | Toelichting |
|---|---|---|---|
| VoertuigId | int | ja | Auto-increment |
| Kenteken | string | ja | NL-kentekenformaat (bijv. `AB-123-C`) |
| VinNummer | string | ja | 17-karakter VIN |
| Merk | string | ja | Bijv. Volkswagen, Toyota, BMW, Peugeot |
| Model | string | ja | |
| Bouwjaar | int | ja | |
| Kleur | string | nee | |
| HeeftAdas | bool | ja | ADAS-camera/sensoren achter de (voor)ruit |
| AdasSensoren | string | nee | Beschrijving sensorpakket (bijv. "Lane Assist, ACC, AEB") |
| GlascoatingAanwezig | bool | nee | |
| Ruitsticker | bool | nee | Bijv. tolvignet, registratiesticker |

#### Werkorder
De uit te voeren werkzaamheden per dossier.

| Veld | Type | Verplicht | Toelichting |
|---|---|---|---|
| WerkorderId | int | ja | Auto-increment |
| DossierNummer | string | ja | FK naar Schadedossier |
| WerkorderType | enum(RuitReparatie, RuitVervanging, AdasKalibratie, NalevingControle) | ja | |
| PlanDatum | DateTime | ja | |
| PlanTijdvak | enum(Ochtend, Middag, Dag) | ja | |
| Monteur | string | ja | Naam van de uitvoerend monteur |
| Locatie | enum(Werkplaats, OpLocatie) | ja | |
| OpLocatieAdres | string | nee | Alleen bij Locatie=OpLocatie |
| Artikelnummer | string | nee | Onderdeelnummer van de ruit/kit |
| Leverancier | string | nee | Bijv. Saint-Gobain Sekurit, Pilkington, AGC |
| Status | enum(Gepland, Onderweg, Bezig, Afgerond, Geannuleerd) | ja | |
| AdasKalibratieTool | string | nee | Bijv. Hella Gutmann, Bosch DAS 3000 |
| UrenBesteed | decimal | nee | |
| Opmerkingen | string | nee | |

#### Factuur
Financiële afhandeling.

| Veld | Type | Verplicht | Toelichting |
|---|---|---|---|
| FactuurId | int | ja | Auto-increment |
| FactuurNummer | string | ja | Formaat: `F-2024-NNNNN` |
| DossierNummer | string | ja | FK naar Schadedossier |
| FactuurDatum | DateTime | ja | |
| BedragExBtw | decimal | ja | |
| BtwBedrag | decimal | ja | 21% |
| BedragInclBtw | decimal | ja | |
| Eigenrisico | decimal | nee | Doorbelast aan klant |
| VerzekeraarAandeel | decimal | nee | |
| BetaalStatus | enum(Open, Verzonden, BetaaldKlant, BetaaldVerzekeraar, Volledig) | ja | |
| Creditnota | bool | nee | |

#### Voorraad (lookup)
Ruitvoorraad en artikelen.

| Veld | Type | Verplicht | Toelichting |
|---|---|---|---|
| ArtikelId | int | ja | Auto-increment |
| Artikelnummer | string | ja | |
| Omschrijving | string | ja | Bijv. "Voorruit VW Golf 8 2020+ ADAS" |
| GlasType | enum(Voorruit, Achterruit, Zijruit, Dakraam) | ja | |
| MetAdas | bool | ja | Ruit geschikt voor ADAS-camera |
| VoorraadAantal | int | ja | |
| MinimumVoorraad | int | ja | Bestelsignaal |
| InkoopPrijs | decimal | ja | |
| VerkoopPrijs | decimal | ja | |
| Leverancier | string | ja | |
| LevertijdDagen | int | nee | |

### Seed-data-vereisten

De seed-generator produceert **minimaal 25 schadedossiers** met samenhangende data:
- Realistische Nederlandse namen, adressen, postcodes en plaatsen.
- Mix van KlantType: ±40% particulier, ±35% lease, ±15% wagenparkbeheerder, ±10% dealer.
- Bekende Nederlandse verzekeraarsnamen (Centraal Beheer, Interpolis, FBTO, Univé, OHRA, Allianz, etc.).
- Realistische leasemaatschappijen (LeasePlan, ALD Automotive, Alphabet, Arval, Athlon).
- Echte automerken en -modellen passend bij het bouwjaar; ADAS-sensoren realistisch voor modellen vanaf ±2018.
- Kentekens in geldig NL-formaat; VIN-nummers in geldig 17-karakter formaat.
- Voorexpertise nodig bij ±30% van de dossiers (hogere kans bij duurdere voertuigen en vervanging).
- ADAS-herkalibratie bij alle voertuigen met HeeftAdas=true + GlasType=Voorruit + Actie=Vervanging.
- Werkorders met logische plandata (na schadedatum, ADAS-kalibratie NA ruitvervanging).
- Facturen met realistische bedragen: ruitreparatie €75–€150, ruitvervanging €250–€1200 (afhankelijk van merk/ADAS), ADAS-kalibratie €150–€400.
- Voorraad: ±30 artikelen met realistische ruiten voor de voertuigen in de dossiers.
- Alles deterministische seed (vaste `Random(42)`) conform de huisregel.

## 2. Binding in de designer

- Bindbare parameters (registry-metadata markeert ze: `Data`, `TValue`-koppelingen, Text/Value-properties): het property-panel toont naast vrije invoer een BRON-keuze — entiteit (voor collecties) of entiteit.veld (voor waarden/kolommen).
- DataGrid krijgt een kolommen-editor in het property-panel: kolommen afleiden uit de entiteitvelden (aanvinken, titel, formaat — numeriek rechts uitgelijnd met tabular figures conform de huisregels), sorteer/filter/paging-vlaggen.
- Formulier-sectie: "genereer formulier uit entiteit" — velden → passende Agt-wrappers (het patroon uit de patronenbibliotheek volgt), incl. validators uit verplicht/type.
- Designtijd-rendering: de DesignRenderer voedt gebonden componenten met de seed-data (geen echte services in de editor).

## 3. Export met dataservice

- CodeGen breidt uit: per entiteit een record + een gegenereerde `<Naam>DataService` (in-memory, deterministische seed, CRUD-methoden signature-compatibel met het ShowcaseDataService-patroon), DI-registratie in de template-`Program.cs`, en bindingen in de gegenereerde Razor (`@inject`, `Data="@..."`, kolomdefinities).
- Compile-roundtrip-test uitgebreid: geëxporteerd project met gebonden grid buildt én de bUnit-render toont seed-rijen.
- `docs/designer/DATA.md`: het datamodel, de bindingsemantiek, en hoe je de gegenereerde service later vervangt door een echte API-service (het contract is het aanknopingspunt). Documenteer de domeinentiteiten (autoruitschade) als voorbeeld.

## Verificatie

Build/test groen. Handmatig: de voorgedefinieerde entiteit "Schadedossier" openen → grid op canvas binden → kolommen kiezen (dossiernummer, kenteken, schadesoort, glastype, status, ADAS) → seed-data live zichtbaar in de designer met realistische autoruitschade-data → formulier genereren uit de entiteit "Werkorder" → export → uitgepakt project draait met gevulde grid en werkend formulier. Rapporteer: de bindbare-parameter-dekking (welke componenten v1 bindbaar zijn), de seed-generator-mogelijkheden, en het service-contract in de export.
