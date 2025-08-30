using System;
using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Server.Services;

/// <summary>
/// Error handling: provide <see cref="InvalidRectangleException"/> details via <see cref="InvalidRectangleError"/>
/// </summary>
internal sealed class ServerErrorHandler : IServerErrorHandler
{
    public ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
    {
        if (error is InvalidRectangleException ex)
        {
            var detail = new InvalidRectangleError(ex.Message, ex.Points);
            return new ServerFaultDetail { Detail = detail };
        }

        return null;
    }
}