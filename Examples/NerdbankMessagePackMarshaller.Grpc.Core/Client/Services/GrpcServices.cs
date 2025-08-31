using Contract;
using ServiceModel.Grpc.DesignTime;

namespace Client.Services;

[ImportGrpcService(typeof(ICalculator))] // instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[NerdbankMessagePackDesignTimeExtension] // instruct ServiceModel.Grpc.Nerdbank.MessagePackMarshaller to generate required code during the build process
internal static partial class GrpcServices;