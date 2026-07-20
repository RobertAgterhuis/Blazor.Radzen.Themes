# Radzen Component Inventory

[Docs Index](README.md)

Source assembly: local NuGet cache (`radzen.blazor` 11.1.5)
Radzen.Blazor version: 11.1.5
Generated: 2026-07-17 12:39:02

Total components: 420
Missing demos: 0
Theme status complete: 420
Theme status pending: 0

## Demo page template rollout (Prompt 43, current state)

This iteration introduces the reusable demo template framework and migrates one hundred sixty-one standalone component pages plus the DataGrid flagship page.

| Category | Standalone components in inventory | Migrated to new template | Coverage |
|---|---:|---:|---:|
| Data & Scheduling | 10 | 10 | 100% |
| Data Visualization | 40 | 40 | 100% |
| Feedback & Overlays | 10 | 10 | 100% |
| Forms & Inputs | 29 | 29 | 100% |
| Navigation & Actions | 17 | 17 | 100% |
| Layout & Display | 24 | 24 | 100% |
| Misc | 31 | 31 | 100% |
| **Total standalone inventory** | **161** | **161** | **100%** |

Additional flagship page migrated: `RadzenDataGrid` (inventory marks this as Child, but now has dedicated page route `/catalog/data-grid`).

| Component | Category | Standalone/Child | Demo page | Theme status | Wrapper | Demopagina-sjabloon | Voorbeelden (n) |
|---|---|---|---|---|---|---|---|
| GanttTimelineView`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| InsertTableDialog | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataFilterItem`1 | Data & Scheduling | Child | child of RadzenDataFilter`1 (/catalog/data-filter) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataFilterProperty`1 | Data & Scheduling | Child | child of RadzenDataFilter`1 (/catalog/data-filter) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataFilter`1 | Data & Scheduling | Standalone | /catalog/data-filter | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenDataGridColumn`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataGridFooterCell`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataGridGroupFooterCell`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataGridGroupFooterRow`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataGridGroupRow`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataGridHeaderCell`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataGridRow`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataGrid`1 | Data & Scheduling | Child | child of RadzenDataGrid (/catalog/data-advanced) | Plum Ink complete | AgtDataGrid | ✓ |  |
| RadzenDataListRow`1 | Data & Scheduling | Child | child of RadzenDataList`1 (/catalog/data-list) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDataList`1 | Data & Scheduling | Standalone | /catalog/data-list | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenGanttColumn`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenGanttDayView`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenGanttMonthView`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenGanttWeekView`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenGanttYearView`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenGantt`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorTable | Data & Scheduling | Child | child of RadzenHtmlEditor (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorTableTools | Data & Scheduling | Child | child of RadzenHtmlEditor (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenPager | Data & Scheduling | Standalone | /catalog/pager | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenPivotDataGrid`1 | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenScheduler`1 | Data & Scheduling | Standalone | /catalog/data-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenSpreadsheetInsertTable | Data & Scheduling | Child | child of RadzenSpreadsheet (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetTableDesignToolset | Data & Scheduling | Child | child of RadzenSpreadsheet (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenTable | Data & Scheduling | Standalone | /catalog/table | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenTableBody | Data & Scheduling | Child | /catalog/data-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenTableCell | Data & Scheduling | Child | /catalog/data-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenTableHeader | Data & Scheduling | Child | /catalog/data-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenTableHeaderCell | Data & Scheduling | Child | /catalog/data-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenTableHeaderRow | Data & Scheduling | Child | /catalog/data-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenTableRow | Data & Scheduling | Child | /catalog/data-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenTree | Data & Scheduling | Standalone | /catalog/tree | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenTreeItem | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenTreeLevel | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenTreemap | Data & Scheduling | Standalone | /catalog/treemap | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| TableDesignPanel | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| TableFrame | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| TableFrameItem | Data & Scheduling | Child | child of Radzen component family (/catalog/data-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ChartOverlay | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ChartSharedTooltip | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ChartSharedTooltipItem | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ChartTooltip | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| EditChartDialog | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| GaugeBand | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| GaugePointer | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| GaugeScale | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| LinearGaugeBand | Data Visualization | Child | child of RadzenLinearGauge (/catalog/gauges) | Plum Ink complete | Raw Radzen |  |  |
| LinearGaugePointer | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| LinearGaugeScaleRenderer | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenArcGauge | Data Visualization | Standalone | /catalog/arc-gauge | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenArcGaugeScale | Data Visualization | Child | /catalog/gauges | Plum Ink complete | Raw Radzen |  |  |
| RadzenArcGaugeScaleValue | Data Visualization | Child | /catalog/gauges | Plum Ink complete | Raw Radzen |  |  |
| RadzenAreaSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenAreaSeries`1 | Data Visualization | Standalone | /catalog/area-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenBarSeries`1 | Data Visualization | Standalone | /catalog/bar-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenBoxPlotSeries`1 | Data Visualization | Standalone | /catalog/box-plot-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenBubbleSeries`1 | Data Visualization | Standalone | /catalog/bubble-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenBulletSeries`1 | Data Visualization | Standalone | /catalog/bullet-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenCandlestickSeries`1 | Data Visualization | Standalone | /catalog/candlestick-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenChart | Data Visualization | Standalone | /catalog/chart | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenColumnSeries`1 | Data Visualization | Standalone | /catalog/column-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenContourSeries`1 | Data Visualization | Standalone | /catalog/contour-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenDonutSeries`1 | Data Visualization | Standalone | /catalog/donut-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenFullStackedAreaSeries`1 | Data Visualization | Standalone | /catalog/full-stacked-area-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenFullStackedBarSeries`1 | Data Visualization | Standalone | /catalog/full-stacked-bar-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenFullStackedColumnSeries`1 | Data Visualization | Standalone | /catalog/full-stacked-column-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenFullStackedBarSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenFullStackedColumnSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenFullStackedLineSeries`1 | Data Visualization | Standalone | /catalog/full-stacked-line-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenFunnelSeries`1 | Data Visualization | Standalone | /catalog/funnel-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenHeatmapSeries`1 | Data Visualization | Standalone | /catalog/heatmap-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenHighLowSeries`1 | Data Visualization | Standalone | /catalog/high-low-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenHorizontalWaterfallSeries`1 | Data Visualization | Standalone | /catalog/horizontal-waterfall-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenLineSeries`1 | Data Visualization | Standalone | /catalog/line-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenLinearGauge | Data Visualization | Standalone | /catalog/linear-gauge | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenLinearGaugeScale | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenLinearGaugeScalePointer | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenLinearGaugeScaleRange | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenOhlcSeries`1 | Data Visualization | Standalone | /catalog/ohlc-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenPieSeries`1 | Data Visualization | Standalone | /catalog/pie-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenPyramidSeries`1 | Data Visualization | Standalone | /catalog/pyramid-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenRadialGauge | Data Visualization | Standalone | /catalog/radial-gauge | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenRadialGaugeScale | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenRadialGaugeScalePointer | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenRadialGaugeScaleRange | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenRangeAreaSeries`1 | Data Visualization | Standalone | /catalog/range-area-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenRangeBarSeries`1 | Data Visualization | Standalone | /catalog/range-bar-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenRangeColumnSeries`1 | Data Visualization | Standalone | /catalog/range-column-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenRangeNavigatorLineSeries`1 | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSankeyDiagram`1 | Data Visualization | Standalone | /catalog/sankey-diagram | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenScatterSeries`1 | Data Visualization | Standalone | /catalog/scatter-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenSeriesAnnotation`1 | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesDataLabels | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesMeanLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesMedianLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesModeLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesMovingAverageLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesPolynomialTrendLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesReferenceBand | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesReferenceLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesTrendLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesValueLabel | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSeriesValueLine | Data Visualization | Child | child of RadzenChart (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSparkline | Data Visualization | Standalone | /catalog/sparkline | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenSpiderChart | Data Visualization | Standalone | /catalog/spider-chart | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenSpiderColumnSeries`1 | Data Visualization | Standalone | /catalog/spider-column-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenSpiderSeries`1 | Data Visualization | Standalone | /catalog/spider-series | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenSpreadsheetInsertChart | Data Visualization | Child | child of RadzenSpreadsheet (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenStackedAreaSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenStackedBarSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenStackedColumnSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenStackedLineSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenTimeline | Data Visualization | Standalone | /catalog/display | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenTimelineItem | Data Visualization | Child | /catalog/display | Plum Ink complete | Raw Radzen |  |  |
| RadzenWaterfallSeries`1 | Data Visualization | Standalone | /catalog/charts-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenYearTimelineView | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| YearTimelineView | Data Visualization | Child | child of Radzen component family (/catalog/charts-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ConditionalFormatDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| CustomSortDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| DataValidationDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| DialogContainer | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| DrawingSizeDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| FilterDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| FormatCellsDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| HyperlinkDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenAIChat | Feedback & Overlays | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenAlert | Feedback & Overlays | Standalone | /catalog/feedback | Plum Ink complete | AgtAlert | ✓ |  |
| RadzenChat | Feedback & Overlays | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenDialog | Feedback & Overlays | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenLogin | Feedback & Overlays | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenNotification | Feedback & Overlays | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenNotificationMessage | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenPopup | Feedback & Overlays | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenProgressBar | Feedback & Overlays | Standalone | /catalog/feedback | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenProgressBarCircular | Feedback & Overlays | Standalone | /catalog/feedback | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenSkeleton | Feedback & Overlays | Standalone | /catalog/feedback | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenTooltip | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RenameSheetDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| SpreadsheetShortcutsDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| TooltipOverlay | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| Top10FilterDialog | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ValidationListPopup | Feedback & Overlays | Child | child of Radzen component family (/catalog/overlays-advanced) | Plum Ink complete | Raw Radzen |  |  |
| DropDownBase`1 | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| EditorColorPicker | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| EditorDropDown | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| EditorDropDownItem | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| FormComponentWithAutoComplete`1 | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenAutoComplete | Forms & Inputs | Standalone | /catalog/text-inputs | Plum Ink complete | AgtAutoComplete<TItem> | ✓ |  |
| RadzenCheckBoxListItem`1 | Forms & Inputs | Standalone | /catalog/checkbox-list | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenCheckBoxList`1 | Forms & Inputs | Standalone | /catalog/checkbox-list | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenCheckBox`1 | Forms & Inputs | Standalone | /catalog/forms-advanced | Plum Ink complete | AgtCheckbox | ✓ |  |
| RadzenChip | Forms & Inputs | Standalone | /catalog/chip | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenChipItem | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenChipList`1 | Forms & Inputs | Standalone | /catalog/chip-list | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenColorPicker | Forms & Inputs | Standalone | /catalog/color-picker | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenColorPickerItem | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDatePicker`1 | Forms & Inputs | Standalone | /catalog/forms-advanced | Plum Ink complete | AgtDatePicker | ✓ |  |
| RadzenDropDownDataGridColumn | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDropDownDataGrid`1 | Forms & Inputs | Standalone | /catalog/dropdown-data-grid | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenDropDownItem`1 | Forms & Inputs | Standalone | /catalog/forms-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenDropDown`1 | Forms & Inputs | Standalone | /catalog/forms-advanced | Plum Ink complete | AgtDropdown | ✓ |  |
| RadzenDropZoneContainer`1 | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDropZoneItem`1 | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenDropZone`1 | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenFileInput`1 | Forms & Inputs | Standalone | /catalog/forms-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenListBoxItem`1 | Forms & Inputs | Standalone | /catalog/list-box | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenListBox`1 | Forms & Inputs | Standalone | /catalog/list-box | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenMask | Forms & Inputs | Standalone | /catalog/mask | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenNumericRangeValidator | Forms & Inputs | Standalone | /catalog/numeric-range-validator | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenNumeric`1 | Forms & Inputs | Standalone | /catalog/forms-advanced | Plum Ink complete | AgtNumericField | ✓ |  |
| RadzenPassword | Forms & Inputs | Standalone | /catalog/text-inputs | Plum Ink complete | AgtPassword | ✓ |  |
| RadzenPickList`1 | Forms & Inputs | Standalone | /catalog/pick-list | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenRating | Forms & Inputs | Standalone | /catalog/rating | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenSecurityCode | Forms & Inputs | Standalone | /catalog/security-code | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenSlider`1 | Forms & Inputs | Standalone | /catalog/slider | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenSpreadsheetTextAlign | Forms & Inputs | Child | child of RadzenSpreadsheet (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetTextWrap | Forms & Inputs | Child | child of RadzenSpreadsheet (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSwitch | Forms & Inputs | Standalone | /catalog/selection-inputs | Plum Ink complete | AgtSwitch | ✓ |  |
| RadzenText | Forms & Inputs | Standalone | /catalog/text | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenTextArea | Forms & Inputs | Standalone | /catalog/text-inputs | Plum Ink complete | AgtTextArea | ✓ |  |
| RadzenTextBox | Forms & Inputs | Standalone | /catalog/forms-advanced | Plum Ink complete | AgtTextField | ✓ |  |
| RadzenTimeSpanPicker`1 | Forms & Inputs | Standalone | /catalog/time-span-picker | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenUpload | Forms & Inputs | Standalone | /catalog/upload | Plum Ink complete | AgtFileUpload | ✓ | 3 |
| RadzenUploadHeader | Forms & Inputs | Standalone | /catalog/upload | Plum Ink complete | Raw Radzen | ✓ | 3 |
| Text | Forms & Inputs | Child | child of Radzen component family (/catalog/forms-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ColumnHeader | Layout & Display | Child | child of Radzen component family (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| ImageOverlay | Layout & Display | Child | child of Radzen component family (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenBarcode | Layout & Display | Standalone | /catalog/barcode | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenBody | Layout & Display | Child | child of RadzenLayout (/catalog/layout-component) | Plum Ink complete | Raw Radzen |  |  |
| RadzenCard | Layout & Display | Standalone | /catalog/card | Plum Ink complete | AgtCard | ✓ | 3 |
| RadzenCardGroup | Layout & Display | Standalone | /catalog/card-group | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenCarousel | Layout & Display | Standalone | /catalog/layout | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenCarouselItem | Layout & Display | Child | /catalog/layout | Plum Ink complete | Raw Radzen |  |  |
| RadzenColumn | Layout & Display | Child | /catalog/layout | Plum Ink complete | Raw Radzen |  |  |
| RadzenColumnOptions | Layout & Display | Child | child of RadzenDataGrid`1 (/catalog/data-grid) | Plum Ink complete | Raw Radzen |  |  |
| RadzenFieldset | Layout & Display | Standalone | /catalog/fieldset | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenFooter | Layout & Display | Child | child of RadzenLayout (/catalog/layout-component) | Plum Ink complete | Raw Radzen |  |  |
| RadzenGravatar | Layout & Display | Standalone | /catalog/gravatar | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenGridRow | Layout & Display | Child | child of RadzenDataGrid`1 (/catalog/data-grid) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHeader | Layout & Display | Child | child of RadzenLayout (/catalog/layout-component) | Plum Ink complete | AgtSidebarLayout |  |  |
| RadzenHtmlEditor | Layout & Display | Standalone | /catalog/html-editor | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenHtmlEditorAlignCenter | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorAlignLeft | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorAlignRight | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorBackground | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorBackgroundItem | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorBold | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorColor | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorColorItem | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorCustomTool | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorFontName | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorFontNameItem | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorFontSize | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorFormatBlock | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorImage | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorIndent | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorItalic | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorJustify | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorOrderedList | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorOutdent | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorRedo | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorRemoveFormat | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorSeparator | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorSource | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorStrikeThrough | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorSubscript | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorSuperscript | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorUnderline | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorUndo | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorUnlink | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorUnorderedList | Layout & Display | Child | child of RadzenHtmlEditor (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenIcon | Layout & Display | Standalone | /catalog/icon | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenImage | Layout & Display | Standalone | /catalog/image | Plum Ink complete | Raw Radzen | ✓ | 1 (single-capability) |
| RadzenLayout | Layout & Display | Standalone | /catalog/layout-component | Plum Ink complete | AgtSidebarLayout | ✓ | 3 |
| RadzenMarkdown | Layout & Display | Standalone | /catalog/markdown | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenPanel | Layout & Display | Standalone | /catalog/layout | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenPivotColumn`1 | Layout & Display | Child | child of Radzen component family (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenPivotRow`1 | Layout & Display | Child | child of Radzen component family (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenQRCode | Layout & Display | Standalone | /catalog/qr-code | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenRow | Layout & Display | Standalone | /catalog/layout | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenSidebar | Layout & Display | Standalone | /catalog/sidebar | Plum Ink complete | AgtSidebarLayout | ✓ | 3 |
| RadzenSidebarToggle | Layout & Display | Standalone | /catalog/sidebar-toggle | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenSplitter | Layout & Display | Standalone | /catalog/splitter | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenSplitterPane | Layout & Display | Child | child of RadzenSplitter (/catalog/splitter) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetInsertColumnAfter | Layout & Display | Child | child of RadzenSpreadsheet (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetInsertColumnBefore | Layout & Display | Child | child of RadzenSpreadsheet (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetInsertImage | Layout & Display | Child | child of RadzenSpreadsheet (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetInsertRowAfter | Layout & Display | Child | child of RadzenSpreadsheet (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetInsertRowBefore | Layout & Display | Child | child of RadzenSpreadsheet (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| RadzenStack | Layout & Display | Standalone | /catalog/stack | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RowHeader | Layout & Display | Child | child of Radzen component family (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| Splitter | Layout & Display | Child | child of RadzenSplitter (/catalog/layout-advanced) | Plum Ink complete | Raw Radzen |  |  |
| AgendaSlotEvents | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| AgendaView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Appointment | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| AutofillItem | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| AutofillOverlay | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CategoryAxis | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CategoryAxisTick | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CategoryAxisTitle | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CellEditor | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CellSelection | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CellSelectionItem | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CellView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| ClipPath | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CornerHeaderCell | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| DataBoundFormComponent`1 | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| DaySlotEvents | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| DayView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Draggable | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Expander | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| FormComponent`1 | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| FormulaEditor | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| FunctionHint | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| GradientDefs | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Hours | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| HoverOverlay | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| InputPrompt | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Legend | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| LegendItem | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Line | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| LinearGradientDef | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Marker | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Markers`1 | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| MonthView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| PagedDataBoundComponent`1 | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| Path | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadialGradientDef | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenAgendaView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenAppearanceToggle | Misc | Standalone | /catalog/appearance-toggle | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenAxisCrosshair | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenAxisTitle | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenBadge | Misc | Standalone | /catalog/badge | Plum Ink complete | AgtBadge | ✓ | 3 |
| RadzenBarOptions | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenCategoryAxis | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenCompareValidator | Misc | Standalone | /catalog/compare-validator | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenComponent | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenComponentWithChildren | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenComponents | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenContent | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenContentContainer | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenCustomValidator | Misc | Standalone | /catalog/custom-validator | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenDataAnnotationValidator | Misc | Standalone | /catalog/data-annotation-validator | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenDayView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenEmailValidator | Misc | Standalone | /catalog/email-validator | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenFlexComponent | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenFormField | Misc | Standalone | /catalog/form-field | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenGoogleMap | Misc | Standalone | /catalog/google-map | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenGoogleMapMarker | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenGridLines | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenHeading | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenHeatmap | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenLabel | Misc | Child | /catalog/forms | Plum Ink complete | Raw Radzen |  |  |
| RadzenLegend | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenLengthValidator | Misc | Standalone | /catalog/length-validator | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenLiveRegion | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenMarkers | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenMediaQuery | Misc | Standalone | /catalog/media-query | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenMonthView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenMultiDayView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenPivotAggregate`1 | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenPivotField`1 | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenRangeNavigator | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenRegexValidator | Misc | Standalone | /catalog/regex-validator | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenRequiredValidator | Misc | Standalone | /catalog/forms-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenSSRSViewer | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSSRSViewerParameter | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSelectBarItem | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSelectBar`1 | Misc | Standalone | /catalog/select-bar | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenSignaturePad | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenSpiderLegend | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheet | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetAutoFilter | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetBackgroundColor | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetBold | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetCellBorders | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetColor | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetConditionalFormat | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetCustomSort | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetDataFormat | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetDataValidation | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetDecreaseDecimals | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetFontFamily | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetFontSize | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetFreeze | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetIncreaseDecimals | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetInsertHyperlink | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetItalic | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetMergeCells | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetOpen | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetRedo | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetSave | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetStrikethrough | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetUnderline | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetUndo | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenSpreadsheetVerticalAlign | Misc | Child | child of RadzenSpreadsheet (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenTemplateForm`1 | Misc | Standalone | /catalog/template-form | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenTheme | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenTicks | Misc | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenTileLayout | Misc | Standalone | /catalog/tile-layout | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenTileLayoutItem | Misc | Child | /catalog/layout-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenToc | Misc | Standalone | /catalog/toc | Plum Ink complete | Raw Radzen | ✓ | 3 |
| RadzenTocItem | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenValueAxis | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenWeekView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenYearPlannerView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RadzenYearView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RangePicker | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RangePickerBar | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| RangeSelectionItem | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| SelectionOverlay | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| SheetEditor | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| SheetEditorHighlight | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| SpiderLegend | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| SpreadsheetAccessibility | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| ValidationError | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| ValueAxis | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| ValueAxisTick | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| ValueAxisTitle | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| VirtualGrid | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| WeekView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| YearPlannerView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| YearView | Misc | Child | child of Radzen component family (/catalog/all-components) | Plum Ink complete | Raw Radzen |  |  |
| CellMenu | Navigation & Actions | Child | child of Radzen component family (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| CellMenuItem | Navigation & Actions | Child | child of Radzen component family (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| EditorButton | Navigation & Actions | Child | child of Radzen component family (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| RadzenAccordion | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenAccordionItem | Navigation & Actions | Child | child of Radzen component family (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| RadzenBreadCrumb | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | AgtBreadcrumb | ✓ |  |
| RadzenBreadCrumbItem | Navigation & Actions | Child | /catalog/navigation | Plum Ink complete | AgtBreadcrumb |  |  |
| RadzenButton | Navigation & Actions | Standalone | /catalog/buttons | Plum Ink complete | AgtPrimaryButton/AgtSecondaryButton | ✓ |  |
| RadzenContextMenu | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenDataGridFilterMenu`1 | Navigation & Actions | Child | child of RadzenDataGrid (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| RadzenFab | Navigation & Actions | Standalone | /catalog/buttons | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenFabMenu | Navigation & Actions | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenFabMenuItem | Navigation & Actions | Child | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorLink | Navigation & Actions | Child | child of RadzenHtmlEditor (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| RadzenHtmlEditorTableCommandButton | Navigation & Actions | Child | child of RadzenHtmlEditor (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| RadzenLink | Navigation & Actions | Standalone | /catalog/all-components | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenMenu | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenMenuItem | Navigation & Actions | Child | /catalog/navigation | Plum Ink complete | Raw Radzen |  |  |
| RadzenMenuItemWrapper | Navigation & Actions | Child | child of Radzen component family (/catalog/navigation) | Plum Ink complete | Raw Radzen |  |  |
| RadzenPanelMenu | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenPanelMenuItem | Navigation & Actions | Child | /catalog/navigation | Plum Ink complete | Raw Radzen |  |  |
| RadzenProfileMenu | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenProfileMenuItem | Navigation & Actions | Child | /catalog/navigation | Plum Ink complete | Raw Radzen |  |  |
| RadzenRadioButtonListItem`1 | Navigation & Actions | Child | child of RadzenRadioButtonList`1 (/catalog/radio-button-list) | Plum Ink complete | Raw Radzen |  |  |
| RadzenRadioButtonList`1 | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | AgtRadioList<TValue> | ✓ |  |
| RadzenSpeechToTextButton | Navigation & Actions | Standalone | /catalog/overlays-advanced | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenSplitButton | Navigation & Actions | Standalone | /catalog/buttons | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenSplitButtonItem | Navigation & Actions | Child | /catalog/buttons | Plum Ink complete | Raw Radzen |  |  |
| RadzenSteps | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | Raw Radzen | ✓ |  |
| RadzenStepsItem | Navigation & Actions | Child | /catalog/navigation | Plum Ink complete | Raw Radzen |  |  |
| RadzenTabs | Navigation & Actions | Standalone | /catalog/navigation | Plum Ink complete | AgtTabs | ✓ |  |
| RadzenTabsItem | Navigation & Actions | Child | /catalog/navigation | Plum Ink complete | AgtTabItem |  |  |
| RadzenToggleButton | Navigation & Actions | Standalone | /catalog/buttons | Plum Ink complete | Raw Radzen | ✓ |  |

