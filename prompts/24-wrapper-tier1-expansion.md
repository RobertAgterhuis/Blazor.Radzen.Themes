# Prompt 24 — Wrapper-uitbreiding tier 1 (beleid, geen dekking)

Principle: the theme already covers ALL Radzen components — wrappers exist to enforce policy (a11y labels, Dutch defaults, intent enums, sensible defaults, migration isolation), not coverage. Wrap only high-traffic components; complex/rare ones (charts, Scheduler, Gantt, HtmlEditor, PivotGrid, GoogleMap) stay raw-Radzen by design — document that decision.

---

Copy below into Claude Code in the repo root:

---

Extend Agterhuis.Ui with the tier-1 wrapper set below, following ALL existing conventions (Agt prefix, wrap-not-inherit, CSS isolation, tokens only, Label/AriaLabel guard, demo page + ≥2 bUnit tests each, catalog cross-link per prompt 23).

## Tier 1 — build these

Forms (complete the everyday form toolkit):
- `AgtCheckbox` — label ALWAYS rendered (clickable), guard applies.
- `AgtSwitch` — with on/off label support ("Aan"/"Uit" defaults, overridable).
- `AgtRadioList<TValue>` — vertical default, Orientation param; fieldset/legend semantics.
- `AgtTextArea` — auto-rows default 3, char-counter slot (MaxLength shows "x/y tekens").
- `AgtPassword` — show/hide toggle button (aria-pressed, target ≥24px).
- `AgtAutoComplete<TItem>` — debounce default 300ms, "Geen resultaten" empty text, LoadData passthrough.
- `AgtFileUpload` — wraps RadzenUpload: NL default texts, max-size/extension params with built-in validation message, progress display.

Feedback/overlay services:
- `AgtNotificationService` — thin wrapper over Radzen NotificationService with intent-based helpers (`Success/Warning/Danger/Info(string title, string? detail)`), consistent duration defaults, and icons per intent (color never the only carrier).
- Extend `AgtConfirmDialog` service with a `ConfirmDeleteAsync(itemName)` convenience (danger-styled, NL text).

Display/navigation:
- `AgtBadge` — intent-based (AgtIntent + Neutral), icon slot, always icon-or-text + color.
- `AgtTabs`/`AgtTabItem` — thin wrapper: keyboard/ARIA passthrough intact, animated indicator from the theme, lazy-render param.
- `AgtBreadcrumb` — takes `IEnumerable<(string Text, string? Href)>`, nav/aria-label semantics.

## Explicitly NOT wrapped (document in README + docs/CONSUMING.md)

Charts, Scheduler, Gantt, PivotDataGrid, HtmlEditor, Tree, DataFilter, GoogleMap, SSRSViewer, QRCode/Barcode, Chat/AIChat — consumers use raw Radzen (fully themed); add a short "wanneer wrapper, wanneer raw Radzen" decision guide with the policy criteria: wrap = used in 2+ apps AND policy to enforce; raw = rare, complex, or passthrough-only.

## Conventions per wrapper

Parameters follow existing patterns (Disabled, CssClass, AriaLabel/Label guard where input-like); intent via `AgtIntent`; low-level passthrough may expose Radzen types; NL default texts overridable via parameters. Every wrapper: demo page under "Agt componenten" (correct category group), cross-link to its Radzen catalog page, ≥2 bUnit tests (render + behavior; guard test where applicable). Update `docs/RADZEN-COMPONENT-INVENTORY.md` wrapper column and CHANGELOG.md (minor version).

## Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (all guard tests incl. parity/bleed unaffected). Walk each new demo page in two theme families (one light, one dark). Report: wrappers added, decision-guide location, and any Radzen API that forced a deviation from conventions.
