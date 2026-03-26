param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$localDotnet = Join-Path $projectRoot ".dotnet\dotnet.exe"
$projectFile = Join-Path $projectRoot "src\DateVault.App\DateVault.App.csproj"

if (-not (Test-Path $localDotnet)) {
    & (Join-Path $projectRoot "setup-dotnet.ps1")
}

if (-not (Test-Path $localDotnet)) {
    throw "Local .NET SDK not found at $localDotnet"
}

& $localDotnet run --project $projectFile -c $Configuration
