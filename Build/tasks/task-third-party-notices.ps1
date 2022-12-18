[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Sources,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Repository,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $BuildOut,

    [Parameter()]
    [string]
    $GithubToken
)

Enter-Build {
    $releaseVersion = (Select-Xml -Path (Join-Path $Sources "Versions.props") -XPath "Project/PropertyGroup/ServiceModelGrpcVersion").Node.InnerText
}

task Default Core, AspNetCore, Swashbuckle, NSwag, DesignTime, SelfHost, ProtoBufMarshaller, MessagePackMarshaller

task Core {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc"),
        (Join-Path $Sources "ServiceModel.Grpc.Test"),
        (Join-Path $Sources "ServiceModel.Grpc.TestApi")
    )

    Write-ThirdPartyNotices `
        -AppNames "Core" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task AspNetCore {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.Test"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.TestApi")
    )

    Write-ThirdPartyNotices `
        -AppNames "AspNetCore", "Core" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.AspNetCore $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task Swashbuckle {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.Swashbuckle"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.Swashbuckle.Test"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.Test"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.TestApi")
    )

    Write-ThirdPartyNotices `
        -AppNames "AspNetCoreSwashbuckle", "Core" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.AspNetCore.Swashbuckle $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task NSwag {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.NSwag"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.NSwag.Test"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.Test"),
        (Join-Path $Sources "ServiceModel.Grpc.AspNetCore.TestApi")
    )

    Write-ThirdPartyNotices `
        -AppNames "AspNetCoreNSwag", "Core" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.AspNetCore.NSwag $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task DesignTime {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc.DesignTime"),
        (Join-Path $Sources "ServiceModel.Grpc.DesignTime.Roslyn3"),
        (Join-Path $Sources "ServiceModel.Grpc.DesignTime.Roslyn4"),
        (Join-Path $Sources "ServiceModel.Grpc.DesignTime.Test"),
        (Join-Path $Sources "ServiceModel.Grpc.DesignTime.Generator.Test")
    )

    Write-ThirdPartyNotices `
        -AppNames "DesignTime" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.DesignTime $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}


task SelfHost {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc.SelfHost"),
        (Join-Path $Sources "ServiceModel.Grpc.SelfHost.Test")
    )

    Write-ThirdPartyNotices `
        -AppNames "SelfHost", "Core" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.SelfHost $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task ProtoBufMarshaller {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc.ProtoBufMarshaller")
    )

    Write-ThirdPartyNotices `
        -AppNames "ProtoBuf", "Core" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.ProtoBufMarshaller $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task MessagePackMarshaller {
    $projects = @(
        (Join-Path $Sources "ServiceModel.Grpc.MessagePackMarshaller")
    )
    
    Write-ThirdPartyNotices `
        -AppNames "MessagePack", "Core" `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.MessagePackMarshaller $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}