$ErrorActionPreference = 'Stop'

$base = 'samples/Agterhuis.Ui.Demo/Components/Pages/Catalog'
$examplesBase = Join-Path $base 'Examples'

$components = @(
    @{ Name='DataFilterItem'; Route='data-filter-item'; Kicker='Data & Scheduling'; Intro='Filter-item scenarios binnen datafilter-workflows.'; Doc='https://blazor.radzen.com/datafilter'; Kind='DataFilter' },
    @{ Name='DataFilterProperty'; Route='data-filter-property'; Kicker='Data & Scheduling'; Intro='Property-definities voor filterbare datavelden.'; Doc='https://blazor.radzen.com/datafilter'; Kind='DataFilter' },
    @{ Name='DataFilter'; Route='data-filter'; Kicker='Data & Scheduling'; Intro='Interactieve dataselectie met conditionele regels.'; Doc='https://blazor.radzen.com/datafilter'; Kind='DataFilter' },
    @{ Name='DataListRow'; Route='data-list-row'; Kicker='Data & Scheduling'; Intro='Rijtemplate-patronen binnen datalijsten.'; Doc='https://blazor.radzen.com/datalist'; Kind='DataList' },
    @{ Name='Table'; Route='table'; Kicker='Data & Scheduling'; Intro='Semantische tabelopmaak voor compacte datasets.'; Doc='https://blazor.radzen.com/table'; Kind='Table' },
    @{ Name='Treemap'; Route='treemap'; Kicker='Data & Scheduling'; Intro='Hiërarchische waardevisualisatie als blokdiagram.'; Doc='https://blazor.radzen.com/treemap'; Kind='Treemap' },
    @{ Name='ArcGauge'; Route='arc-gauge'; Kicker='Data Visualization'; Intro='Booggauge voor KPI-indicatoren met schaalwaarde.'; Doc='https://blazor.radzen.com/gauge'; Kind='ArcGauge' },
    @{ Name='AreaSeries'; Route='area-series'; Kicker='Data Visualization'; Intro='Area-series binnen lijn/gebiedsgrafieken.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='BarSeries'; Route='bar-series'; Kicker='Data Visualization'; Intro='Horizontale bars voor vergelijkende metingen.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='BoxPlotSeries'; Route='box-plot-series'; Kicker='Data Visualization'; Intro='Spreidingsvisualisatie met mediaan en kwartielen.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='BubbleSeries'; Route='bubble-series'; Kicker='Data Visualization'; Intro='Bubbels met extra dimensie via grootte.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='BulletSeries'; Route='bullet-series'; Kicker='Data Visualization'; Intro='Bullet-chart weergave voor target vs actual.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='CandlestickSeries'; Route='candlestick-series'; Kicker='Data Visualization'; Intro='OHLC-stijl visualisatie voor trendgegevens.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='Chart'; Route='chart'; Kicker='Data Visualization'; Intro='Basischartcontainer met meerdere series.'; Doc='https://blazor.radzen.com/chart'; Kind='Chart' },
    @{ Name='ColumnSeries'; Route='column-series'; Kicker='Data Visualization'; Intro='Kolomseries voor categorische vergelijking.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='ContourSeries'; Route='contour-series'; Kicker='Data Visualization'; Intro='Contourweergave voor intensiteitspatronen.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='DonutSeries'; Route='donut-series'; Kicker='Data Visualization'; Intro='Donutverdeling met proportionele segmenten.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='FullStackedAreaSeries'; Route='full-stacked-area-series'; Kicker='Data Visualization'; Intro='100%-gestapelde area-series voor aandeelvergelijking.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='FullStackedBarSeries'; Route='full-stacked-bar-series'; Kicker='Data Visualization'; Intro='100%-gestapelde bar-series voor aandeelvergelijking.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' },
    @{ Name='FullStackedColumnSeries'; Route='full-stacked-column-series'; Kicker='Data Visualization'; Intro='100%-gestapelde kolomseries per categorie.'; Doc='https://blazor.radzen.com/chart'; Kind='Series' }
)

