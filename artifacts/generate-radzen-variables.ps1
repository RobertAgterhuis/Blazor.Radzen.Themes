$ErrorActionPreference = "Stop"
Set-Location "d:\repositories\Agterhuis.Ui"

$css = "C:\Users\ragterhuis\.nuget\packages\radzen.blazor\11.1.5\staticwebassets\css\material-base.css"
$allVars = Select-String -Path $css -Pattern '--rz-[a-z0-9-]+\s*:' -AllMatches |
    ForEach-Object { $_.Matches.Value } |
    ForEach-Object { ($_ -replace '\s*:$', '').Trim() } |
    Sort-Object -Unique

$includePattern = '(color|background|shadow|focus|hover|active|selected|primary|secondary|success|warning|danger|info|overlay|surface|series|gridline|stroke|fill|icon)'
$excludePattern = '(radius|size|width|height|padding|margin|font|line-height|letter-spacing|transition|duration|timing|order|gap|opacity|zindex|index|transform|translate|rotate|scale|weight|family|style)'

$colorVars = $allVars | Where-Object {
    $_ -match $includePattern -and $_ -notmatch $excludePattern
}

function Resolve-Light([string]$n) {
    if ($n -match 'on-secondary') { return 'var(--agt-color-gray-900)' }
    if ($n -match 'secondary') { return 'var(--agt-color-accent-500)' }
    if ($n -match 'on-primary') { return 'var(--agt-color-white)' }
    if ($n -match 'primary') { return 'var(--agt-color-primary-500)' }
    if ($n -match 'danger') { return 'var(--agt-color-danger)' }
    if ($n -match 'warning') { return 'var(--agt-color-warning)' }
    if ($n -match 'success') { return 'var(--agt-color-success)' }
    if ($n -match 'info') { return 'var(--agt-color-info)' }
    if ($n -match 'disabled') { return 'var(--agt-color-gray-500)' }
    if ($n -match 'focus') { return 'var(--agt-color-primary-500)' }
    if ($n -match 'hover') { return 'var(--agt-color-primary-100)' }
    if ($n -match 'active') { return 'var(--agt-color-primary-200)' }
    if ($n -match 'selected') { return 'var(--agt-color-accent-400)' }
    if ($n -match 'shadow') { return 'var(--agt-shadow-sm)' }
    if ($n -match 'base-50') { return 'var(--agt-color-gray-50)' }
    if ($n -match 'base-100') { return 'var(--agt-color-gray-100)' }
    if ($n -match 'base-(200|300|400)') { return 'var(--agt-color-gray-200)' }
    if ($n -match 'base-(500|600)') { return 'var(--agt-color-gray-500)' }
    if ($n -match 'base-(700|800)') { return 'var(--agt-color-gray-700)' }
    if ($n -match 'base-900') { return 'var(--agt-color-gray-900)' }
    if ($n -match 'border') { return 'var(--agt-color-gray-200)' }
    if ($n -match '(background|surface|overlay|panel|card|popup|dialog|item)') { return 'var(--agt-color-white)' }
    if ($n -match '(text|color|icon|label|title|caption|value|axis|gridline|stroke|fill)') { return 'var(--agt-color-gray-900)' }
    if ($n -match '(chart|series)') { return 'var(--agt-color-primary-500)' }
    return 'var(--agt-color-gray-900)'
}

function Resolve-Dark([string]$n) {
    if ($n -match 'on-secondary') { return 'var(--agt-color-gray-900)' }
    if ($n -match 'secondary') { return 'var(--agt-color-accent-400)' }
    if ($n -match 'on-primary') { return 'var(--agt-color-white)' }
    if ($n -match 'primary') { return 'var(--agt-color-primary-400)' }
    if ($n -match 'danger') { return 'var(--agt-color-danger)' }
    if ($n -match 'warning') { return 'var(--agt-color-warning)' }
    if ($n -match 'success') { return 'var(--agt-color-success)' }
    if ($n -match 'info') { return 'var(--agt-color-info)' }
    if ($n -match 'disabled') { return 'var(--agt-color-gray-500)' }
    if ($n -match 'focus') { return 'var(--agt-color-primary-300)' }
    if ($n -match 'hover') { return 'var(--agt-color-primary-800)' }
    if ($n -match 'active') { return 'var(--agt-color-primary-700)' }
    if ($n -match 'selected') { return 'var(--agt-color-accent-400)' }
    if ($n -match 'shadow') { return 'var(--agt-shadow-md)' }
    if ($n -match 'base-50') { return 'var(--agt-color-primary-900)' }
    if ($n -match 'base-(100|200|300|400)') { return 'var(--agt-color-primary-800)' }
    if ($n -match 'base-(500|600)') { return 'var(--agt-color-gray-500)' }
    if ($n -match 'base-(700|800)') { return 'var(--agt-color-gray-100)' }
    if ($n -match 'base-900') { return 'var(--agt-color-white)' }
    if ($n -match 'border') { return 'var(--agt-color-primary-700)' }
    if ($n -match '(background|surface|overlay|panel|card|popup|dialog|item)') { return 'var(--agt-color-primary-900)' }
    if ($n -match '(text|color|icon|label|title|caption|value|axis|gridline|stroke|fill)') { return 'var(--agt-color-white)' }
    if ($n -match '(chart|series)') { return 'var(--agt-color-primary-300)' }
    return 'var(--agt-color-white)'
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add(':root {') | Out-Null
foreach ($v in $colorVars) {
    $lines.Add("    ${v}: $(Resolve-Light $v);") | Out-Null
}
$lines.Add('}') | Out-Null
$lines.Add('') | Out-Null
$lines.Add('[data-agt-theme="dark"] {') | Out-Null
foreach ($v in $colorVars) {
    $lines.Add("    ${v}: $(Resolve-Dark $v);") | Out-Null
}
$lines.Add('}') | Out-Null

$target = "src/Agterhuis.Ui/wwwroot/css/theme/_variables.css"
$lines | Set-Content -Path $target -Encoding utf8

"allVars=$($allVars.Count)"
"mappedColorVars=$($colorVars.Count)"
(Get-Content $target | Measure-Object -Line).Lines
