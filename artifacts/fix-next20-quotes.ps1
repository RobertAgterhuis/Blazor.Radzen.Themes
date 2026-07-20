$ErrorActionPreference = 'Stop'

$targets = @(
  'DataFilterItem','DataFilterProperty','DataFilter','DataListRow','Table','Treemap','ArcGauge',
  'AreaSeries','BarSeries','BoxPlotSeries','BubbleSeries','BulletSeries','CandlestickSeries','Chart',
  'ColumnSeries','ContourSeries','DonutSeries','FullStackedAreaSeries','FullStackedBarSeries','FullStackedColumnSeries'
)

$files = @()
foreach ($target in $targets) {
  $examplePath = "samples/Agterhuis.Ui.Demo/Components/Pages/Catalog/Examples/$target"
  if (Test-Path $examplePath) {
    $files += Get-ChildItem $examplePath -Filter '*.razor' -File
  }

  $page = "samples/Agterhuis.Ui.Demo/Components/Pages/Catalog/Catalog${target}Page.razor"
  if (Test-Path $page) {
    $files += Get-Item $page
  }
}

foreach ($file in $files) {
  $content = Get-Content $file.FullName -Raw
  $fixed = $content -replace '""', '"'
  if ($fixed -ne $content) {
    Set-Content -Path $file.FullName -Value $fixed -Encoding utf8
  }
}

Write-Host "Fixed quote escaping in $($files.Count) files."
