. (Join-Path $PSScriptRoot ".\step-third-party-notices-scripts.ps1")

$sourceDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Sources"))
$thirdPartyRepository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "third-party-libraries"))
$binDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))

# ServiceModel.Grpc
$appNames = @("Core")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.Test"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.TestApi")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir

# ServiceModel.Grpc.AspNetCore
$appNames = @("AspNetCore", "Core")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.Test"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.TestApi")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir

# ServiceModel.Grpc.AspNetCore.Swashbuckle
$appNames = @("AspNetCoreSwashbuckle", "Core")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.Swashbuckle"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.Swashbuckle.Test"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.Test"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.TestApi")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir

# ServiceModel.Grpc.AspNetCore.NSwag
$appNames = @("AspNetCoreNSwag", "Core")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.NSwag"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.NSwag.Test"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.Test"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.TestApi")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir

# ServiceModel.Grpc.DesignTime
$appNames = @("DesignTime")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc.DesignTime"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.DesignTime.Test"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.DesignTime.Generator.Test")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir

# ServiceModel.Grpc.SelfHost
$appNames = @("SelfHost", "Core")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc.SelfHost"),
    (Join-Path $script:sourceDir "ServiceModel.Grpc.SelfHost.Test")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir

# ServiceModel.Grpc.ProtoBufMarshaller
$appNames = @("ProtoBuf", "Core")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc.ProtoBufMarshaller")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir

# ServiceModel.Grpc.MessagePackMarshaller
$appNames = @("MessagePack", "Core")
$sources = @(
    (Join-Path $script:sourceDir "ServiceModel.Grpc.MessagePackMarshaller")
)

Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir