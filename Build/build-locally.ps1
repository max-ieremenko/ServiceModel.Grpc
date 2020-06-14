#Install-Module -Name psake
#Requires -Modules @{ModuleName='psake'; RequiredVersion='4.9.0'}

$env:GITHUB_SHA = "test commit id"

$psakeMain = Join-Path $PSScriptRoot "build-locally-tasks.ps1"
Invoke-psake $psakeMain