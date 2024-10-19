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

task . Clean, Build, UnitTest, ThirdPartyNotices, Pack

task Clean {
    Remove-DirectoryRecurse -Path $PathBuildOut
    Remove-DirectoryRecurse -Path $PathSources -Filters 'bin', 'obj'

    Clear-NugetCache
    
    New-Item -Path $PathBuildOut -ItemType Directory | Out-Null
}

task Build {
    $solutionFile = Join-Path $PathSources 'ServiceModel.Grpc.sln'
    Invoke-Build -File 'task-build.ps1' -Path $solutionFile
}

task UnitTest {
    $builds = @(
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net462' }
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net6.0' }
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net8.0' }
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net9.0' }
    )
    
    Build-Parallel $builds -ShowParameter Framework -MaximumBuilds 4
}

task ThirdPartyNotices {
    Invoke-Build -File 'task-third-party-notices.ps1' -Sources $PathSources -Repository $PathThirdParty -BuildOut $PathBuildOut -GithubToken $GithubToken
}

task Pack {
    Invoke-Build -File 'task-pack.ps1' -Sources $PathSources -BuildOut $PathBuildOut
}