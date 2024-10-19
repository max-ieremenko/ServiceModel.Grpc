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
    $releaseVersion = Get-ReleaseVersion -Sources $Sources
}

task . Core, Emit, AspNetCore, Swashbuckle, NSwag, DesignTime, SelfHost, ClientDI, ProtoBufMarshaller, MessagePackMarshaller, MemoryPackMarshaller

task Core {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.Core'),
        (Join-Path $Sources 'ServiceModel.Grpc.Core.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.Filters'),
        (Join-Path $Sources 'ServiceModel.Grpc.Filters.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.Interceptors'),
        (Join-Path $Sources 'ServiceModel.Grpc.Interceptors.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc'),
        (Join-Path $Sources 'ServiceModel.Grpc.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.TestApi')
    )

    Write-ThirdPartyNotices `
        -AppNames 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task Emit {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.Descriptions'),
        (Join-Path $Sources 'ServiceModel.Grpc.Emit'),
        (Join-Path $Sources 'ServiceModel.Grpc.Emit.Test')
    )

    Write-ThirdPartyNotices `
        -AppNames 'Emit' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.Emit $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task AspNetCore {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.TestApi')
    )

    Write-ThirdPartyNotices `
        -AppNames 'AspNetCore', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.AspNetCore $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task Swashbuckle {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.Swashbuckle'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.Swashbuckle.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.TestApi')
    )

    Write-ThirdPartyNotices `
        -AppNames 'AspNetCoreSwashbuckle', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.AspNetCore.Swashbuckle $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task NSwag {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.NSwag'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.NSwag.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.AspNetCore.TestApi')
    )

    Write-ThirdPartyNotices `
        -AppNames 'AspNetCoreNSwag', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.AspNetCore.NSwag $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task DesignTime {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.DesignTime'),
        (Join-Path $Sources 'ServiceModel.Grpc.DesignTime.CodeAnalysis'),
        (Join-Path $Sources 'ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp'),
        (Join-Path $Sources 'ServiceModel.Grpc.DesignTime.CodeAnalysis.Internal'),
        (Join-Path $Sources 'ServiceModel.Grpc.DesignTime.CodeAnalysis.Internal.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.DesignTime.Generator.Test'),
        (Join-Path $Sources 'ServiceModel.Grpc.DesignTime.Generators')
    )

    Write-ThirdPartyNotices `
        -AppNames 'DesignTime' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.DesignTime $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task SelfHost {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.SelfHost'),
        (Join-Path $Sources 'ServiceModel.Grpc.SelfHost.Test')
    )

    Write-ThirdPartyNotices `
        -AppNames 'SelfHost', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.SelfHost $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task ClientDI {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.Client.DependencyInjection'),
        (Join-Path $Sources 'ServiceModel.Grpc.Client.DependencyInjection.Test')
    )

    Write-ThirdPartyNotices `
        -AppNames 'ClientDI', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.Client.DependencyInjection $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task ProtoBufMarshaller {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.ProtoBufMarshaller')
    )

    Write-ThirdPartyNotices `
        -AppNames 'ProtoBuf', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.ProtoBufMarshaller $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task MessagePackMarshaller {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.MessagePackMarshaller'),
        (Join-Path $Sources 'ServiceModel.Grpc.MessagePackMarshaller.Test')
    )
    
    Write-ThirdPartyNotices `
        -AppNames 'MessagePack', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.MessagePackMarshaller $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}

task MemoryPackMarshaller {
    $projects = @(
        (Join-Path $Sources 'ServiceModel.Grpc.MemoryPackMarshaller'),
        (Join-Path $Sources 'ServiceModel.Grpc.MemoryPackMarshaller.Test')
    )
    
    Write-ThirdPartyNotices `
        -AppNames 'MemoryPack', 'Core' `
        -Sources $projects `
        -Repository $Repository `
        -Title "ServiceModel.Grpc.MemoryPackMarshaller $releaseVersion" `
        -Out $BuildOut `
        -GithubToken $GithubToken
}