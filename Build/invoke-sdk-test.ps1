#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.10.4" }

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [ValidateSet("win", "linux")] 
    [string]
    $Platform,

    [Parameter()]
    [AllowNull()]
    [string]
    $Filter
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "scripts" "Clear-NugetCache.ps1")
. (Join-Path $PSScriptRoot "scripts" "Get-FullPath.ps1")
. (Join-Path $PSScriptRoot "scripts" "Remove-DirectoryRecurse.ps1")

$distinctPath = New-Object System.Collections.Generic.HashSet[string]
$examples = @()

$configurations = Get-ChildItem -Path (Get-FullPath (Join-Path $PSScriptRoot "../Examples")) -Filter "*test-configuration.ps1" -File -Recurse
foreach ($configuration in $configurations) {
    if (-not [string]::IsNullOrWhiteSpace($Filter) -and $configuration.FullName -notmatch $Filter) {
        continue
    }

    $example = & $configuration
    if ($example.Platform -notin "win", "linux") {
        throw "Platform $($example.Platform) is not supported: $configuration"
    }

    if ($example.Platform -ne $Platform) {
        continue
    }

    $examples += $example
    $path = Split-Path $configuration -Parent
    $example.BuildParallelizable = $distinctPath.Add($path)
    $example.Solution = Join-Path $path $example.Solution

    if ($example.Tests -isnot [object[]]) {
        throw "Tests must be array of objects: $configuration"
    }

    foreach ($test in $example.Tests) {
        if ($test -isnot [object[]]) {
            throw "Test item $test must be array of objects: $configuration"
        }

        foreach ($case in $test) {
            if ($case -isnot [hashtable]) {
                throw "Case in a test item must be hashtable: $configuration"
            }

            if ($case.App.EndsWith(".dll", "OrdinalIgnoreCase") -or $case.App.EndsWith(".exe", "OrdinalIgnoreCase")) {
                $case.App = Join-Path $path $case.App
            }
            
            if (-not $case.ContainsKey("Port")) {
                $case.Port = 0
            }
        }
    }
}

Invoke-Build `
    -File (Join-Path $PSScriptRoot "tasks" "sdk-test-tasks.ps1") `
    -Examples $examples `
    -PathBuildArtifacts (Get-FullPath (Join-Path $PSScriptRoot "../build-out"))