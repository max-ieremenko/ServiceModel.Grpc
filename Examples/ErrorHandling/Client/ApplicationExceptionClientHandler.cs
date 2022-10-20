using System;
using ServiceModel.Grpc.Interceptors;

namespace Client;

internal sealed class ApplicationExceptionClientHandler : IClientErrorHandler
{
    public void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail)
    {
        // if marker is ApplicationException
        if ((detail.Detail is string name) && name == "ApplicationException")
        {
            // throw custom exception
            throw new ApplicationException(detail.OriginalError.Status.Detail);
        }
    }
}