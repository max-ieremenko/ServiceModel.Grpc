using System;
using System.Diagnostics;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ServiceModel.Grpc.Client;
using BlazorApp.Shared;

namespace ConsoleApp.Client;

public static class Program
{
    public static async Task Main()
    {
        // using HTTP 1.1 connection as Blazor client does, see BlazorApp.Client/Program.cs
        await DemoHttp11("http://localhost:5000");

        // using HTTP 2.0, see BlazorApp.Server/appsettings.json, gRPC endpoint
        await DemoHttp2("http://localhost:5001");

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task DemoHttp11(string address)
    {
        var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
        httpHandler.HttpVersion = System.Net.HttpVersion.Version11;

        using var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = httpHandler });

        var client = new ClientFactory().CreateClient<IWeatherForecastService>(channel);

        await FetchData(client, "HTTP 1.1");
        await Streaming(client, "HTTP 1.1");
    }

    private static async Task DemoHttp2(string address)
    {
        using var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions());

        var client = new ClientFactory().CreateClient<IWeatherForecastService>(channel);

        await FetchData(client, "HTTP 2.0");
        await Streaming(client, "HTTP 2.0");
    }

    private static async Task FetchData(IWeatherForecastService client, string demoName)
    {
        Console.WriteLine($"$--- FetchData {demoName} ---");

        var forecasts = await client.GetForecasts();
        foreach (var forecast in forecasts)
        {
            Console.WriteLine($"{forecast.Date.ToShortDateString()}, {forecast.TemperatureC}C {forecast.TemperatureF}F, {forecast.Summary}");
        }
    }

    private static async Task Streaming(IWeatherForecastService client, string demoName)
    {
        Console.WriteLine($"--- Streaming {demoName} ---");

        var counter = 1;
        await foreach (var forecast in client.StartForecast(CancellationToken.None))
        {
            Console.WriteLine($"{counter}: {forecast.Date.ToShortDateString()}, {forecast.TemperatureC}C {forecast.TemperatureF}F, {forecast.Summary}");
            
            if (counter++ == 5)
            {
                break;
            }
        }
    }
}