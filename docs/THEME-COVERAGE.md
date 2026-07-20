# Theme Coverage

[Docs Index](README.md)

Installed Radzen.Blazor version: `11.1.5`

Built-in theme families: `plum`, `ocean`, `dagobah`, `dathomir`, `hoth`, `tatooine`, `imperial`, `azure`, `ms365`, `volt`, `autotaalglas`, `autotaalglas-contrast`, `autotaalglas-portal`, `autotaalglas-mono`.

Catalog coverage routes used by the inventory mapping:

- `/catalog/buttons`
- `/catalog/text-inputs`
- `/catalog/selection-inputs`
- `/catalog/pickers`
- `/catalog/forms`
- `/catalog/forms-advanced`
- `/catalog/data`
- `/catalog/data-advanced`
- `/catalog/layout`
- `/catalog/layout-advanced`
- `/catalog/navigation`
- `/catalog/feedback`
- `/catalog/charts-advanced`
- `/catalog/gauges`
- `/catalog/display`
- `/catalog/embed`
- `/catalog/overlays-advanced`
- `/catalog/all-components`

Status legend:

- `variables-only`: covered via `src/Agterhuis.Ui/wwwroot/css/theme/_variables.css`
- `custom overrides`: covered via family partials in `src/Agterhuis.Ui/wwwroot/css/theme/`
- `not in installed version`: not present in Radzen 11.1.5 type inventory
- `not applicable`: external/embed content where only container styling is applied

## Coverage Table

