function Clear-NugetCache {
    param ()

    $nugetPackages = Get-NugetPath
    if (Test-Path $nugetPackages) {
        Get-ChildItem -Path $nugetPackages -Filter 'servicemodel.grpc*' -Directory | Remove-Item -Force -Recurse
    }
}
