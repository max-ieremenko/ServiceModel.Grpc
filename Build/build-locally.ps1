#Requires -Version "7.0"

[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $SkipBuild,

    [Parameter()]
    [switch]
    $SkipLinuxSdk,

    [Parameter()]
    [switch]
    $SkipWinSdk,

    [Parameter()]
    [switch]
    $SkipBenchmarks,

    [Parameter()]
    [ValidateSet("Release", "Debug")]
    [string]
    $BenchmarksConfiguration = "Release"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "scripts" "Get-ModuleVersion.ps1")
. (Join-Path $PSScriptRoot "scripts" "Resolve-ModulePath.ps1")

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot "invoke-ci-build.ps1")
}

if (-not $SkipLinuxSdk) {
    $repository = Join-Path $PSScriptRoot "../"
    $invokeBuild = Resolve-ModulePath -Name InvokeBuild -Version (Get-ModuleVersion -Name InvokeBuild)
    docker run `
        -it `
        --rm `
        --entrypoint pwsh `
        -v "${invokeBuild}:/root/.local/share/powershell/Modules/InvokeBuild" `
        -v "$($repository):/repository" `
        mcr.microsoft.com/dotnet/sdk:6.0.401-jammy `
        "/repository/Build/invoke-sdk-test.ps1" `
        -Platform "linux"
}

if (-not $SkipWinSdk) {
    & (Join-Path $PSScriptRoot "invoke-sdk-test.ps1") -Platform "win"
}

# benchmarks
if (-not $SkipBenchmarks) {
    & (Join-Path $PSScriptRoot "invoke-benchmarks.ps1") -Configuration $BenchmarksConfiguration
}