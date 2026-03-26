param(
    [string]$Channel = "8.0"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$toolsDir = Join-Path $projectRoot ".tools"
$installScript = Join-Path $toolsDir "dotnet-install.ps1"
$installDir = Join-Path $projectRoot ".dotnet"

New-Item -ItemType Directory -Force -Path $toolsDir | Out-Null

if (-not (Test-Path $installScript)) {
    Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript
}

& $installScript -Channel $Channel -InstallDir $installDir -NoPath
