using Microsoft.AspNetCore.Routing;
using Service;
using ServiceModel.Grpc.DesignTime;

namespace Demo.ServerAspNetCore;

[ExportGrpcService(typeof(Calculator), GenerateAspNetExtensions = true)] // instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[MemoryPackDesignTimeExtension] // instruct ServiceModel.Grpc.MemoryPackMarshaller to generate required code during the build process
internal static partial class GrpcServices
{
    public static void MapAllGrpcServices(IEndpointRouteBuilder builder)
    {
        // map generated Calculator endpoint
        builder.MapCalculator();
    }
}