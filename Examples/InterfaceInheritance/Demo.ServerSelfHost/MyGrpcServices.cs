using Service;
using ServiceModel.Grpc.DesignTime;

namespace Demo.ServerSelfHost;

[ExportGrpcService(typeof(DoubleCalculator), GenerateSelfHostExtensions = true)] // configure ServiceModel.Grpc.DesignTime to generate a source code for DoubleCalculator endpoint
internal static partial class MyGrpcServices
{
}