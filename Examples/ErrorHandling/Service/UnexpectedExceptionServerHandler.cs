using System;
using System.Diagnostics;
using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Service
{
    // this handler is responsible to process InvalidOperationException and NotSupportedException on server-side
    public sealed class UnexpectedExceptionServerHandler : IServerErrorHandler
    {
        public ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
        {
            if (error is NotSupportedException || error is InvalidOperationException)
            {
                // provide detailed information for the client error handler
                var detail = new UnexpectedErrorDetail
                {
                    Message = error.Message,
                    ExceptionType = error.GetType().FullName,
                    FullException = error.ToString(),
                    MethodName = Process.GetCurrentProcess().ProcessName
                };

                return new ServerFaultDetail { Detail = detail };
            }

            // ignore other exceptions
            return null;
        }
    }
}