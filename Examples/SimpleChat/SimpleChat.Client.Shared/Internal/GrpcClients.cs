using ServiceModel.Grpc.DesignTime;
using SimpleChat.Shared;

namespace SimpleChat.Client.Shared.Internal;

// instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[ImportGrpcService(typeof(IChatService), GenerateDependencyInjectionExtensions = true)]
[ImportGrpcService(typeof(IAccountService), GenerateDependencyInjectionExtensions = true)]
internal static partial class GrpcClients;