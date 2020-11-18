using Contract;
using Service;
using ServiceModel.Grpc.DesignTime;

namespace Demo.SelfHost.DesignTime
{
    [ImportGrpcService(typeof(IPersonService))] // configure ServiceModel.Grpc.DesignTime to generate a source code for IPersonService client proxy
    [ExportGrpcService(typeof(PersonService), GenerateSelfHostExtensions = true)] // configure ServiceModel.Grpc.DesignTime to generate a source code for PersonService endpoint
    internal static partial class MyGrpcServices
    {
    }
}
