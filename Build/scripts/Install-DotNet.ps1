[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Version
)

$versions = dotnet --list-sdks
foreach ($installedVersion in $versions) {
    # 6.0.401 [C:\Program Files\dotnet\sdk]
    $test = ($installedVersion -split " ")[0]

    if ($test -eq $Version) {
        Write-Output ".net sdk $version is alredy installed"
        return
    }
}

$installDir = "C:\Program Files\dotnet"
$installScript = "dotnet-install.ps1"

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

"$dotnetInstall -Version $Version -InstallDir $installDir"
& $dotnetInstall -Version $Version -InstallDir $installDir
