param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishScript = Join-Path $projectRoot "publish.ps1"
$publishDir = Join-Path $projectRoot "artifacts\publish\$Runtime"
$portableDir = Join-Path $projectRoot "artifacts\portable"
$zipPath = Join-Path $portableDir ("DateVault-{0}-{1}.zip" -f $Runtime, (Get-Date -Format "yyyyMMdd-HHmmss"))

if (-not (Test-Path $publishScript)) {
    throw "Publish script not found at $publishScript"
}

powershell -ExecutionPolicy Bypass -File $publishScript -Configuration $Configuration -Runtime $Runtime

if (-not (Test-Path $publishDir)) {
    throw "Publish output not found at $publishDir"
}

New-Item -ItemType Directory -Force -Path $portableDir | Out-Null
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "Portable package created:"
Write-Host $zipPath
