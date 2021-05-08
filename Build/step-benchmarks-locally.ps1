$sourceDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Benchmarks"))
$solutionFile = Join-Path $sourceDir "Benchmarks.sln"

Exec { dotnet restore $solutionFile }
Exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Debug }

$app = Join-Path $sourceDir "ServiceModel.Grpc.Benchmarks/bin/Debug/net5.0/ServiceModel.Grpc.Benchmarks.dll"
Exec { dotnet $app }