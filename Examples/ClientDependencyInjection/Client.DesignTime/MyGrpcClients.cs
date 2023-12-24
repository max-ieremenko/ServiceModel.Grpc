using Contract;
using ServiceModel.Grpc.DesignTime;

namespace Client.DesignTime;

[ImportGrpcService(typeof(ICalculator), GenerateDependencyInjectionExtensions = true)]
[ImportGrpcService(typeof(IRandomNumberGenerator), GenerateDependencyInjectionExtensions = true)]
internal static partial class MyGrpcClients
{
}