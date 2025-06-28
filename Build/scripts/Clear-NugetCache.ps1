function Clear-NugetCache {
    param ()

    $nugetPackages = Get-NugetPath
    Get-ChildItem -Path $nugetPackages -Filter 'servicemodel.grpc*' -Directory | Remove-Item -Force -Recurse
}
