using Contract;
using ServiceModel.Grpc.DesignTime;

namespace ClientDesignTime;

[ImportGrpcService(typeof(IDebugService))] // configure ServiceModel.Grpc.DesignTime to generate a source code for IDebugService proxy
internal static partial class MyGrpcServices
{
}