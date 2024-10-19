using Contract;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.DesignTime;

namespace Client;

[ImportGrpcService(typeof(ICalculator))]// instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[MemoryPackDesignTimeExtension] // instruct ServiceModel.Grpc.MemoryPackMarshaller to generate required code during the build process
internal static partial class GrpcServices
{
    public static void AddAllClients(IClientFactory clientFactory)
    {
        // register generated ICalculator client
        clientFactory.AddCalculatorClient();
    }
}