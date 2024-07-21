#Requires -Version "7.0"

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Path,

    [Parameter()]
    [switch]
    $SkipDotnetRestore
)

if (-not $SkipDotnetRestore) {
    foreach ($sln in (Get-ChildItem -Path $Path -Recurse -Filter '*.sln' -File)) {
        dotnet restore $sln.FullName --verbosity q
    }
}

$packages = @()
$distinct = New-Object System.Collections.Generic.HashSet[string]

# dot not pass .sln - .shproj is not supported
foreach ($project in (Get-ChildItem -Path $Path -Recurse -Filter '*.csproj' -File)) {
    $outdated = dotnet list $project.FullName package --outdated --format json | ConvertFrom-Json

    foreach ($package in $outdated.projects.frameworks.topLevelPackages) {
        if (-not $distinct.Add($package.id)) {
            continue
        }

        $packages += New-Object PSObject -Property @{ 
            name       = $package.id
            version    = $package.resolvedVersion
            newVersion = $package.latestVersion
        }
    }
}

$packages | Sort-Object { $_.id }