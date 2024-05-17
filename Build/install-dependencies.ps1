#Requires -Version "7.0"

[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet(".net", "InvokeBuild", "ThirdPartyLibraries", "ZipAsFolder")] 
    [string[]]
    $List
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "scripts" "Get-ModuleVersion.ps1")

if (-not $List -or (".net" -in $List)) {
    $script = Join-Path $PSScriptRoot "scripts/Install-DotNet.ps1"
    
    & $script "6.0.421"
    & $script "7.0.405"

    $version = (Get-Content -Raw (Join-Path $PSScriptRoot "../Sources/global.json") | ConvertFrom-Json).sdk.version
    & $script $version
}

if (-not $List -or ("InvokeBuild" -in $List)) {
    $script = Join-Path $PSScriptRoot "scripts/Install-Module.ps1"
    $version = Get-ModuleVersion "InvokeBuild"
    & $script "InvokeBuild" $version
}

if (-not $List -or ("ThirdPartyLibraries" -in $List)) {
    $script = Join-Path $PSScriptRoot "scripts/Install-Module.ps1"
    $version = Get-ModuleVersion "ThirdPartyLibraries"
    & $script "ThirdPartyLibraries" $version
}

if (-not $List -or ("ZipAsFolder" -in $List)) {
    $script = Join-Path $PSScriptRoot "scripts/Install-Module.ps1"
    $version = Get-ModuleVersion "ZipAsFolder"
    & $script "ZipAsFolder" $version
}