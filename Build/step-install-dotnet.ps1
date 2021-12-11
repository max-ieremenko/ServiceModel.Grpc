$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$installDir = "C:\Program Files\dotnet"
$installScript = "dotnet-install.ps1"
$installVersion = (Get-Content -Raw (Join-Path $PSScriptRoot "..\Sources\global.json") | ConvertFrom-Json).sdk.version

if ($IsLinux) {
    $installDir = "/usr/share/dotnet"
    $installScript = "dotnet-install.sh"
}

$downloadDir = Join-Path ([System.IO.Path]::GetTempPath()) "install-dotnet"
if (Test-Path $downloadDir) {
    Remove-Item -Path $downloadDir -Recurse -Force
}

New-Item -Path $downloadDir -ItemType Directory | Out-Null

$dotnetInstall = Join-Path $downloadDir $installScript
Invoke-WebRequest -Uri "https://dot.net/v1/$installScript" -OutFile $dotnetInstall

if ($IsLinux) {
    chmod +x $dotnetInstall
}

"$dotnetInstall -Version $installVersion -InstallDir $installDir"
& $dotnetInstall -Version $installVersion -InstallDir $installDir
