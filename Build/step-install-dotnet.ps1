$downloadDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
if (-not (Test-Path $downloadDir)) {
    New-Item -Path $downloadDir -ItemType Directory | Out-Null
}

$dotnetInstall = Join-Path $downloadDir "dotnet-install.ps1"
if (Test-Path $dotnetInstall) {
    Remove-Item -Path $dotnetInstall
}

Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $dotnetInstall

& $dotnetInstall -Version "5.0.100-rc.2.20464.8" -InstallDir "C:\Program Files\dotnet"
