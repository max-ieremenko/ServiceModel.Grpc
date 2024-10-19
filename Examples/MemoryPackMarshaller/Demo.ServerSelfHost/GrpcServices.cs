using Service;
using ServiceModel.Grpc.DesignTime;
using Grpc.Core;
using System;

namespace Demo.ServerSelfHost;

[ExportGrpcService(typeof(Calculator), GenerateSelfHostExtensions = true)] // instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[MemoryPackDesignTimeExtension] // instruct ServiceModel.Grpc.MemoryPackMarshaller to generate required code during the build process
internal static partial class GrpcServices
{
    public static void MapAllGrpcServices(Server.ServiceDefinitionCollection services, Action<ServiceModelGrpcServiceOptions> configure)
    {
        // map generated Calculator endpoint
        services.AddCalculator(new Calculator(), configure);
    }
}