function Write-ThirdPartyNotices {
    param (
        $appNames, $sources, $repository, $out
    )
    
    $appName = $appNames[0]
    $generateAppNames = $appNames | ForEach-Object {"-appName", $_}
    $source = $sources | ForEach-Object {"-source", $_}
    $outTemp = Join-Path $out "Temp"

    exec {
        ThirdPartyLibraries update `
            -appName $appName `
            $source `
            -repository $repository
    }

    exec {
        ThirdPartyLibraries validate `
            -appName $appName `
            $source `
            -repository $repository
    }

    exec {
        ThirdPartyLibraries generate `
            $generateAppNames `
            -repository $repository `
            -to $outTemp
    }

    $licenseFile = $appName + "ThirdPartyNotices.txt"
    Move-Item (Join-Path $outTemp "ThirdPartyNotices.txt") (Join-Path $out $licenseFile) -Force
    Remove-Item -Path $outTemp -Recurse -Force
}