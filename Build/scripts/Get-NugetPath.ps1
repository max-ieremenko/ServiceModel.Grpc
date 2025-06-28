function Get-NugetPath {
    param ()

    if ($IsWindows) {
        $result = Join-Path $env:USERPROFILE '.nuget\packages'
    }
    else {
        $result = '~/.nuget/packages'
    }

    if (-not (Test-Path $result -PathType Container)) {
        throw "Path $result not found."
    }

    $result
}