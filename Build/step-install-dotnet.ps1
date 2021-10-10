$installDir = "C:\Program Files\dotnet"
$installScript = "dotnet-install.ps1"
$installVersion = (Get-Content -Raw (Join-Path $PSScriptRoot "..\Sources\global.json") | ConvertFrom-Json).sdk.version

if ($IsLinux) {
    $installDir = "/usr/share/dotnet"
    $installScript = "dotnet-install.sh"
}

$downloadDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
if (-not (Test-Path $downloadDir)) {
    New-Item -Path $downloadDir -ItemType Directory | Out-Null
}

$dotnetInstall = Join-Path $downloadDir $installScript
if (Test-Path $dotnetInstall) {
    Remove-Item -Path $dotnetInstall
}

Invoke-WebRequest -Uri "https://dot.net/v1/$installScript" -OutFile $dotnetInstall

if ($IsLinux) {
    chmod +x $dotnetInstall
}

& $dotnetInstall -Version $installVersion -InstallDir $installDir
