# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## [Unreleased]
### Added
- Initial scaffold for Agterhuis.Ui Razor Class Library.
- Agt component wrappers, design tokens, custom theme, and demo app.
- xUnit and bUnit test setup for component behavior.
- Added `dagobah-light` and `dagobah-dark` built-in theme variants with full token parity, registration, and smoke-test coverage.
- Tier-1 wrapper uitbreiding: `AgtCheckbox`, `AgtSwitch`, `AgtRadioList<TValue>`, `AgtTextArea`, `AgtPassword`, `AgtAutoComplete<TItem>`, `AgtFileUpload`, `AgtBadge`, `AgtTabs`, `AgtTabItem`, `AgtBreadcrumb`.
- Service-uitbreidingen: `IAgtNotificationService`/`AgtNotificationService` en `IAgtConfirmDialog.ConfirmDeleteAsync`.
- Demo-pagina's en navigatie voor alle tier-1 wrappers toegevoegd onder Agt componenten.
- Documentatiebeleid toegevoegd voor "wanneer wrapper, wanneer raw Radzen" inclusief expliciete non-wrapper set.
- `docs/RADZEN-COMPONENT-INVENTORY.md` uitgebreid met Wrapper-kolom.
- Nieuwe built-in themafamilie `autotaalglas` toegevoegd (`autotaalglas-light` + `autotaalglas-dark`) met volledige token parity, registratie in `AgtTheme`/`AgtUiOptions`, en WCAG-contrastdocumentatie.
- Drie extra built-in Autotaalglas-subfamilies toegevoegd: `autotaalglas-contrast`, `autotaalglas-portal` en `autotaalglas-mono`, inclusief switcher-hints, token parity, bleed-auditdekking en contrastdocumentatie.
- Enterprise navigation polish voor catalog- en showcase-shells: filterbare sidebar, toetsenbordnavigatie (pijlen/Home/End/Enter), rail-weergave op desktop, pakketversie/GitHub-footer en verfijnde token-gedreven actieve/hover/focus-states in gedeelde `_navigation.css`.
- Nieuwe built-in themafamilie `azure` toegevoegd (`azure-light` + `azure-dark`) met portal-dichte hairlines, donker chrome in beide varianten, systeem-Segoe-stack zonder gebundelde Microsoft-assets, token parity, smoke-testdekking en contrastdocumentatie.
- Nieuwe built-in themafamilie `ms365` toegevoegd (`ms365-light` + `ms365-dark`) met Fluent 2 blue chrome, card-first surfaces, zachte radius/shadow-voorkeur, token parity, smoke-testdekking en contrastdocumentatie.
- Nieuwe built-in themafamilie `volt` toegevoegd (`volt-light` + `volt-dark`) met warme grafietcanvas, elektrische lime-accenten, editorial typografie en volledige token parity-dekking.