| Family | Component | Status | Notes |
|---|---|---|---|
| Buttons | Button | custom overrides | `_buttons.css` |
| Buttons | SplitButton | custom overrides | `_buttons.css` |
| Buttons | ToggleButton | custom overrides | `_buttons.css` |
| Buttons | SelectBar | custom overrides | `_buttons.css` |
| Buttons | Fab | custom overrides | `_buttons.css` |
| Buttons | FabMenu | custom overrides | `_buttons.css` |
| Buttons | SpeechToTextButton | variables-only | variable mapping only |
| Text inputs | TextBox | custom overrides | `_inputs.css` |
| Text inputs | TextArea | custom overrides | `_inputs.css` |
| Text inputs | Password | custom overrides | `_inputs.css` |
| Text inputs | Numeric | custom overrides | `_inputs.css` |
| Text inputs | Mask | custom overrides | `_inputs.css` |
| Text inputs | AutoComplete | custom overrides | `_inputs.css` |
| Text inputs | SecurityCode | variables-only | variable mapping only |
| Selection inputs | CheckBox | custom overrides | `_inputs.css` |
| Selection inputs | CheckBoxList | variables-only | variable mapping only |
| Selection inputs | RadioButtonList | variables-only | variable mapping only |
| Selection inputs | Switch | custom overrides | `_inputs.css` |
| Selection inputs | Slider | custom overrides | `_inputs.css` |
| Selection inputs | Rating | custom overrides | `_inputs.css` |
| Selection inputs | ColorPicker | custom overrides | `_inputs.css` |
| Selection inputs | Chip | variables-only | variable mapping only |
| Selection inputs | ChipList | variables-only | variable mapping only |
| Pickers | DropDown | custom overrides | `_pickers.css` |
| Pickers | DropDownDataGrid | custom overrides | `_pickers.css` |
| Pickers | DropDownTree | not in installed version | no `RadzenDropDownTree` in 11.1.5 |
| Pickers | ListBox | custom overrides | `_pickers.css` |
| Pickers | DatePicker | custom overrides | `_pickers.css` |
| Pickers | TimeSpanPicker | custom overrides | `_pickers.css` |
| Forms infrastructure | TemplateForm | variables-only | variable mapping only |
| Forms infrastructure | FormField | variables-only | label float states covered by variables |
| Forms infrastructure | Label | variables-only | variable mapping only |
| Forms infrastructure | Fieldset | custom overrides | `_layout.css` |
| Forms infrastructure | FileInput | variables-only | variable mapping only |
| Forms infrastructure | Upload | custom overrides | `_pickers.css`, `_inputs.css` |
| Forms infrastructure | Dropzone | variables-only | variable mapping only |
| Validators | RequiredValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Validators | LengthValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Validators | NumericRangeValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Validators | CompareValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Validators | EmailValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Validators | RegexValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Validators | DataAnnotationValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Validators | CustomValidator | custom overrides | `_feedback.css` + `_variables.css` |
| Data | DataGrid | custom overrides | `_datagrid.css` |
| Data | PivotDataGrid | variables-only | variable mapping only |
| Data | DataList | custom overrides | `_datagrid.css` |
| Data | DataFilter | custom overrides | `_datagrid.css` |
| Data | Pager | custom overrides | `_datagrid.css`, `_navigation.css` |
| Data | PickList | custom overrides | `_datagrid.css` |
| Data | Table | custom overrides | `_datagrid.css` |
| Data | Tree | custom overrides | `_datagrid.css` |
| Scheduling/planning | Scheduler | custom overrides | `_scheduler.css` |
| Scheduling/planning | Gantt | custom overrides | `_scheduler.css` |
| Navigation | Menu | custom overrides | `_navigation.css` |
| Navigation | PanelMenu | custom overrides | `_navigation.css` |
| Navigation | ContextMenu | custom overrides | `_navigation.css`, `_overlays.css` |
| Navigation | Tabs | custom overrides | `_navigation.css` |
| Navigation | Steps | custom overrides | `_navigation.css` |
| Navigation | Breadcrumb | custom overrides | `_navigation.css` |
| Navigation | Accordion | custom overrides | `_navigation.css` |
| Navigation | Link | variables-only | variable mapping only |
| Navigation | Login | variables-only | variable mapping only |
| Navigation | ProfileMenu | custom overrides | `_navigation.css` |
| Navigation | TOC | variables-only | variable mapping only |
| Overlays | Dialog | custom overrides | `_overlays.css` |
| Overlays | Side dialogs | custom overrides | `_overlays.css` |
| Overlays | Popup | custom overrides | `_overlays.css` |
| Overlays | Tooltip | custom overrides | `_overlays.css` |
| Overlays | Notification | custom overrides | `_overlays.css` |
| Layout | Layout | custom overrides | `_layout.css` |
| Layout | Sidebar | custom overrides | `_layout.css` |
| Layout | Panel | custom overrides | `_layout.css` |
| Layout | Card | custom overrides | `_layout.css` |
| Layout | CardGroup | custom overrides | `_layout.css` |
| Layout | Splitter | custom overrides | `_layout.css` |
| Layout | Stack | custom overrides | `_layout.css` |
| Layout | Row | custom overrides | `_layout.css` |
| Layout | Column | custom overrides | `_layout.css` |
| Layout | Carousel | custom overrides | `_layout.css` |
| Feedback | Alert | custom overrides | `_feedback.css` |
| Feedback | Badge | custom overrides | `_feedback.css` |
| Feedback | ProgressBar | custom overrides | `_feedback.css` |
| Feedback | ProgressBarCircular | custom overrides | `_feedback.css` |
| Feedback | Skeleton | custom overrides | `_feedback.css` |
| Charts | Area | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Bar | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Column | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Donut | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Line | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Pie | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Scatter | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Bubble | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Stacked variants | custom overrides | `_charts.css` + `_variables.css` |
| Charts | Sparkline | variables-only | variable mapping only |
| Charts | SpiderChart | variables-only | variable mapping only |
| Charts | SankeyDiagram | variables-only | variable mapping only |
| Charts | Trends/annotations | variables-only | variable mapping only |
| Gauges | ArcGauge | variables-only | gauge scale variables available in 11.1.5 |
| Gauges | RadialGauge | variables-only | gauge scale variables available in 11.1.5 |
| Gauges | LinearGauge | variables-only | gauge scale variables available in 11.1.5 |
| Display/media | Timeline | custom overrides | `_display.css` |
| Display/media | Image | custom overrides | `_display.css` |
| Display/media | Gravatar | custom overrides | `_display.css` |
| Display/media | Icon | custom overrides | `_display.css` |
| Display/media | Markdown | custom overrides | `_display.css` |
| Display/media | HtmlEditor | custom overrides | `_display.css` |
| Display/media | Chat | variables-only | variable mapping only |
| Display/media | AIChat | variables-only | variable mapping only |
| Embed/passthrough | QRCode | not applicable | container-level theme only |
| Embed/passthrough | Barcode | not applicable | container-level theme only |
| Embed/passthrough | GoogleMap | not applicable | external map content |
| Embed/passthrough | SSRSViewer | not applicable | external report renderer |
| Embed/passthrough | Spreadsheet | not applicable | interactive external surface |
| Embed/passthrough | Document Processing components | not in installed version | no public Radzen Blazor component types in 11.1.5 |
| UI fundamentals | `.rz-text-*` typography | variables-only | remapped text/baseline color vars |
| UI fundamentals | utility colors | variables-only | remapped semantic vars |
| UI fundamentals | borders | variables-only | remapped border vars |
| UI fundamentals | shadows | variables-only | remapped shadow vars |

## Unreached Styling Flags

The following items are flagged for future deep-dive because visual behavior can depend on runtime-generated markup and JS state transitions not fully asserted by static CSS inspection:
- SpeechToTextButton busy/listening states
- Chat and AIChat message role variants
- Spreadsheet internal grid parts
- SSRS viewer internals

These are currently covered at variable/container level and included in catalog/coverage for upgrade regression tracking.
