[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Name,

    [Parameter(Mandatory)]
    [string]
    $Version
)

$test = Get-InstalledModule -Name $Name -MinimumVersion $Version -ErrorAction 'SilentlyContinue'
if ($test) {
    Write-Output "$Name $($test.Version) is alredy installed"
    return
}

Write-Output "Install $Name $version"
Install-Module -Name $Name -RequiredVersion $Version -Force
