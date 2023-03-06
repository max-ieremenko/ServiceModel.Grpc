# ServiceModel.Grpc ASP.NET Core server configuration

## Startup.cs

To configure services use the ConfigureServices method.
To bind service use Grpc.AspNetCore.Server binding in Configure method.

``` c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Grpc.AspNetCore.Server configuration
        services.AddGrpc(options =>
        {
            options.ResponseCompressionLevel = CompressionLevel.Optimal;
            // ...
        });

        // enable ServiceModel.Grpc
        services
            .AddServiceModelGrpc(options =>
            {
                options.DefaultMarshallerFactory = ...
                options.DefaultErrorHandlerFactory = ...
                options.Filters = ...
            });

        // optional configuration for a specific service
        services
            .AddServiceModelGrpcServiceOptions<MyService>(options =>
            {
                options.MarshallerFactory = ...
                options.ErrorHandlerFactory = ...
                options.Filters = ...
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

- IMarshallerFactory DefaultMarshallerFactory: by default is null (DataContractMarshallerFactory.Default)
- Func<IServiceProvider, IServerErrorHandler> DefaultErrorHandlerFactory: by default is null (error handling by gRPC API)
- FilterCollection\<IServerFilter\> Filters: list of global server filters

## Service binding

``` c#
// contract
[ServiceContract]
public interface IMyService { }

// implementation
// [Authorize]
// [AllowAnonymous]
internal sealed class MyService : IMyService {}
```

There are two options to bind a service:
- option 1: via implementation `MyService`
- option 2: via interface `IMyService`

``` c#
public class Startup
{
    // option 1: via implementation `MyService`
    public void ConfigureServices(IServiceCollection services)
    {
        // dependency injection
        services.AddTransient<MyService>();

        // optional configuration
        services
            .AddServiceModelGrpcServiceOptions<MyService>(options =>
            {
                // ...
            });
    }

    // option 1: via implementation `MyService`
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseEndpoints(endpoints =>
        {
            // bind the service
            endpoints.MapGrpcService<MyService>();
        });
    }

    // option 2: via interface `IMyService`
    public void ConfigureServices(IServiceCollection services)
    {
        // dependency injection
        services.AddTransient<IMyService, MyService>();

        // optional configuration
        services
            .AddServiceModelGrpcServiceOptions<IMyService>(options =>
            {
                // ...
            });
    }

    // option 2: via interface `IMyService`
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseEndpoints(endpoints =>
        {
            // bind the service, use interface
            endpoints.MapGrpcService<IMyService>();
        });
    }
}
```

In the case of option 2, at runtime, ServiceModel.Grpc resolves the implementation of `IMyService` via the current `IServiceProvider` in order to get the implementation type:

``` c#
IServiceProvider currentProvider;
Type implementationType = currentProvider.GetRequiredService<IMyService>().GetType();
```