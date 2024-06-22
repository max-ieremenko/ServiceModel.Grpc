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

task Default DotnetPack, JoinServiceModelGrpc, Test

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
    $releaseVersion = (Select-Xml -Path (Join-Path $Sources 'Versions.props') -XPath 'Project/PropertyGroup/ServiceModelGrpcVersion').Node.InnerText

    $sources = 'ServiceModel.Grpc.Core' `
        , 'ServiceModel.Grpc.Descriptions' `
        , 'ServiceModel.Grpc.Emit' `
        , 'ServiceModel.Grpc.Filters' `
        , 'ServiceModel.Grpc.Interceptors'
    
    foreach ($source in $sources) {
        Merge-NugetPackages -Source (Join-Path $BuildOut "$source.$releaseVersion.nupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.$releaseVersion.nupkg")
        Merge-NugetPackages -Source (Join-Path $BuildOut "$source.$releaseVersion.snupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.$releaseVersion.snupkg")
    }
}

task Test {
    $packageList = Get-ChildItem -Path $BuildOut -Recurse -Filter *.nupkg | ForEach-Object { $_.FullName }
    assert $packageList 'no packages found'
    
    $packageList | Test-NugetPackage
}