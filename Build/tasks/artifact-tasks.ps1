param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $PathSources,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $PathThirdParty,

    [Parameter(Mandatory)]
    [string]
    $PathBuildOut,

    [Parameter()]
    [string]
    $GithubToken
)

task . ThirdPartyNotices, Pack

task ThirdPartyNotices {
    Invoke-Build -File 'task-third-party-notices.ps1' -Sources $PathSources -Repository $PathThirdParty -BuildOut $PathBuildOut -GithubToken $GithubToken
}

task Pack {
    Invoke-Build -File 'task-pack.ps1' -Sources $PathSources -BuildOut $PathBuildOut
}