#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.9.12" }

[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("win", "linux")] 
    [string]
    $Platform
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "scripts" "Clear-NugetCache.ps1")
. (Join-Path $PSScriptRoot "scripts" "Get-FullPath.ps1")
. (Join-Path $PSScriptRoot "scripts" "Remove-DirectoryRecurse.ps1")

if ($Platform -eq "win") {
    $solutions = @(
        @{ Path = "CompatibilityWithNativegRPC/CompatibilityWithNativegRPC.sln" }
        @{ Path = "ErrorHandling/ErrorHandling.sln" }
        @{ Path = "MigrateWCFFaultContractTogRpc/MigrateWCFFaultContractTogRpc.sln" }
        @{ Path = "MigrateWCFTogRpc/MigrateWCFTogRpc.sln" }
        @{ Path = "ProtobufMarshaller/ProtobufMarshaller.sln" }
    )
}
else {
    $solutions = @(
        @{ Path = "CreateClientAndServerASPNETCore/CreateClientAndServerASPNETCore.sln" }
        @{ Path = "grpc-dotnet-Compressor/Compressor.sln" }
        @{ Path = "grpc-dotnet-Counter/Counter.sln" }
        @{ Path = "grpc-dotnet-Interceptor/Interceptor.sln" }
        @{ Path = "JsonWebTokenAuthentication/JsonWebTokenAuthentication.sln" }
        @{ Path = "Swagger/NSwagSwagger.sln" }
        @{ Path = "Swagger/SwashbuckleSwagger.sln"; BuildParallelizable = $false }

        @{ Path = "CustomMarshaller/CustomMarshaller.sln"; Framework = "net6.0"; Apps = "Demo.ServerAspNetCore", "Demo.ServerSelfHost" }
        @{ Path = "FileUploadDownload/FileUploadDownload.sln"; Configuration = "Debug"; Framework = "net6.0"; Apps = "Benchmarks" }
        @{ Path = "InterfaceInheritance/InterfaceInheritance.sln"; Framework = "net6.0"; Apps = "Demo.ServerAspNetCore", "Demo.ServerSelfHost" }
        @{ Path = "MessagePackMarshaller/MessagePackMarshaller.sln"; Framework = "net6.0"; Apps = "Demo.ServerAspNetCore", "Demo.ServerSelfHost" }
        @{ Path = "ServerFilters/ServerFilters.sln"; Framework = "net6.0"; Apps = "ServerAspNetHost", "ServerSelfHost" }
        @{ Path = "SyncOverAsync/SyncOverAsync.sln"; Framework = "net6.0"; Apps = "Demo.ServerAspNetCore", "Demo.ServerSelfHost" }
    )
}

$examples = Get-FullPath (Join-Path $PSScriptRoot "../Examples")
foreach ($solution in $solutions) {
    $solution.Path = Join-Path $examples $solution.Path

    if (-not $solution.ContainsKey("Configuration")) {
        $solution.Configuration = "Release"
    }

    if (-not $solution.ContainsKey("Apps")) {
        $solution.Apps = @()
    }

    if (-not $solution.ContainsKey("BuildParallelizable")) {
        $solution.BuildParallelizable = $true
    }
}

Invoke-Build `
    -File (Join-Path $PSScriptRoot "tasks" "sdk-test-tasks.ps1") `
    -Solutions $solutions `
    -PathBuildArtifacts (Get-FullPath (Join-Path $PSScriptRoot "../build-out"))