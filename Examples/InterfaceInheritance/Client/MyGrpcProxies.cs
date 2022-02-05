using Contract;
using ServiceModel.Grpc.DesignTime;

namespace Client
{
    [ImportGrpcService(typeof(IGenericCalculator<int>))] // configure ServiceModel.Grpc.DesignTime to generate a source code for IGenericCalculator<int> client proxy
    internal static partial class MyGrpcProxies
    {
    }
}
