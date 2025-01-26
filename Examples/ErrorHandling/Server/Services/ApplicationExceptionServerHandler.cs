using System;
using ServiceModel.Grpc.Interceptors;

namespace Server.Services;

// this handler is responsible for processing ApplicationException on server-side
internal sealed class ApplicationExceptionServerHandler : IServerErrorHandler
{
    public ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
    {
        if (error is ApplicationException)
        {
            // provide a marker for the client exception handler
            return new ServerFaultDetail { Detail = "ApplicationException" };
        }

        // ignore other exceptions
        return null;
    }
}