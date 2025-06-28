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
    [ValidateSet('Release', 'Debug')]
    [string]
    $BenchmarksConfiguration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $IsWindows) {
    throw "$([Environment]::OSVersion.VersionString) is not supported."
}

. (Join-Path $PSScriptRoot 'scripts' 'Get-ModuleVersion.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Get-NugetPath.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Resolve-ModulePath.ps1')
. (Join-Path $PSScriptRoot 'scripts' 'Build-LinuxSdkImage.ps1')

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot 'invoke-ci-build.ps1')
}

if (-not $SkipLinuxSdk) {
    $repository = Join-Path $PSScriptRoot '../'
    $invokeBuild = Resolve-ModulePath -Name InvokeBuild -Version (Get-ModuleVersion -Name InvokeBuild)
    $nugetCache = Get-NugetPath
    if (-not $LinuxSdkFilter) {
        $LinuxSdkFilter = ' '
    }

    $image = Build-LinuxSdkImage

    #docker run -it --rm --entrypoint pwsh -v "$(Join-Path $env:USERPROFILE .nuget\packages):/root/.nuget/packages" -v "$(Join-Path (Get-Location) ..\):/repository" service-model-grpc/sdk:8.0-jammy
    docker run `
        -it `
        --rm `
        --entrypoint pwsh `
        -v "$($invokeBuild):/root/.local/share/powershell/Modules/InvokeBuild" `
        -v "$($nugetCache):/root/.nuget/packages" `
        -v "$($repository):/repository" `
        $image `
        '/repository/Build/invoke-sdk-test.ps1' `
        -Filter $LinuxSdkFilter

    if ($LASTEXITCODE) {
        throw "Docker exited with $LASTEXITCODE code."
    }
}

if (-not $SkipWinSdk) {
    & (Join-Path $PSScriptRoot 'invoke-sdk-test.ps1') -Filter $WinSdkFilter
}

# benchmarks
if (-not $SkipBenchmarks) {
    & (Join-Path $PSScriptRoot 'invoke-benchmarks.ps1') -Configuration $BenchmarksConfiguration
}