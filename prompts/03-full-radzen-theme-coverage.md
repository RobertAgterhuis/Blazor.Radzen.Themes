# Prompt 3b — Full Radzen theme coverage + component catalog

Goal: every Radzen component looks on-brand (purple `--agt-color-primary-*`, gold `--agt-color-accent-*`) in BOTH light and dark mode — without wrapping them all. Wrappers stay reserved for high-traffic components; the theme itself must cover the entire Radzen library.

---

Copy below into Claude Code in the repo root:

---

In this repo (Agterhuis.Ui design system), extend the theme so that ALL Radzen Blazor components are fully styled by our brand, then prove it with a catalog.

## 1. Restructure theme CSS

Split the current `src/Agterhuis.Ui/wwwroot/css/agt-theme.css` into partials that are bundled into one file (keep the single public URL `_content/Agterhuis.Ui/css/agt-theme.css` working — either concatenate at build time or use plain `@import` of co-located files under `css/theme/`):

- `_variables.css` — the complete `--rz-*` → `--agt-*` mapping for light (`:root`) and dark (`[data-agt-theme="dark"]`)
- one partial per family: `_buttons.css`, `_inputs.css`, `_pickers.css`, `_datagrid.css`, `_navigation.css`, `_overlays.css`, `_layout.css`, `_feedback.css`, `_display.css`, `_scheduler.css`, `_charts.css`

## 2. Map ALL Radzen theme variables

Read the full variable set from the installed Radzen package (`~/.nuget/packages/radzen.blazor/<version>/staticwebassets/css/material-base.css` — parse every `--rz-*` variable it defines). Map every color-bearing variable to an `--agt-*` token; leave structural ones (sizes) at Radzen defaults unless our tokens dictate otherwise. Do not invent variable names — only override ones that exist in the installed version.

## 3. Override coverage per family (light + dark)

This is the COMPLETE component checklist, taken from blazor.radzen.com. Every item below must appear in docs/THEME-COVERAGE.md with a status. Add overrides wherever variables alone don't reach:

- **Buttons**: Button, SplitButton, ToggleButton, SelectBar, Fab, FabMenu, SpeechToTextButton — incl. busy states and all ButtonStyle variants
- **Text inputs**: TextBox, TextArea, Password, Numeric, Mask, AutoComplete, SecurityCode
- **Selection inputs**: CheckBox, CheckBoxList, RadioButtonList, Switch, Slider, Rating, ColorPicker, Chip, ChipList
- **Pickers/dropdowns** (style the popup panels too): DropDown (single/multiple/filtering/grouping), DropDownDataGrid, DropDownTree, ListBox, DatePicker, TimeSpanPicker
- **Forms infrastructure**: TemplateForm, FormField (label float states!), Label, Fieldset, FileInput, Upload, Dropzone
- **Validators** (all render `.rz-message`): RequiredValidator, LengthValidator, NumericRangeValidator, CompareValidator, EmailValidator, RegexValidator, DataAnnotationValidator, CustomValidator — error color, spacing, popup variant
- **Data**: DataGrid (header, stripes, selection, cell selection, group rows/headers/footers, frozen columns, inline/in-cell edit, density, empty state, pager), PivotDataGrid, DataList, DataFilter, Pager, PickList, Table, Tree (selection, checkboxes, drag-drop)
- **Scheduling/planning**: Scheduler (month/week/day, appointments — gold accent for selection/today), Gantt (bars, dependencies, critical path, baselines)
- **Navigation**: Menu, PanelMenu, ContextMenu, Tabs, Steps, Breadcrumb, Accordion, Link, Login, ProfileMenu, TOC (table of contents)
- **Overlays**: Dialog (incl. side dialogs), Popup, Tooltip, Notification
- **Layout**: Layout, Sidebar, Panel, Card, CardGroup, Splitter, Stack, Row, Column, Carousel
- **Feedback**: Alert (all styles/variants), Badge, ProgressBar, ProgressBarCircular, Skeleton-style loading CSS
- **Charts** (brand series palette: primary-500, accent-400, primary-300, accent-600, primary-700, info — plus axis/gridline/legend/tooltip colors per mode): Area, Bar, Column, Donut, Line, Pie, Scatter, Bubble, Stacked variants, Sparkline, SpiderChart, SankeyDiagram, chart trends/annotations
- **Gauges**: ArcGauge, RadialGauge, LinearGauge
- **Display/media**: Timeline, Image, Gravatar, Icon, Markdown (rendered typography!), HtmlEditor (toolbar, dropdowns, dialogs), Chat, AIChat
- **Embed/passthrough** (minimal theming — container borders/backgrounds only; mark N/A where content is external): QRCode, Barcode, GoogleMap, SSRSViewer, Spreadsheet (if present in the installed version), Document Processing components (if present)
- **UI fundamentals**: typography classes (`.rz-text-*`), colors utilities, borders, shadows — verify our token remap flows into these

Rules: tokens only, no hard-coded hex outside `agt-tokens.css`; gold surfaces always get dark text (`--agt-color-gray-900`); WCAG AA contrast in both modes; keep the `prefers-reduced-motion` block global. If a component listed here does not exist in the installed Radzen.Blazor version, mark it "not in installed version" in THEME-COVERAGE.md instead of guessing class names.

## 4. Catalog in the demo app

Create `/catalog/{family}` pages in `samples/Agterhuis.Ui.Demo` — one page per family bullet above (buttons, text-inputs, selection-inputs, pickers, forms, validators, data, scheduling, navigation, overlays, layout, feedback, charts, gauges, display, embed), using RAW Radzen components (not our Agt wrappers) so the pages prove the theme alone carries the brand. Each page shows the main states: default, hover-documented, focus, disabled, validation error where applicable. Add a catalog index page with links, and keep the existing theme toggle so every page can be checked in dark mode.

## 5. Verification

- `dotnet build -c Release` zero warnings; `dotnet test` green.
- Add a bUnit smoke test per family page (renders without exception).
- Produce `docs/THEME-COVERAGE.md`: table of every Radzen component vs. status (variables-only / custom overrides / not applicable), so gaps are visible.
- Flag any Radzen component whose styling could not be reached via CSS variables + class overrides.

Note the installed Radzen.Blazor version in THEME-COVERAGE.md; class names (`.rz-*`) may change between majors, so this doc is the checklist for upgrade regression testing.
