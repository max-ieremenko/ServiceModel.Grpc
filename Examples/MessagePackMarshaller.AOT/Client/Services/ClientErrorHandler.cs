using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Client.Services;

/// <summary>
/// Error handling: raise <see cref="InvalidRectangleException"/> with details from <see cref="InvalidRectangleError"/>
/// </summary>
internal sealed class ClientErrorHandler : IClientErrorHandler
{
    public void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail)
    {
        if (detail.Detail is InvalidRectangleError error)
        {
            throw new InvalidRectangleException(error.Message, detail.OriginalError, error.Points);
        }
    }
}