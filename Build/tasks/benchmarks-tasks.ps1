param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $PathSources,

    [Parameter(Mandatory)]
    [string]
    $PathBuildOut,

    [Parameter(Mandatory)]
    [ValidateSet('Release', 'Debug')]
    [string]
    $Configuration
)

task . Clean, Build, Run, CopyResults, TestResults

Enter-Build {
    $pathApp = Join-Path $PathSources 'ServiceModel.Grpc.Benchmarks/bin' $Configuration
    $pathBuildOutArtifacts = Join-Path $PathBuildOut 'BenchmarkDotNet.Artifacts'
    Clear-NugetCache
    Add-NugetSource -Path $PathBuildOut
}

Exit-Build {
    Remove-NugetSource
    Clear-NugetCache
}

task Clean {
    Remove-DirectoryRecurse -Path $PathSources -Filters 'bin', 'obj'

    if (-not (Test-Path $PathBuildOut)) {
        New-Item -Path $PathBuildOut -ItemType Directory | Out-Null
    }
    else {
        Remove-DirectoryRecurse $pathBuildOutArtifacts
    }
}

task Build {
    $solutionFile = Join-Path $PathSources 'Benchmarks.slnx'
    Invoke-Build -File 'task-build.ps1' -Path $solutionFile -Configuration $Configuration
}

task Run {
    Set-Location -Path $pathApp
    exec { dotnet 'ServiceModel.Grpc.Benchmarks.dll' --filter *UnaryCall* }
}

task CopyResults {
    if ($Configuration -eq 'Release') {
        $source = Join-Path $pathApp 'BenchmarkDotNet.Artifacts/results'
        Move-Item -Path $source $pathBuildOutArtifacts -Force
    }
}

task TestResults -If ($Configuration -eq 'Release') {
    $files = Get-ChildItem -Path $pathBuildOutArtifacts -File -Filter '*.md'
    $files
    if (($files -isnot [array]) -or ($files.Length -le 2)) {
        throw 'It seems that BenchmarkDotNet failed to create artifacts.'
    }
}