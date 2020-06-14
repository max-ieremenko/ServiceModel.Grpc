function Write-ThirdPartyNotices($appNames, $sources, $repository, $out) {
    $appName = $appNames[0]
    $generateAppNames = $appNames | ForEach-Object {"-appName", $_}
    $source = $sources | ForEach-Object {"-source", $_}
    $outTemp = Join-Path $out "Temp"

    ThirdPartyLibraries update `
        -appName $appName `
        $source `
        -repository $repository
  
    ThirdPartyLibraries validate `
        -appName $appName `
        $source `
        -repository $repository

    ThirdPartyLibraries generate `
        $generateAppNames `
        -repository $repository `
        -to $outTemp

    $licenseFile = $appName + "ThirdPartyNotices.txt"
    Move-Item (Join-Path $outTemp "ThirdPartyNotices.txt") (Join-Path $out $licenseFile) -Force
    Remove-Item -Path $outTemp -Recurse -Force
}