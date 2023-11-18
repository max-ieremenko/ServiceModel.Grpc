#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.10.4" }
#Requires -Modules @{ ModuleName="ThirdPartyLibraries"; ModuleVersion="3.5.0" }
#Requires -Modules @{ ModuleName="ZipAsFolder"; ModuleVersion="0.0.1" }

[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $GithubToken
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "scripts" "Clear-NugetCache.ps1")
. (Join-Path $PSScriptRoot "scripts" "Get-FullPath.ps1")
. (Join-Path $PSScriptRoot "scripts" "Remove-DirectoryRecurse.ps1")
. (Join-Path $PSScriptRoot "scripts" "Test-NugetPackage.ps1")
. (Join-Path $PSScriptRoot "scripts" "Write-ThirdPartyNotices.ps1")

Invoke-Build `
    -File (Join-Path $PSScriptRoot "tasks" "ci-build-tasks.ps1") `
    -PathSources (Get-FullPath (Join-Path $PSScriptRoot "../Sources")) `
    -PathThirdParty (Get-FullPath (Join-Path $PSScriptRoot "third-party-libraries")) `
    -PathBuildOut (Get-FullPath (Join-Path $PSScriptRoot "../build-out")) `
    -GithubToken $GithubToken