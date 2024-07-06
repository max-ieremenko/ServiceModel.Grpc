[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Sources,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $BuildOut
)

Enter-Build {
    $releaseVersion = Get-ReleaseVersion -Sources $Sources
}

task . DotnetPack, JoinServiceModelGrpc, JoinServiceModelGrpcEmit, Test

task DotnetPack {
    $projects = @()
    foreach ($file in (Get-ChildItem -Path $Sources -Recurse -Filter *.csproj)) {
        $test = Select-Xml -Path $file -XPath 'Project/PropertyGroup/IsPackable'
        if ($test -and $test.Node.InnerText -eq 'true') {
            $projects += $file.FullName
        }
    }

    $builds = @()
    foreach ($project in $projects) {
        $builds += @{ File = 'task-dotnet-pack.ps1'; ProjectFile = $project; BuildOut = $BuildOut }
    }

    Build-Parallel $builds -ShowParameter ProjectFile -MaximumBuilds 1
}

task JoinServiceModelGrpc {
    $sources = 'ServiceModel.Grpc.Filters', 'ServiceModel.Grpc.Interceptors'
    
    foreach ($source in $sources) {
        Merge-NugetPackages -Source (Join-Path $BuildOut "$source.$releaseVersion.nupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.$releaseVersion.nupkg")
        Merge-NugetPackages -Source (Join-Path $BuildOut "$source.$releaseVersion.snupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.$releaseVersion.snupkg")
    }
}

task JoinServiceModelGrpcEmit {
    $source = 'ServiceModel.Grpc.Descriptions'
    
    Merge-NugetPackages -Source (Join-Path $BuildOut "$source.$releaseVersion.nupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.Emit.$releaseVersion.nupkg")
    Merge-NugetPackages -Source (Join-Path $BuildOut "$source.$releaseVersion.snupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.Emit.$releaseVersion.snupkg")
}

task Test {
    $packageList = Get-ChildItem -Path $BuildOut -Recurse -Filter *.nupkg | ForEach-Object { $_.FullName }
    assert $packageList 'no packages found'
    
    $packageList | Test-NugetPackage
}