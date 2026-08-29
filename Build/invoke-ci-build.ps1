#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.14.23" }

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'scripts' 'Clear-NugetCache.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Get-FullPath.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Get-NugetPath.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Get-ReleaseVersion.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Remove-DirectoryRecurse.ps1')

Invoke-Build `
    -File (Join-Path $PSScriptRoot 'tasks' 'ci-build-tasks.ps1') `
    -PathSources (Get-FullPath (Join-Path $PSScriptRoot '../Sources')) `
    -PathBuildOut (Get-FullPath (Join-Path $PSScriptRoot '../build-out'))