using BlazorApp.Shared;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.DesignTime;

namespace BlazorApp.Client;

// instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[ImportGrpcService(typeof(IWeatherForecastService))]
internal static partial class GrpcClients
{
    // register generated IWeatherForecastService client to avoid Reflection.Emit at runtime in the browser
    public static void AddAllClients(IClientFactory clientFactory)
    {
        clientFactory.AddWeatherForecastServiceClient();
    }
}