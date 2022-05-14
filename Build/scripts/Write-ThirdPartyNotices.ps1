function Write-ThirdPartyNotices {
    param (
        $appNames, $sources, $repository, $title, $out
    )
    
    $appName = $appNames[0]
    $outTemp = Join-Path $out "Temp"

    Update-ThirdPartyLibrariesRepository -AppName $appName -Source $sources -Repository $repository -InformationAction Continue

    Test-ThirdPartyLibrariesRepository -AppName $appName -Source $sources -Repository $repository -InformationAction Continue

    Publish-ThirdPartyNotices -AppName $appNames -Repository $repository -Title $title -To $outTemp -InformationAction Continue

    $licenseFile = $appName + "ThirdPartyNotices.txt"
    Move-Item (Join-Path $outTemp "ThirdPartyNotices.txt") (Join-Path $out $licenseFile) -Force
    Remove-Item -Path $outTemp -Recurse -Force
}