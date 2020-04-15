# ServiceModel.Grpc ASP.NET Core server configuration

## Startup.cs

To configure services use ConfigureServices method.
To bind service, use Grpc.AspNetCore.Server binding in Configure method.

``` c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Grpc.AspNetCore.Server configuration
        services.AddGrpc(options =>
        {
            options.ResponseCompressionLevel = CompressionLevel.Optimal;
        });

        // enable ServiceModel.Grpc
        services
            .AddServiceModelGrpc(options =>
            {
                options.DefaultMarshallerFactory = ...
            });

        // optional configuration for a specific service
        services
            .AddServiceModelGrpcServiceOptions<ICalculator>(options =>
            {
                options.MarshallerFactory = ...
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            // bind the service
            endpoints.MapGrpcService<MyService>();
        });
    }
}
```

#### ServiceModelGrpcServiceOptions
- IMarshallerFactory DefaultMarshallerFactory: by default is null, it means DataContractMarshallerFactory.Default

## Silent proxy generation

In this example

``` c#
[ServiceContract]
public interface IContract
{
    [OperationContract]
    Task<T> SumAsync<T>(T x, T y);

    void Dispose();
}

internal sealed class Service : IContract { }
```

SumAsync and Dispose will be ignored during the service binding, use log output to see warnings.
