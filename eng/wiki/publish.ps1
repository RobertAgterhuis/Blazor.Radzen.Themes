param(
    [string]$WikiRepo = "https://github.com/RobertAgterhuis/Blazor.Radzen.Themes.wiki.git",
    [string]$Branch = "master",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot "..\..")
$sourceDir = Join-Path $repoRoot "eng\wiki"
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("agterhuis-ui-wiki-" + [System.Guid]::NewGuid().ToString("N"))

Write-Host "Preparing wiki publish from: $sourceDir"
Write-Host "Target wiki repo: $WikiRepo"

git clone --branch $Branch $WikiRepo $tempDir | Out-Host
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $tempDir)) {
    Write-Warning "Failed to clone wiki repository. If this is the first publish, initialize the GitHub wiki once via the UI by clicking 'Create the first page', then rerun this script."
    Write-Host "Local source pages prepared for sync:"
    Get-ChildItem -Path $sourceDir -Filter "*.md" | Select-Object -ExpandProperty Name | Out-Host
    exit 1
}

Get-ChildItem -Path $tempDir -File -Filter "*.md" | Remove-Item -Force
Copy-Item -Path (Join-Path $sourceDir "*.md") -Destination $tempDir -Force

Push-Location $tempDir
try {
    if ($DryRun) {
        Write-Host "Dry run mode. Showing pending wiki changes:"
        git status --short | Out-Host
        return
    }

    if (-not (git status --porcelain)) {
        Write-Host "No wiki changes to publish."
        return
    }

    git add *.md | Out-Host
    git commit -m "docs(wiki): sync from eng/wiki" | Out-Host
    git push origin $Branch | Out-Host
    Write-Host "Wiki publish complete."
}
finally {
    Pop-Location
    Remove-Item -Path $tempDir -Recurse -Force
}
