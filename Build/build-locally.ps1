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
    [string]
    $LinuxSdkFilter,

    [Parameter()]
    [switch]
    $SkipWinSdk,

    [Parameter()]
    [string]
    $WinSdkFilter,

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
    $nugetCache = Join-Path $HOME .nuget\packages
    if (-not $LinuxSdkFilter) {
        $LinuxSdkFilter = " "
    }

    $image = "mcr.microsoft.com/dotnet/sdk:8.0-jammy"
    docker pull $image

    docker run `
        -it `
        --rm `
        --entrypoint pwsh `
        -v "$($invokeBuild):/root/.local/share/powershell/Modules/InvokeBuild" `
        -v "$($nugetCache):/root/.nuget/packages" `
        -v "$($repository):/repository" `
        $image `
        "/repository/Build/invoke-sdk-test.ps1" `
        -Platform "linux" `
        -Filter $LinuxSdkFilter

    if ($LASTEXITCODE) {
        throw "Docker exited with $LASTEXITCODE code."
    }
}

if (-not $SkipWinSdk) {
    & (Join-Path $PSScriptRoot "invoke-sdk-test.ps1") -Platform "win" -Filter $WinSdkFilter
}

# benchmarks
if (-not $SkipBenchmarks) {
    & (Join-Path $PSScriptRoot "invoke-benchmarks.ps1") -Configuration $BenchmarksConfiguration
}