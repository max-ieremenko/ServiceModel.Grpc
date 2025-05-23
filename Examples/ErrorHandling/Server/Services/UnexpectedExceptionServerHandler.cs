﻿using System;
using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Server.Services;

// this handler is responsible for processing InvalidOperationException and NotSupportedException on server-side
internal sealed class UnexpectedExceptionServerHandler : IServerErrorHandler
{
    public ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
    {
        if (error is NotSupportedException || error is InvalidOperationException)
        {
            // provide detailed information for the client error handler
            var detail = new UnexpectedErrorDetail
            {
                Message = error.Message,
                ExceptionType = error.GetType()?.FullName,
                FullException = error.ToString(),
                MethodName = context.ServerCallContext.Method
            };

            return new ServerFaultDetail { Detail = detail };
        }

        // ignore other exceptions
        return null;
    }
}