[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    $Settings
)

task Default {
    $solutionFile = Join-Path $Settings.sources "ServiceModel.Grpc.sln"
    
    exec { dotnet restore $solutionFile }
    exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }
}
