param(
    [Parameter(Mandatory = $true)]
    [string]$FilePath,
    [string]$TimestampUrl = "http://timestamp.digicert.com",
    [string]$CertificateThumbprint = ""
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $FilePath)) {
    throw "Target file not found: $FilePath"
}

$signtool = Get-Command signtool.exe -ErrorAction SilentlyContinue
if (-not $signtool) {
    throw "signtool.exe not found. Install Windows SDK first."
}

if ([string]::IsNullOrWhiteSpace($CertificateThumbprint)) {
    Write-Host "Template mode only. Example usage:"
    Write-Host ".\\sign-package-template.ps1 -FilePath .\\artifacts\\publish\\win-x64\\DateVault.exe -CertificateThumbprint YOUR_CERT_THUMBPRINT"
    exit 0
}

& $signtool.Source sign `
    /sha1 $CertificateThumbprint `
    /fd SHA256 `
    /tr $TimestampUrl `
    /td SHA256 `
    $FilePath
