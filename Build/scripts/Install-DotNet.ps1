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

if (Get-Command -Name dotnet -ErrorAction SilentlyContinue) {
    $versions = dotnet --list-sdks
    foreach ($installedVersion in $versions) {
        # 6.0.401 [C:\Program Files\dotnet\sdk]
        $test = ($installedVersion -split " ")[0]
    
        if (Test-Version -Target $Version -Test $test) {
            Write-Output ".net sdk $test is alredy installed"
            return
        }
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
