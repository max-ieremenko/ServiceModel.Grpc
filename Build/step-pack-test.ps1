[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

task Default {
    $packageList = Get-ChildItem -Path $Settings.buildOut -Recurse -Filter *.nupkg | ForEach-Object {$_.FullName}

    if ($packageList.Count -eq 0) {
        throw "no packages found."
    }
    
    $tempPath = Join-Path ([System.IO.Path]::GetTempPath()) "step-pack-test"
    try {
        foreach ($package in $packageList) {
            Test-NugetPackage -PackageFileName $package -TempDirectory $tempPath
        }
    }
    finally {
        Remove-DirectoryRecurse $tempPath
    }
}
