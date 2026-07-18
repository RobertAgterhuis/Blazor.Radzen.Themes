param(
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

function Get-CssBlockTokens {
    param(
        [Parameter(Mandatory = $true)][string]$Css,
        [Parameter(Mandatory = $true)][string]$SelectorPrefix
    )

    $start = $Css.IndexOf($SelectorPrefix, [StringComparison]::Ordinal)
    if ($start -lt 0) {
        throw "Selector '$SelectorPrefix' not found."
    }

    $openBrace = $Css.IndexOf('{', $start)
    if ($openBrace -lt 0) {
        throw "Opening brace for selector '$SelectorPrefix' not found."
    }

    $depth = 0
    for ($index = $openBrace; $index -lt $Css.Length; $index++) {
        switch ($Css[$index]) {
            '{' { $depth++ }
            '}' {
                $depth--
                if ($depth -eq 0) {
                    $block = $Css.Substring($openBrace + 1, $index - $openBrace - 1)
                    return [regex]::Matches($block, '--agt-[a-z0-9-]+(?=\s*:)', [Text.RegularExpressions.RegexOptions]::IgnoreCase) |
                        ForEach-Object { $_.Value } |
                        Sort-Object -Unique
                }
            }
        }
    }

    throw "Could not find closing brace for selector '$SelectorPrefix'."
}

$themeDir = Join-Path $RootPath 'src/Agterhuis.Ui/wwwroot/css/themes'
$referenceTokens = Get-CssBlockTokens -Css (Get-Content (Join-Path $themeDir 'agt-theme.plum.css') -Raw) -SelectorPrefix 'html[data-agt-theme="dark"],'

$scopes = @(
    @{ File = 'agt-theme.plum.css'; Selector = ':root:not([data-agt-theme]),' },
    @{ File = 'agt-theme.ocean.css'; Selector = 'html[data-agt-theme="ocean-light"]' },
    @{ File = 'agt-theme.ocean.css'; Selector = 'html[data-agt-theme="ocean-dark"]' },
    @{ File = 'agt-theme.dagobah.css'; Selector = 'html[data-agt-theme="dagobah-light"]' },
    @{ File = 'agt-theme.dagobah.css'; Selector = 'html[data-agt-theme="dagobah-dark"]' }
)

$failures = @()
foreach ($scope in $scopes) {
    $css = Get-Content (Join-Path $themeDir $scope.File) -Raw
    $tokens = Get-CssBlockTokens -Css $css -SelectorPrefix $scope.Selector
    $missing = @($referenceTokens | Where-Object { $_ -notin $tokens })
    if ($missing.Count -gt 0) {
        $failures += "[$($scope.File)] $($scope.Selector) missing: $($missing -join ', ')"
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host 'Theme token parity check passed.'