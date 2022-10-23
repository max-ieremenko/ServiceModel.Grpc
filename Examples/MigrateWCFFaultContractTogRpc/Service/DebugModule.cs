using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Interceptors;

namespace Service;

public static class DebugModule
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DebugService>();

        services.AddSingleton<IServerErrorHandler, FaultExceptionServerHandler>();
    }
}