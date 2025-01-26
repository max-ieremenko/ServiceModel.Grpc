using Contract;
using ServiceModel.Grpc.DesignTime;

namespace Client;

// configure ServiceModel.Grpc.DesignTime to generate a source code for ICalculator client proxy
[ImportGrpcService(typeof(ICalculator))]
internal static partial class MyGrpcClients;