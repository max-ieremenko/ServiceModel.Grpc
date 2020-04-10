using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal delegate Task<TResponse> UnaryServerMethod<in TService, in TRequest, TResponse>(
        TService service,
        TRequest request,
        ServerCallContext context)
        where TRequest : class
        where TResponse : class;
}
