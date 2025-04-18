[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Version
)

function Test-Version {
    param (
        [Parameter(Mandatory)]
        [System.Management.Automation.SemanticVersion]
        $Target,

        [Parameter(Mandatory)]
        [System.Management.Automation.SemanticVersion]
        $Test
    )

    # 6.0 vs 7.0
    if ($Target.Major -ne $Test.Major -or $Target.Minor -ne $Test.Minor) {
        $false
    }
    else {
        # 6.0.0 vs 6.0.1
        # 7.0.100 vs 7.0.100-rc.2.22477.23
        $Target.CompareTo($Test) -le 0
    }
}

function Get-InstallationPath {
    if (Get-Command -Name dotnet -ErrorAction SilentlyContinue) {
        $versions = dotnet --list-sdks
        foreach ($installedVersion in $versions) {
            $path = ($installedVersion -split ' ')[1]
            $path = $path.Trim('[', ']')
            if (Test-Path $path) {
                return  (Split-Path -Path $path -Parent)
            }
        }
    }

    $IsLinux ? '/usr/share/dotnet' : 'C:\Program Files\dotnet'
}

if (Get-Command -Name dotnet -ErrorAction SilentlyContinue) {
    $versions = dotnet --list-sdks
    foreach ($installedVersion in $versions) {
        # 6.0.401 [C:\Program Files\dotnet\sdk]
        $test = ($installedVersion -split ' ')[0]
    
        if (Test-Version -Target $Version -Test $test) {
            Write-Output ".net sdk $test is already installed"
            return
        }
    }
}

$installDir = Get-InstallationPath
$installScript = 'dotnet-install.sh'

if ($IsWindows) {
    $installScript = 'dotnet-install.ps1'
}

$downloadDir = Join-Path ([System.IO.Path]::GetTempPath()) 'install-dotnet'
if (Test-Path $downloadDir) {
    Remove-Item -Path $downloadDir -Recurse -Force
}

New-Item -Path $downloadDir -ItemType Directory | Out-Null

$dotnetInstall = Join-Path $downloadDir $installScript
Invoke-WebRequest -Uri "https://dot.net/v1/$installScript" -OutFile $dotnetInstall

if (-not $IsWindows) {
    chmod +x $dotnetInstall
}

"$dotnetInstall -Version $Version -InstallDir $installDir"
if ($IsLinux -and (Get-Command -Name sudo -ErrorAction SilentlyContinue)) {
    sudo /bin/bash $dotnetInstall -Version $Version -InstallDir $installDir
}
else {
    & $dotnetInstall -Version $Version -InstallDir $installDir
}