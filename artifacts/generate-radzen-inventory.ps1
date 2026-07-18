$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$pkgProps = Join-Path $repo 'Directory.Packages.props'
[xml]$xml = Get-Content -Raw $pkgProps
$radzenVersion = ($xml.Project.ItemGroup.PackageVersion | Where-Object { $_.Include -eq 'Radzen.Blazor' }).Version
$nuget = Join-Path $env:USERPROFILE ".nuget\packages\radzen.blazor\$radzenVersion\lib"
$asmPath = (Get-ChildItem $nuget -Recurse -Filter 'Radzen.Blazor.dll' | Sort-Object FullName -Descending | Select-Object -First 1).FullName

$aspComponents = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'Microsoft.AspNetCore.Components' } | Select-Object -First 1
if (-not $aspComponents) {
    $sharedRoot = Join-Path $env:ProgramFiles 'dotnet\shared\Microsoft.AspNetCore.App'
    $latestAsp = Get-ChildItem $sharedRoot -Directory | Sort-Object Name -Descending | Select-Object -First 1
    $aspPath = Join-Path $latestAsp.FullName 'Microsoft.AspNetCore.Components.dll'
    $aspComponents = [System.Reflection.Assembly]::LoadFrom($aspPath)
}

$asm = [System.Reflection.Assembly]::LoadFrom($asmPath)
$componentBase = $aspComponents.GetType('Microsoft.AspNetCore.Components.ComponentBase', $true)
$types = $asm.GetExportedTypes() | Where-Object { -not $_.IsAbstract -and $componentBase.IsAssignableFrom($_) } | Sort-Object Name

$catalogDir = Join-Path $repo 'samples\Agterhuis.Ui.Demo\Components\Pages\Catalog'
$catalogFiles = Get-ChildItem $catalogDir -Filter '*.razor'
$catalogContent = @{}
foreach ($f in $catalogFiles) {
    $catalogContent[$f.Name] = Get-Content -Raw $f.FullName
}

$childSuffix = '(Item|Column|Series|Axis|Scale|Value|Title|Legend|Tooltip|Tick|Band|Step|StripLine|Marker|Label|DialogOptions)$'
$serviceBound = @('RadzenGoogleMap', 'RadzenSSRSViewer', 'RadzenSpeechToTextButton')

function Get-Category([string]$name) {
    switch -Regex ($name) {
        'Button|Fab|SplitButton|ProfileMenu|Menu|BreadCrumb|Tabs|Accordion|Steps|Link|ContextMenu|PanelMenu' { 'Navigation & Actions'; break }
        'Text|Numeric|Password|Mask|AutoComplete|SecurityCode|FileInput|Upload|DropDown|ListBox|DatePicker|TimeSpanPicker|ColorPicker|Rating|CheckBox|Radio|Switch|Slider|Chip|PickList|DropZone' { 'Forms & Inputs'; break }
        'DataGrid|PivotDataGrid|DataList|Table|Tree|Pager|DataFilter|Gantt|Scheduler' { 'Data & Scheduling'; break }
        'Chart|Series|Gauge|Sparkline|Sankey|Timeline' { 'Data Visualization'; break }
        'Dialog|Popup|Tooltip|Alert|Notification|Progress|Skeleton|Login|Chat|AIChat' { 'Feedback & Overlays'; break }
        'Card|Panel|Fieldset|Row|Column|Stack|Splitter|Carousel|QRCode|Barcode|Image|Icon|HtmlEditor|Markdown|Gravatar|RadzenBody|RadzenHeader|RadzenFooter|RadzenSidebar|RadzenLayout' { 'Layout & Display'; break }
        default { 'Misc' }
    }
}

$rows = @()
foreach ($t in $types) {
    $name = $t.Name
    $isChild = [bool]($name -match $childSuffix)
    $parent = ''
    if ($isChild) {
        $parent = $name -replace '(Item|Column|Series|Axis|Scale|Value|Title|Legend|Tooltip|Tick|Band|Step|StripLine|Marker|Label|DialogOptions)$', ''
    }

    $demo = ''
    foreach ($f in $catalogFiles) {
        if ($catalogContent[$f.Name] -match "<\s*$name\b") {
            $route = ((Get-Content -Raw $f.FullName) -split "`n" | Where-Object { $_ -match '^@page\s+"' } | Select-Object -First 1)
            if ($route) {
                $demo = ($route -replace '^@page\s+"', '' -replace '"\s*$', '')
            }
            break
        }
    }

    if (-not $demo -and $isChild -and $parent) {
        $candidate = 'Radzen' + $parent
        foreach ($f in $catalogFiles) {
            if ($catalogContent[$f.Name] -match "<\s*$candidate\b") {
                $route = ((Get-Content -Raw $f.FullName) -split "`n" | Where-Object { $_ -match '^@page\s+"' } | Select-Object -First 1)
                if ($route) {
                    $demo = 'child of ' + $candidate + ' (' + ($route -replace '^@page\s+"', '' -replace '"\s*$', '') + ')'
                }
                break
            }
        }
    }

    if (-not $demo -and ($serviceBound -contains $name)) {
        $demo = 'external-service component (placeholder required)'
    }

    $rows += [pscustomobject]@{
        Component   = $name
        Category    = (Get-Category $name)
        Mode        = if ($isChild) { 'Child' } else { 'Standalone' }
        Demo        = if ([string]::IsNullOrWhiteSpace($demo)) { 'MISSING' } else { $demo }
        ThemeStatus = 'Plum Ink pending'
    }
}

$out = Join-Path $repo 'docs\RADZEN-COMPONENT-INVENTORY.md'
$lines = @()
$lines += '# Radzen Component Inventory'
$lines += ''
$lines += "Source assembly: $asmPath"
$lines += "Radzen.Blazor version: $radzenVersion"
$lines += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$lines += ''
$lines += "Total components: $($rows.Count)"
$lines += "Missing demos: $((@($rows | Where-Object { $_.Demo -eq 'MISSING' })).Count)"
$lines += ''
$lines += '| Component | Category | Standalone/Child | Demo page | Theme status |'
$lines += '|---|---|---|---|---|'
foreach ($r in $rows | Sort-Object Category, Component) {
    $lines += "| $($r.Component) | $($r.Category) | $($r.Mode) | $($r.Demo) | $($r.ThemeStatus) |"
}
Set-Content -Path $out -Value $lines -Encoding UTF8

$missing = $rows | Where-Object { $_.Demo -eq 'MISSING' -and $_.Mode -eq 'Standalone' } | Select-Object -ExpandProperty Component
$missingPath = Join-Path $repo 'docs\RADZEN-MISSING-STANDALONE.txt'
Set-Content -Path $missingPath -Value ($missing -join [Environment]::NewLine) -Encoding UTF8

Write-Output "Generated: $out"
Write-Output "Missing standalone count: $($missing.Count)"
Get-Content -Path $missingPath | Select-Object -First 200
