function Get-NugetPath {
    param ()

    if ($IsWindows) {
        $result = Join-Path $env:USERPROFILE '.nuget\packages'
    }
    else {
        $result = '~/.nuget/packages'
    }

    $result
}