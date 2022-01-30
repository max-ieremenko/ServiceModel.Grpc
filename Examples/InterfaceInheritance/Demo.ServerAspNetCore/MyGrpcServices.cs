using Service;
using ServiceModel.Grpc.DesignTime;

namespace Demo.ServerAspNetCore
{
    [ExportGrpcService(typeof(GenericCalculator<int>), GenerateAspNetExtensions = true)] // configure ServiceModel.Grpc.DesignTime to generate a source code for IGenericCalculator<int> endpoint
    internal static partial class MyGrpcServices
    {
    }
}
