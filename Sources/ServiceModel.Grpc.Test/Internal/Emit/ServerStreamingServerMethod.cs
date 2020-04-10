using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal delegate Task ServerStreamingServerMethod<in TService, in TRequest, out TResponse>(
        TService service,
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context)
        where TRequest : class
        where TResponse : class;
}
