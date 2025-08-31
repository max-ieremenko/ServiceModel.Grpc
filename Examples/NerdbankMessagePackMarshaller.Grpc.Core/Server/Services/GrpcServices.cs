using ServiceModel.Grpc.DesignTime;

#pragma warning disable ServiceModelGrpcInternalAPI

namespace Server.Services;

[ExportGrpcService(typeof(Calculator), GenerateSelfHostExtensions = true)] // instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[NerdbankMessagePackDesignTimeExtension] // instruct ServiceModel.Grpc.Nerdbank.MessagePackMarshaller to generate required code during the build process
internal static partial class GrpcServices;