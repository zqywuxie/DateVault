param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$localDotnet = Join-Path $projectRoot ".dotnet\dotnet.exe"

if (-not (Test-Path $localDotnet)) {
    & (Join-Path $projectRoot "setup-dotnet.ps1")
}

if (-not (Test-Path $localDotnet)) {
    throw "Local .NET SDK not found at $localDotnet"
}

& $localDotnet build (Join-Path $projectRoot "DateVault.sln") -c $Configuration
