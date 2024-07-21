function Write-ThirdPartyNotices {
    param (
        [Parameter(Mandatory)]
        [string[]]
        $AppNames,
        
        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string[]]
        $Sources,

        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Repository,
        
        [Parameter(Mandatory)]
        [string]
        $Title,

        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Out,

        [Parameter()]
        [string]
        $GithubToken
    )
    
    $appName = $AppNames[0]
    $outTemp = Join-Path $Out 'temp'

    Update-ThirdPartyLibrariesRepository -AppName $appName -Source $Sources -Repository $Repository -GithubPersonalAccessToken $GithubToken

    Test-ThirdPartyLibrariesRepository -AppName $appName -Source $Sources -Repository $Repository

    Publish-ThirdPartyNotices -AppName $AppNames -Repository $Repository -Title $Title -To $outTemp

    $licenseFile = $appName + 'ThirdPartyNotices.txt'
    Move-Item (Join-Path $outTemp 'ThirdPartyNotices.txt') (Join-Path $Out $licenseFile) -Force
    Remove-DirectoryRecurse -Path $outTemp
}