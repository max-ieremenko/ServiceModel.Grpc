$sourceDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Sources"))
$solutionFile = Join-Path $sourceDir "ServiceModel.Grpc.sln"

dotnet restore $solutionFile
dotnet build $solutionFile -t:Rebuild -p:Configuration=Release
