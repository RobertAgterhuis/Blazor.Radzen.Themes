# Razor Parser Roadmap (Post-DFI-003)

Datum: 2026-07-22  
Status: gepland (niet in current fixbatch)

## Doel

Een veilige Razor->design model parser toevoegen voor de designer, zonder regressies in:
- documentintegriteit
- undo/redo
- validatie
- export
- deterministische serialisatie

## Waarom uitgesteld

DFI-003 is nu opgelost via expliciet read-only contract voor de Razor-tab.  
Een parser is functioneel waardevol, maar te groot voor een `smallest safe change` fixbatch.

## Roadmap

### Fase 1 - Contract en subset (MVP)

Doel:
- Definieer supported Razor subset die door de designer zelf gegenereerd wordt.
- Alles buiten subset wordt expliciet als unsupported gemeld.

Taken:
1. Spec document opstellen met ondersteunde component syntax.
2. Mappingregels voor component -> `DesignNode` + parameters + slots.
3. Error model voor parsefouten (pad, regel, type).
4. Feature flag (`EnableRazorRoundTrip`) toevoegen.

Acceptatiecriteria:
1. Unsupported syntax geeft duidelijke fout, geen silent corruption.
2. Parser muteert bestaand document niet bij parsefailure.

### Fase 2 - Parse pipeline implementatie

Doel:
- Veilige parseflow met validatie en migration hooks.

Taken:
1. Parser service introduceren (pure service, geen UI-afhankelijkheid).
2. Parse output naar tijdelijk `DesignDocument` model.
3. `DesignDocumentValidator` integreren vóór apply.
4. Alleen bij valide output `OnDocumentChanged` triggeren.

Acceptatiecriteria:
1. Foute input resulteert in foutmelding, niet in gedeeltelijke apply.
2. Undo/redo werkt na parser-apply identiek aan JSON-apply gedrag.

### Fase 3 - Roundtrip hardening

Doel:
- Deterministische en betrouwbare roundtrip voor ondersteunde subset.

Taken:
1. Golden tests: model -> Razor -> model vergelijkingen.
2. Snapshot tests voor gegenereerde Razor fragmenten.
3. Property/slot edgecases (nested templates, enum/nullables).
4. Performance budget en parse-time metingen.

Acceptatiecriteria:
1. Roundtrip is lossless binnen supported subset.
2. Parser voert geen side effects uit buiten apply-moment.
3. Testdekking voor high-risk mappers is expliciet aanwezig.

## Teststrategie

### Unit tests
1. `DesignerRazorParserTests` voor syntax/mapping/error cases.
2. `DesignerRazorRoundtripTests` voor deterministische subset roundtrip.

### bUnit tests
1. `DesignerCodePanel_FindingDfi003_*` voor UI-flow:
   - parse success -> model update
   - parse fail -> foutmelding + geen modelwijziging

### End-to-end (optioneel)
1. Eén Playwright flow voor edit/apply/reload op representative pagina.

## Risico's en mitigaties

1. Razor grammar-complexiteit:
   - Mitigatie: subset-first, harde unsupported grenzen.
2. Stille modelcorruptie:
   - Mitigatie: immutabele parse-resultaten + validate-before-apply.
3. Incompatibiliteit met bestaande export:
   - Mitigatie: golden export regressietests per fase.

## Exit criteria voor activatie

Parser mag pas default-enabled worden als:
1. Fase 1-3 acceptatiecriteria gehaald zijn.
2. Build + volledige testset groen blijft.
3. Auditfinding DFI-003 herlabeld kan worden naar `Fixed (Parser)`.