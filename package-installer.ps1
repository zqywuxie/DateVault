param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishScript = Join-Path $projectRoot "publish.ps1"
$projectFile = Join-Path $projectRoot "src\DateVault.App\DateVault.App.csproj"
$publishDir = Join-Path $projectRoot "artifacts\publish\$Runtime"
$installerRoot = Join-Path $projectRoot "artifacts\installer"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

if (-not (Test-Path $publishScript)) {
    throw "Publish script not found at $publishScript"
}

if (-not (Test-Path $projectFile)) {
    throw "Project file not found at $projectFile"
}

$projectContent = Get-Content -Path $projectFile -Raw -Encoding utf8
$versionMatch = [regex]::Match($projectContent, '<Version>([^<]+)</Version>')
$version = if ($versionMatch.Success) { $versionMatch.Groups[1].Value } else { "1.0.0" }

powershell -ExecutionPolicy Bypass -File $publishScript -Configuration $Configuration -Runtime $Runtime

if (-not (Test-Path $publishDir)) {
    throw "Publish output not found at $publishDir"
}

$bundleName = "DateVault-Setup-$Runtime"
$bundleDir = Join-Path $installerRoot $bundleName
$appDir = Join-Path $bundleDir "app"
$iconSource = Join-Path $projectRoot "assets\datevault.ico"
$installerZip = Join-Path $installerRoot ("{0}-{1}.zip" -f $bundleName, $timestamp)

