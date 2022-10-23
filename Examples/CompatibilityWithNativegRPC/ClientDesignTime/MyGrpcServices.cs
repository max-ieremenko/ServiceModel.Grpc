using Contract;
using ServiceModel.Grpc.DesignTime;

namespace ClientDesignTime;

[ImportGrpcService(typeof(ICalculator))] // configure ServiceModel.Grpc.DesignTime to generate a source code for ICalculator proxy
internal static partial class MyGrpcServices
{
}