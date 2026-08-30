param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $PathSources,

    [Parameter(Mandatory)]
    [string]
    $PathBuildOut
)

task . Clean, Build, UnitTest

task Clean {
    Remove-DirectoryRecurse -Path $PathBuildOut
    Remove-DirectoryRecurse -Path $PathSources -Filters 'bin', 'obj'

    Clear-NugetCache
    
    New-Item -Path $PathBuildOut -ItemType Directory | Out-Null
}

task Build {
    $solutionFile = Join-Path $PathSources 'ServiceModel.Grpc.slnx'
    Invoke-Build -File 'task-build.ps1' -Path $solutionFile
}

task UnitTest {
    $builds = @(
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net481' }
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net8.0' }
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net9.0' }
        @{ File = 'task-unit-test.ps1'; Sources = $PathSources; Framework = 'net10.0' }
    )
    
    Build-Parallel $builds -ShowParameter Framework -MaximumBuilds 4
}