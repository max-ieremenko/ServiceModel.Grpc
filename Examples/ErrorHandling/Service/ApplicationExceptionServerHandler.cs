using System;
using ServiceModel.Grpc.Interceptors;

namespace Service
{
    // this handler is responsible to process ApplicationException on server-side
    public sealed class ApplicationExceptionServerHandler : IServerErrorHandler
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
}
