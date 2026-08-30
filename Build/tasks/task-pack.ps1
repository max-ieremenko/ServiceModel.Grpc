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

task . DotnetPack, TestDotnetPack, JoinServiceModelGrpc, JoinServiceModelGrpcEmit, DesignTimeILMerge, Test

task DotnetPack {
    Invoke-Build -File 'task-dotnet-pack.ps1' -ProjectFile $Sources -BuildOut $BuildOut
}

task TestDotnetPack {
    Test-NugetPackageList -BuildOut $BuildOut -Name `
        "ServiceModel.Grpc.$releaseVersion" `
        , "ServiceModel.Grpc.AspNetCore.$releaseVersion" `
        , "ServiceModel.Grpc.AspNetCore.NSwag.$releaseVersion" `
        , "ServiceModel.Grpc.AspNetCore.Swashbuckle.$releaseVersion" `
        , "ServiceModel.Grpc.Client.DependencyInjection.$releaseVersion" `
        , "ServiceModel.Grpc.Core.$releaseVersion" `
        , "ServiceModel.Grpc.Descriptions.$releaseVersion" `
        , "ServiceModel.Grpc.DesignTime.$releaseVersion" `
        , "ServiceModel.Grpc.Emit.$releaseVersion" `
        , "ServiceModel.Grpc.Filters.$releaseVersion" `
        , "ServiceModel.Grpc.Interceptors.$releaseVersion" `
        , "ServiceModel.Grpc.MemoryPackMarshaller.$releaseVersion" `
        , "ServiceModel.Grpc.MessagePackMarshaller.$releaseVersion" `
        , "ServiceModel.Grpc.Nerdbank.MessagePackMarshaller.$releaseVersion" `
        , "ServiceModel.Grpc.ProtoBufMarshaller.$releaseVersion" `
        , "ServiceModel.Grpc.SelfHost.$releaseVersion"
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

task DesignTimeILMerge {
    Push-Location -Path (Join-Path $PSScriptRoot '..')
    try {
        exec { dotnet tool restore --verbosity quiet }
        
        $package = Join-Path $BuildOut "ServiceModel.Grpc.DesignTime.$releaseVersion.nupkg"
        Merge-DesignTimePackage -Sources $Sources -PackagePath $package
    }
    finally {
        Pop-Location
    }
}

task Test {
    $packageList = Get-ChildItem -Path $BuildOut -Recurse -Filter *.nupkg | ForEach-Object { $_.FullName }
    assert $packageList 'no packages found'
    
    $packageList | Test-NugetPackage
}