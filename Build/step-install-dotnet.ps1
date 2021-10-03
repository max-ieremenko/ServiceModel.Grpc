$downloadDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
if (-not (Test-Path $downloadDir)) {
    New-Item -Path $downloadDir -ItemType Directory | Out-Null
}

$dotnetInstall = Join-Path $downloadDir "dotnet-install.ps1"
if (Test-Path $dotnetInstall) {
    Remove-Item -Path $dotnetInstall
}

Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $dotnetInstall

$globalJson = Get-Content -Raw (Join-Path $PSScriptRoot "..\Sources\global.json") | ConvertFrom-Json
$sdkVersion = $globalJson.sdk.version

& $dotnetInstall -Version $sdkVersion -InstallDir "C:\Program Files\dotnet"
