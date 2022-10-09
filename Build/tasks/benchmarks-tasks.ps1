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

task Default Clean, Build, Run, CopyResults

task Clean {
    Remove-DirectoryRecurse -Path $PathSources -Filters "bin", "obj"

    if (-not (Test-Path $PathBuildOut)) {
        New-Item -Path $PathBuildOut -ItemType Directory | Out-Null
    }
}

task Build {
    $solutionFile = Join-Path $PathSources "Benchmarks.sln"
    Invoke-Build -File "task-build.ps1" -Path $solutionFile -Configuration $Configuration
}

task Run {
    $app = Join-Path $PathSources "ServiceModel.Grpc.Benchmarks/bin" $Configuration "net6.0"

    Set-Location -Path $app
    exec { dotnet "ServiceModel.Grpc.Benchmarks.dll" --filter *UnaryCall* }
}

task CopyResults {
    if ($Configuration -eq "Release") {
        $source = Join-Path $PathSources "ServiceModel.Grpc.Benchmarks/bin/Release/net6.0" "BenchmarkDotNet.Artifacts/results"
        $dest = Join-Path $PathBuildOut "BenchmarkDotNet.Artifacts"
        Move-Item -Path $source $dest -Force
    }
}