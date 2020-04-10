using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.Emit
{
    public delegate Task DuplexStreamingServerMethod<in TService, in TRequest, out TResponse>(
        TService service,
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context)
        where TRequest : class
        where TResponse : class;
}
