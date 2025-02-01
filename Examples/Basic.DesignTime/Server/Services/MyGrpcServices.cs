using ServiceModel.Grpc.DesignTime;

namespace Server.Services;

// configure ServiceModel.Grpc.DesignTime to generate a source code for Calculator endpoint
[ExportGrpcService(typeof(Calculator), GenerateAspNetExtensions = true)]
internal static partial class MyGrpcServices;