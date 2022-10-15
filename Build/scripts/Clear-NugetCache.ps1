function Clear-NugetCache {
    param ()

    if ($IsWindows) {
        $nugetPackages = Join-Path $env:USERPROFILE ".nuget\packages"
    }
    else {
        $nugetPackages = "~/.nuget/packages"
    }

    if (Test-Path $nugetPackages) {
        Get-ChildItem -Path $nugetPackages -Filter "servicemodel.grpc*" -Directory | Remove-Item -Force -Recurse
    }
}
