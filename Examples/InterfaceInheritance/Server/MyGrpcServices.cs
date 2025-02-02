using Server.Services;
using ServiceModel.Grpc.DesignTime;

namespace Server;

// configure ServiceModel.Grpc.DesignTime to generate a source code for IGenericCalculator<int> endpoint
[ExportGrpcService(typeof(GenericCalculator<int>), GenerateAspNetExtensions = true)]
internal static partial class MyGrpcServices;