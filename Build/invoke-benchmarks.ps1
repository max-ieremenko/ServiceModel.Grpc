#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.10.3" }

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [ValidateSet("Release", "Debug")]
    [string]
    $Configuration
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "scripts" "Get-FullPath.ps1")
. (Join-Path $PSScriptRoot "scripts" "Remove-DirectoryRecurse.ps1")

Invoke-Build `
    -File (Join-Path $PSScriptRoot "tasks" "benchmarks-tasks.ps1") `
    -PathSources (Get-FullPath (Join-Path $PSScriptRoot "../Benchmarks")) `
    -PathBuildOut (Get-FullPath (Join-Path $PSScriptRoot "../build-out")) `
    -Configuration $Configuration