if (Test-Path $bundleDir) {
    Remove-Item -Path $bundleDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $appDir | Out-Null
Copy-Item -Path (Join-Path $publishDir '*') -Destination $appDir -Recurse -Force

if (Test-Path $iconSource) {
    Copy-Item -Path $iconSource -Destination (Join-Path $bundleDir "datevault.ico") -Force
}

$installScript = @"
param(
    [switch]`$NoLaunchAfterInstall,
    [switch]`$NoDesktopShortcut,
    [switch]`$NoStartMenuShortcut
)

`$ErrorActionPreference = "Stop"
`$appName = "DateVault"
`$productVersion = "$version"
`$installRoot = Join-Path `$env:LOCALAPPDATA "Programs\DateVault"
`$appDir = Join-Path `$installRoot "app"
`$sourceAppDir = Join-Path `$PSScriptRoot "app"
`$startMenuDir = Join-Path `$env:APPDATA "Microsoft\Windows\Start Menu\Programs\DateVault"
`$desktopShortcutPath = Join-Path ([Environment]::GetFolderPath("Desktop")) "DateVault.lnk"
`$startMenuShortcutPath = Join-Path `$startMenuDir "DateVault.lnk"
`$uninstallShortcutPath = Join-Path `$startMenuDir "卸载 DateVault.lnk"
`$exePath = Join-Path `$appDir "DateVault.exe"
`$iconPath = Join-Path `$installRoot "datevault.ico"
`$uninstallScriptPath = Join-Path `$installRoot "uninstall.ps1"
`$registryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\DateVault"
`$uninstallArguments = '-ExecutionPolicy Bypass -File "{0}"' -f `$uninstallScriptPath

if (-not (Test-Path `$sourceAppDir)) {
    throw "Installer payload not found: `$sourceAppDir"
}

New-Item -ItemType Directory -Force -Path `$appDir | Out-Null
if (Test-Path `$appDir) {
    Get-ChildItem -Path `$appDir -Force | Remove-Item -Recurse -Force
}

Copy-Item -Path (Join-Path `$sourceAppDir '*') -Destination `$appDir -Recurse -Force

if (Test-Path (Join-Path `$PSScriptRoot "datevault.ico")) {
    Copy-Item -Path (Join-Path `$PSScriptRoot "datevault.ico") -Destination `$iconPath -Force
}

Copy-Item -Path (Join-Path `$PSScriptRoot "uninstall.ps1") -Destination `$uninstallScriptPath -Force

`$shell = New-Object -ComObject WScript.Shell

if (-not `$NoDesktopShortcut) {
    `$shortcut = `$shell.CreateShortcut(`$desktopShortcutPath)
    `$shortcut.TargetPath = `$exePath
    `$shortcut.WorkingDirectory = `$appDir
    if (Test-Path `$iconPath) { `$shortcut.IconLocation = `$iconPath }
    `$shortcut.Save()
}

if (-not `$NoStartMenuShortcut) {
    New-Item -ItemType Directory -Force -Path `$startMenuDir | Out-Null

    `$shortcut = `$shell.CreateShortcut(`$startMenuShortcutPath)
    `$shortcut.TargetPath = `$exePath
    `$shortcut.WorkingDirectory = `$appDir
    if (Test-Path `$iconPath) { `$shortcut.IconLocation = `$iconPath }
    `$shortcut.Save()

    `$uninstallShortcut = `$shell.CreateShortcut(`$uninstallShortcutPath)
    `$uninstallShortcut.TargetPath = "powershell.exe"
    `$uninstallShortcut.Arguments = `$uninstallArguments
    `$uninstallShortcut.WorkingDirectory = `$installRoot
    if (Test-Path `$iconPath) { `$uninstallShortcut.IconLocation = `$iconPath }
    `$uninstallShortcut.Save()
}

New-Item -Path `$registryPath -Force | Out-Null
Set-ItemProperty -Path `$registryPath -Name "DisplayName" -Value `$appName
Set-ItemProperty -Path `$registryPath -Name "DisplayVersion" -Value `$productVersion
Set-ItemProperty -Path `$registryPath -Name "Publisher" -Value "DateVault"
Set-ItemProperty -Path `$registryPath -Name "InstallLocation" -Value `$installRoot
Set-ItemProperty -Path `$registryPath -Name "DisplayIcon" -Value `$exePath
Set-ItemProperty -Path `$registryPath -Name "UninstallString" -Value ("powershell.exe " + `$uninstallArguments)
Set-ItemProperty -Path `$registryPath -Name "NoModify" -Value 1 -Type DWord
Set-ItemProperty -Path `$registryPath -Name "NoRepair" -Value 1 -Type DWord

Write-Host "Installed to:"
Write-Host `$installRoot

if (-not `$NoLaunchAfterInstall -and (Test-Path `$exePath)) {
    Start-Process -FilePath `$exePath
}
"@

$uninstallScript = @"
`$ErrorActionPreference = "Stop"

`$installRoot = Split-Path -Parent `$MyInvocation.MyCommand.Path
`$startMenuDir = Join-Path `$env:APPDATA "Microsoft\Windows\Start Menu\Programs\DateVault"
`$desktopShortcutPath = Join-Path ([Environment]::GetFolderPath("Desktop")) "DateVault.lnk"
`$registryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\DateVault"

if (Test-Path `$desktopShortcutPath) {
    Remove-Item -Path `$desktopShortcutPath -Force -ErrorAction SilentlyContinue
}

if (Test-Path `$startMenuDir) {
    Remove-Item -Path `$startMenuDir -Recurse -Force -ErrorAction SilentlyContinue
}

if (Test-Path `$registryPath) {
    Remove-Item -Path `$registryPath -Recurse -Force -ErrorAction SilentlyContinue
}

if (Test-Path `$installRoot) {
    Remove-Item -Path `$installRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "DateVault has been removed."
"@

$installReadme = @"
DateVault Installer Bundle
Version: $version

How to install
1. Extract this zip to any temporary folder.
2. Right click install.ps1 and run with PowerShell.
3. The app will be installed for the current user only.

Install location
- %LOCALAPPDATA%\Programs\DateVault

How to uninstall
- Run uninstall.ps1 from the install directory
- or use the Start Menu shortcut
- or remove DateVault from Windows Apps list
"@

Set-Content -Path (Join-Path $bundleDir "install.ps1") -Value $installScript -Encoding utf8
Set-Content -Path (Join-Path $bundleDir "uninstall.ps1") -Value $uninstallScript -Encoding utf8
Set-Content -Path (Join-Path $bundleDir "INSTALL.txt") -Value $installReadme -Encoding utf8

New-Item -ItemType Directory -Force -Path $installerRoot | Out-Null
if (Test-Path $installerZip) {
    Remove-Item -Path $installerZip -Force
}

Compress-Archive -Path (Join-Path $bundleDir '*') -DestinationPath $installerZip -CompressionLevel Optimal

Write-Host "Installer bundle created:"
Write-Host $bundleDir
Write-Host $installerZip
