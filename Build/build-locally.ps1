#Install-Module -Name InvokeBuild -RequiredVersion 5.8.6
#Requires -Modules @{ ModuleName="InvokeBuild"; RequiredVersion="5.8.6" }

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$main = Join-Path $PSScriptRoot "build-locally-tasks.ps1"
Invoke-Build -File $main