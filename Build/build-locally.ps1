#Requires -Modules @{ ModuleName="InvokeBuild"; RequiredVersion="5.9.9.0" }
#Requires -Modules @{ ModuleName="ThirdPartyLibraries"; RequiredVersion="3.1.0" }

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$main = Join-Path $PSScriptRoot "build-locally-tasks.ps1"
Invoke-Build -File $main