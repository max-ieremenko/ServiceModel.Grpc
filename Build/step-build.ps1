$sourceDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Sources"))
$solutionFile = Join-Path $sourceDir "ServiceModel.Grpc.sln"

Exec { dotnet restore $solutionFile }
Exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }
