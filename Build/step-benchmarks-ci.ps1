. (Join-Path $PSScriptRoot "scripts/Import-All.ps1")

Enter-Build {
    $sourceDir = Get-FullPath (Join-Path $PSScriptRoot "../Benchmarks")
    $app = Join-Path $sourceDir "ServiceModel.Grpc.Benchmarks/bin/Release/net6.0"
}

task Default Build, Run, CopyResults

task Build {
    $solutionFile = Join-Path $sourceDir "Benchmarks.sln"    
    
    exec { dotnet restore $solutionFile }
    exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }
}

task Run {
    Set-Location -Path $app
    exec { dotnet "ServiceModel.Grpc.Benchmarks.dll" --filter *UnaryCall* }
}

task CopyResults {
    $results = Join-Path $app "BenchmarkDotNet.Artifacts/results"
    Move-Item -Path $results ./../BenchmarkDotNet.Artifacts
}