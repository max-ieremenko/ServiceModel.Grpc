using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Client.Shared;

public sealed class UnexpectedExceptionClientHandler : IClientErrorHandler
{
    public void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail)
    {
        // if marker is UnexpectedErrorDetail
        if (detail.Detail is UnexpectedErrorDetail unexpectedErrorDetail)
        {
            // throw custom exception
            throw new UnexpectedErrorException(unexpectedErrorDetail);
        }
    }
}