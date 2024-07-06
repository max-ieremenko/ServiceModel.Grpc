param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $PathSources,

    [Parameter(Mandatory)]
    [string]
    $PathBuildOut,

    [Parameter(Mandatory)]
    [ValidateSet("Release", "Debug")]
    [string]
    $Configuration
)

task . Clean, Build, Run, CopyResults

Enter-Build {
    $pathApp = Join-Path $PathSources "ServiceModel.Grpc.Benchmarks/bin" $Configuration "net8.0"
    $pathBuildOutArtifacts = Join-Path $PathBuildOut "BenchmarkDotNet.Artifacts"
}

task Clean {
    Remove-DirectoryRecurse -Path $PathSources -Filters "bin", "obj"

    if (-not (Test-Path $PathBuildOut)) {
        New-Item -Path $PathBuildOut -ItemType Directory | Out-Null
    } else {
        Remove-DirectoryRecurse $pathBuildOutArtifacts
    }
}

task Build {
    $solutionFile = Join-Path $PathSources "Benchmarks.sln"
    Invoke-Build -File "task-build.ps1" -Path $solutionFile -Configuration $Configuration
}

task Run {
    Set-Location -Path $pathApp
    exec { dotnet "ServiceModel.Grpc.Benchmarks.dll" --filter *UnaryCall* }
}

task CopyResults {
    if ($Configuration -eq "Release") {
        $source = Join-Path $pathApp "BenchmarkDotNet.Artifacts/results"
        Move-Item -Path $source $pathBuildOutArtifacts -Force
    }
}