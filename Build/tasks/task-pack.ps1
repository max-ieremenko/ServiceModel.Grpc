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
    $projects = 'ServiceModel.Grpc.Core\ServiceModel.Grpc.Core.csproj' `
        , 'ServiceModel.Grpc\ServiceModel.Grpc.csproj' `
        , 'ServiceModel.Grpc.AspNetCore\ServiceModel.Grpc.AspNetCore.csproj' `
        , 'ServiceModel.Grpc.AspNetCore.Swashbuckle\ServiceModel.Grpc.AspNetCore.Swashbuckle.csproj' `
        , 'ServiceModel.Grpc.AspNetCore.NSwag\ServiceModel.Grpc.AspNetCore.NSwag.csproj' `
        , 'ServiceModel.Grpc.DesignTime\ServiceModel.Grpc.DesignTime.csproj' `
        , 'ServiceModel.Grpc.SelfHost\ServiceModel.Grpc.SelfHost.csproj' `
        , 'ServiceModel.Grpc.Client.DependencyInjection\ServiceModel.Grpc.Client.DependencyInjection.csproj' `
        , 'ServiceModel.Grpc.ProtoBufMarshaller\ServiceModel.Grpc.ProtoBufMarshaller.csproj' `
        , 'ServiceModel.Grpc.MessagePackMarshaller\ServiceModel.Grpc.MessagePackMarshaller.csproj'

    $builds = @()
    foreach ($project in $projects) {
        $builds += @{ File = 'task-dotnet-pack.ps1'; ProjectFile = (Join-Path $Sources $project); BuildOut = $BuildOut }
    }

    Build-Parallel $builds -ShowParameter ProjectFile -MaximumBuilds 1
}

task JoinServiceModelGrpc {
    $releaseVersion = (Select-Xml -Path (Join-Path $Sources 'Versions.props') -XPath 'Project/PropertyGroup/ServiceModelGrpcVersion').Node.InnerText

    Merge-NugetPackages -Source (Join-Path $BuildOut "ServiceModel.Grpc.Core.$releaseVersion.nupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.$releaseVersion.nupkg")
    Merge-NugetPackages -Source (Join-Path $BuildOut "ServiceModel.Grpc.Core.$releaseVersion.snupkg") -Destination (Join-Path $BuildOut "ServiceModel.Grpc.$releaseVersion.snupkg")
}

task Test {
    $packageList = Get-ChildItem -Path $BuildOut -Recurse -Filter *.nupkg | ForEach-Object { $_.FullName }
    assert $packageList 'no packages found'
    
    $packageList | Test-NugetPackage
}