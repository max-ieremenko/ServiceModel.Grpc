#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.12.2" }

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [ValidateSet('win', 'linux')] 
    [string]
    $Platform,

    [Parameter()]
    [AllowNull()]
    [string]
    $Filter
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'scripts' 'Clear-NugetCache.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Get-FullPath.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Remove-DirectoryRecurse.ps1')

$distinctPath = New-Object System.Collections.Generic.HashSet[string]
$examples = @()

$configurations = Get-ChildItem -Path (Get-FullPath (Join-Path $PSScriptRoot '../Examples')) -Filter '*test-configuration.ps1' -File -Recurse
foreach ($configuration in $configurations) {
    if (-not [string]::IsNullOrWhiteSpace($Filter) -and $configuration.FullName -notmatch $Filter) {
        continue
    }

    $example = & $configuration
    if ($example.Platform -notin 'win', 'linux') {
        throw "Platform $($example.Platform) is not supported: $configuration"
    }

    if ($example.Platform -ne $Platform) {
        continue
    }

    $examples += $example
    $path = Split-Path $configuration -Parent
    $example.BuildParallelizable = $distinctPath.Add($path)
    $example.Solution = Join-Path $path $example.Solution

    if (-not $example['BuildMode']) {
        $example.BuildMode = 'Rebuild'
    }

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
            
            if (-not $case.ContainsKey('Port')) {
                $case.Port = 0
            }

            if (-not $case.ContainsKey('Type')) {
                $case.Type = 'pwsh'
                if ($case.App.EndsWith('.dll', 'OrdinalIgnoreCase')) {
                    $case.Type = 'dll'
                }
                elseif ($case.App.EndsWith('.exe', 'OrdinalIgnoreCase')) {
                    $case.Type = 'exe'
                }
            }

            if ($case.Type -notin 'exe', 'dll', 'pwsh') {
                throw "Type $($case.Mode) is not supported: $configuration"
            }
        
            if ($case.Type -in 'exe', 'dll') {
                $case.App = Join-Path $path $case.App
            }
        }
    }
}

Invoke-Build `
    -File (Join-Path $PSScriptRoot 'tasks' 'sdk-test-tasks.ps1') `
    -Examples $examples `
    -PathBuildArtifacts (Get-FullPath (Join-Path $PSScriptRoot '../build-out'))