function Get-ExampleContent {
    param([string]$Name, [string]$Kind)

    switch ($Kind) {
        'DataFilter' {
@'
<RadzenDataFilter TItem="FilterRow" Data="Rows" FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive" />

@code {
    private static readonly IReadOnlyList<FilterRow> Rows =
    [
        new("Alpha", "Noord", 12),
        new("Bravo", "West", 20),
        new("Charlie", "Zuid", 15)
    ];

    private sealed record FilterRow(string Name, string Team, int Score);
}
'@
        }
        'DataList' {
@'
<RadzenDataList TItem="DataRow" Data="Rows">
    <Template Context="row">
        <RadzenCard>@row.Name - @row.Score</RadzenCard>
    </Template>
</RadzenDataList>

@code {
    private static readonly IReadOnlyList<DataRow> Rows =
    [
        new("Alpha", 10),
        new("Bravo", 20),
        new("Charlie", 30)
    ];

    private sealed record DataRow(string Name, int Score);
}
'@
        }
        'Table' {
@'
<RadzenTable>
    <RadzenTableHeader>
        <RadzenTableHeaderRow>
            <RadzenTableHeaderCell>Naam</RadzenTableHeaderCell>
            <RadzenTableHeaderCell>Score</RadzenTableHeaderCell>
        </RadzenTableHeaderRow>
    </RadzenTableHeader>
    <RadzenTableBody>
        @foreach (var row in Rows)
        {
            <RadzenTableRow>
                <RadzenTableCell>@row.Name</RadzenTableCell>
                <RadzenTableCell>@row.Score</RadzenTableCell>
            </RadzenTableRow>
        }
    </RadzenTableBody>
</RadzenTable>

@code {
    private static readonly IReadOnlyList<TableRow> Rows =
    [
        new("Alpha", 10),
        new("Bravo", 20),
        new("Charlie", 30)
    ];

    private sealed record TableRow(string Name, int Score);
}
'@
        }
        'Treemap' {
@'
<RadzenCard>
    <strong>Treemap host</strong>
    <p class="agt-muted">Treemap rendering wordt in deze batch als dedicated route ontsloten.</p>
</RadzenCard>
'@
        }
        'ArcGauge' {
@'
<RadzenArcGauge>
    <RadzenArcGaugeScale Min="0" Max="100">
        <RadzenArcGaugeScaleValue Value="72" />
    </RadzenArcGaugeScale>
</RadzenArcGauge>
'@
        }
        'Chart' {
@'
<RadzenChart Style="height: 18rem;">
    <RadzenColumnSeries Data="Rows" CategoryProperty="Label" ValueProperty="ValueA" />
    <RadzenLineSeries Data="Rows" CategoryProperty="Label" ValueProperty="ValueB" />
</RadzenChart>

@code {
    private static readonly IReadOnlyList<ChartRow> Rows =
    [
        new("Jan", 12, 8),
        new("Feb", 16, 10),
        new("Mar", 14, 12)
    ];

    private sealed record ChartRow(string Label, double ValueA, double ValueB);
}
'@
        }
        'Series' {
            $seriesTag = "Radzen$Name"
@"
<RadzenChart Style=""height: 18rem;"">
    <$seriesTag Data=""Rows"" CategoryProperty=""Label"" ValueProperty=""Value"" />
</RadzenChart>

@code {
    private static readonly IReadOnlyList<SeriesRow> Rows =
    [
        new(""Jan"", 12),
        new(""Feb"", 16),
        new(""Mar"", 14)
    ];

    private sealed record SeriesRow(string Label, double Value);
}
"@
        }
        default {
            "<RadzenCard>$Name</RadzenCard>"
        }
    }
}

foreach ($component in $components) {
    $name = $component.Name
    $route = $component.Route
    $kicker = $component.Kicker
    $intro = $component.Intro
    $doc = $component.Doc
    $kind = $component.Kind

    $exampleFolder = Join-Path $examplesBase $name
    New-Item -ItemType Directory -Path $exampleFolder -Force | Out-Null

    foreach ($variant in @('Basic', 'Advanced', 'Compact')) {
        $examplePath = Join-Path $exampleFolder ("{0}{1}Example.razor" -f $name, $variant)
        $exampleContent = Get-ExampleContent -Name $name -Kind $kind
        Set-Content -Path $examplePath -Value $exampleContent -Encoding utf8
    }

    $pagePath = Join-Path $base ("Catalog{0}Page.razor" -f $name)
    $pageContent = @"
@page "/catalog/$route"
@using Agterhuis.Ui.Demo.Components.Pages.Catalog.Examples.$name

<PageTitle>$name (Radzen)</PageTitle>

<ComponentPage Kicker="$kicker"
               Title="$name"
               Introduction="$intro"
               RadzenDocumentationUrl="$doc">
    <DemoExample Title="Basis"
                 Description="Basisscenario voor $name."
                 ExampleComponentType="typeof(${name}BasicExample)"
                 SourcePath="Components/Pages/Catalog/Examples/$name/${name}BasicExample.razor" />

    <DemoExample Title="Uitgebreid"
                 Description="Uitgebreider scenario voor $name."
                 ExampleComponentType="typeof(${name}AdvancedExample)"
                 SourcePath="Components/Pages/Catalog/Examples/$name/${name}AdvancedExample.razor" />

    <DemoExample Title="Compact"
                 Description="Compact scenario voor $name."
                 ExampleComponentType="typeof(${name}CompactExample)"
                 SourcePath="Components/Pages/Catalog/Examples/$name/${name}CompactExample.razor" />
</ComponentPage>
"@
    Set-Content -Path $pagePath -Value $pageContent -Encoding utf8
}

Write-Host 'Generated pages and examples for next 20 components.'
