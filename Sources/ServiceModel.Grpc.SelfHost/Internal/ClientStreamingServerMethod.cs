using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal delegate Task<TResponse> ClientStreamingServerMethod<in TService, in TRequest, TResponse>(
        TService service,
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context)
        where TRequest : class
        where TResponse : class;
}
