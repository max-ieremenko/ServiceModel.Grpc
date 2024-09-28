using Contract;
using ServiceModel.Grpc.Client.DependencyInjection;
using ServiceModel.Grpc.DesignTime;

namespace Client.Services;

[ImportGrpcService(typeof(ICalculator), GenerateDependencyInjectionExtensions = true)]// instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[MessagePackDesignTimeExtension] // instruct ServiceModel.Grpc.MessagePackMarshaller to generate required code during the build process
internal static partial class GrpcServices
{
    public static void AddAllGrpcServices(this IClientFactoryBuilder factoryBuilder)
    {
        // map generated ICalculator client
        factoryBuilder.AddCalculatorClient();
    }
}