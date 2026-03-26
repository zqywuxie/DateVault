param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$localDotnet = Join-Path $projectRoot ".dotnet\dotnet.exe"
$projectFile = Join-Path $projectRoot "src\DateVault.App\DateVault.App.csproj"
$publishDir = Join-Path $projectRoot "artifacts\publish\$Runtime"

if (-not (Test-Path $localDotnet)) {
    & (Join-Path $projectRoot "setup-dotnet.ps1")
}

if (-not (Test-Path $localDotnet)) {
    throw "Local .NET SDK not found at $localDotnet"
}

& $localDotnet publish $projectFile `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir
