# Showcase Coverage (Prompt 28)

[Docs Index](README.md)

This document tracks which standalone Radzen components are meaningfully exercised in the workflow-style showcase app under `/app` routes.

Deze matrix beschrijft welke Radzen standalone componenten betekenisvol in de `/app` workflow zijn toegepast.

## Scope en methode

- Inventarisbron: `docs/RADZEN-COMPONENT-INVENTORY.md`.
- Workflow-scope: shell + pagina's onder `/app/*`.
- We tellen alleen workflow-relevante standalone componenten mee.
- We sluiten bewust uit: interne basiscomponenten, spreadsheet-editor dialoogonderdelen, en componenten die externe diensten/API-sleutels nodig hebben.

## Resultaat

- Workflow-targetset: **63** standalone componenten.
- Meaningful toegepast in `/app`: **58**.
- Bewust niet toegepast: **5**.
- Coverage: **92.1%**.

Formule:

$$
\text{coverage} = \frac{58}{63} \times 100 = 92.1\%
$$

## Matrix (toegepast)

| Component | Route(s) | Workflow-context |
|---|---|---|
| RadzenDataFilter | /app/werkorders | Geavanceerde filtering van operationele orderlijst |
| RadzenDropDownDataGrid | /app/werkorders | Klantkeuze met gridcontext in orderaanmaak |
| RadzenMask | /app/werkorders, /app/werkorders/{id} | Postcode-invoer en validatie |
| RadzenAutoComplete | /app/werkorders, /app/werkorders/{id} | Adresaanvulling tijdens intake |
| RadzenDataList | /app/werkorders, /app/rapportage | Mobiele lijstweergave en prestatie-overzicht |
| RadzenPager | /app/werkorders | Compacte mobiele paginering |
| RadzenScheduler + views | /app/planning | Capaciteits- en afspraakplanning |
| RadzenSelectBar | /app/planning, /app/servicedesk | View/status schakelen |
| RadzenTimeSpanPicker | /app/planning | Standaard duurinstelling voor verplaatsing |
| RadzenRadioButtonList | /app/planning | Tijdslotselectie |
| RadzenGantt | /app/projecten | Fase- en taakplanning per project |
| RadzenSteps | /app/projecten | Wizard voor projectcreatie |
| RadzenPickList | /app/projecten | Teamselectie per project |
| RadzenAccordion | /app/projecten | Fase-uitleg per project |
| RadzenTree | /app/assets | Assetstructuur en selectie |
| RadzenTable | /app/assets | Asset-specificaties |
| RadzenColorPicker | /app/assets | Labelkleurbeheer |
| RadzenDropDownTree | /app/assets, /app/werkorders/{id} | Assetkoppeling vanuit hiërarchie |
| RadzenTemplateForm | /app/assets, /app/werkorders/{id} | Gestructureerde detailbewerking |
| RadzenListBox | /app/servicedesk | Ticketselectie |
| RadzenAIChat | /app/servicedesk | Assistent in servicedeskflow |
| RadzenLogin | /app/servicedesk | Sessieverloop/login fallback |
| RadzenMarkdown | /app/help | Handleidingrendering |
| RadzenToc | /app/help | Inhoudsopgave navigatie |
| RadzenLink | /app/help | Hulptools en runbook-links |
| RadzenPopup | /app/help, layout | Contextuele hulp/notifications |
| RadzenHtmlEditor | /app/werkorders/{id} | Monteursnotities |
| RadzenUpload | /app/werkorders/{id} | Bijlagen/foto-upload |
| RadzenFileInput | /app/werkorders/{id} | Bestandselectie |
| RadzenDropZone | /app/werkorders/{id} | Drop-gebied voor media |
| RadzenCarousel | /app/werkorders/{id} | Foto-preview |
| RadzenTimeline | /app/werkorders/{id} | Historie/auditflow |
| RadzenGravatar | /app/werkorders/{id}, layout | Actorvisualisatie |
| RadzenQRCode | /app/werkorders/{id} | Werkbondeelbaar maken |
| RadzenBarcode | /app/werkorders/{id} | Werkbon scannen op locatie |
| RadzenRating | /app/werkorders/{id} | Klanttevredenheid bij afronding |
| RadzenSplitButton | /app/werkorders/{id} | Opslaan-acties met varianten |
| RadzenProgressBar | /app/projecten, /app/werkorders/{id} | Status- en voortgangsvisualisatie |
| RadzenSparkline | /app/rapportage | KPI-trend op metric-tegels |
| RadzenChart | /app/rapportage | Omzet- en volumegrafieken |
| RadzenStackedColumnSeries | /app/rapportage | Typeverdeling over tijd |
| RadzenArcGauge | /app/rapportage | Bezettingsgraad |
| RadzenRadialGauge | /app/rapportage | SLA-score |
| RadzenLinearGauge | /app/rapportage | Voorraadniveau |
| RadzenSankeyDiagram | /app/rapportage | Stroom van type naar status |
| RadzenSplitter | /app/instellingen | Settings layout in functionele panelen |
| RadzenFieldset | /app/instellingen | Formuliergroepering |
| RadzenSlider | /app/instellingen | Reistijd- en grensinstellingen |
| RadzenSecurityCode | /app/instellingen | 2FA invoer |
| RadzenCompareValidator | /app/instellingen | Wachtwoordherhaling |
| RadzenToggleButton | /app/werkorders | Dichtheid/compact mode schakelen |
| RadzenPassword | /app/instellingen | Beveiligingsinstellingen |
| RadzenCardGroup | /app/projecten | Projectsamenvattingen |
| RadzenChipList | /app/assets | Asset-tags |
| RadzenSpeechToTextButton | /app/werkorders/{id} | Notitie-invoer met spraak |
| RadzenDialog/RadzenNotification | /app shell + pages | Realtime feedback, details, quick preview |
| RadzenSidebar/RadzenLayout/RadzenHeader/Body | /app shell | Productieachtige shellnavigatie |

## Bewust niet (binnen targetset)

| Component | Reden |
|---|---|
| RadzenGoogleMap | Vereist externe Maps API key, niet deterministisch voor lokale demo |
| RadzenSSRSViewer | Vereist externe SSRS server en credentials |
| RadzenSpreadsheet | Geen echte workflowwaarde voor werkorderproces; vooral editor-tooling |
| RadzenPivotDataGrid | Volgende iteratie: nu placeholder in rapportage |
| RadzenChat | AIChat + ticketthread dekt servicedeskdoel al af zonder dubbele UX |

## Verificatie uitgevoerd

- `dotnet build Agterhuis.Ui.sln -c Release -v minimal` ✅
- `dotnet test Agterhuis.Ui.sln -c Release -v minimal` ✅ (157/157)
- Route-smoke uitgebreid met:
  - `/app/projecten`
  - `/app/assets`
  - `/app/servicedesk`
  - `/app/help`
  - `/app/werkorders/{id}`
- Gedragstests toegevoegd voor:
  - projectcreatie
  - chatbericht versturen
  - werkorder-aanmaak vanuit asset-context
