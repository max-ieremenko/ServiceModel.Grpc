#Install-Module -Name psake
#Requires -Modules @{ModuleName='psake'; RequiredVersion='4.9.0'}

$psakeMain = Join-Path $PSScriptRoot "build-tasks.ps1"
Invoke-psake $psakeMain