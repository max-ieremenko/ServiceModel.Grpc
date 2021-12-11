[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

task Default Clean, Build, Run

task Clean {
    Remove-DirectoryRecurse -Path $settings.benchmarks -Filters "bin", "obj"
}

task Build {
    $solutionFile = Join-Path $settings.benchmarks "Benchmarks.sln"

    exec { dotnet restore $solutionFile }
    exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Debug }
}

task Run {
    $app = Join-Path $settings.benchmarks "ServiceModel.Grpc.Benchmarks/bin/Debug/net6.0/ServiceModel.Grpc.Benchmarks.dll"
    exec { dotnet $app }
}
