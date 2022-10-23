using Contract;
using ServiceModel.Grpc.DesignTime;

namespace gRPCClientDesignTime;

[ImportGrpcService(typeof(IPersonService))] // configure ServiceModel.Grpc.DesignTime to generate a source code for IPersonService proxy
internal static partial class MyGrpcServices
{
}