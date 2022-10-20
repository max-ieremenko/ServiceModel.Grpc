using Contract;
using ServiceModel.Grpc.DesignTime;

namespace ClientDesignTime;

[ImportGrpcService(typeof(IGreeter))] // configure ServiceModel.Grpc.DesignTime to generate a source code for IGreeter proxy
internal static partial class MyGrpcServices
{
}