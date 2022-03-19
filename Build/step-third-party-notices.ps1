[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    $Settings
)

task Default Core, AspNetCore, Swashbuckle, NSwag, DesignTime, SelfHost, ProtoBufMarshaller, MessagePackMarshaller

task Core {
    # ServiceModel.Grpc
    $appNames = @("Core")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.Test"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.TestApi")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}

task AspNetCore {
    # ServiceModel.Grpc.AspNetCore
    $appNames = @("AspNetCore", "Core")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.Test"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.TestApi")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}

task Swashbuckle {
    # ServiceModel.Grpc.AspNetCore.Swashbuckle
    $appNames = @("AspNetCoreSwashbuckle", "Core")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.Swashbuckle"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.Swashbuckle.Test"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.Test"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.TestApi")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}

task NSwag {
    # ServiceModel.Grpc.AspNetCore.NSwag
    $appNames = @("AspNetCoreNSwag", "Core")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.NSwag"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.NSwag.Test"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.Test"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.TestApi")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}

task DesignTime {
    # ServiceModel.Grpc.DesignTime
    $appNames = @("DesignTime")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc.DesignTime"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.DesignTime.Roslyn3"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.DesignTime.Roslyn4"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.DesignTime.Test"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.DesignTime.Generator.Test")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}


task SelfHost {
    # ServiceModel.Grpc.SelfHost
    $appNames = @("SelfHost", "Core")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc.SelfHost"),
        (Join-Path $Settings.sources "ServiceModel.Grpc.SelfHost.Test")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}

task ProtoBufMarshaller {
    # ServiceModel.Grpc.ProtoBufMarshaller
    $appNames = @("ProtoBuf", "Core")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc.ProtoBufMarshaller")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}

task MessagePackMarshaller {
    # ServiceModel.Grpc.MessagePackMarshaller
    $appNames = @("MessagePack", "Core")
    $sources = @(
        (Join-Path $Settings.sources "ServiceModel.Grpc.MessagePackMarshaller")
    )

    Write-ThirdPartyNotices $appNames $sources $Settings.thirdParty $Settings.buildOut
}