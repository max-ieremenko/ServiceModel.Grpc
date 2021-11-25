# ServiceModel.Grpc ASP.NET Core server configuration

## Startup.cs

To configure services use ConfigureServices method.
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
        });

        // enable ServiceModel.Grpc
        services
            .AddServiceModelGrpc(options =>
            {
                options.DefaultMarshallerFactory = ...
                options.DefaultErrorHandlerFactory = ...
                options.Filters ...
            });

        // optional configuration for a specific service
        services
            .AddServiceModelGrpcServiceOptions<MyService>(options =>
            {
                options.MarshallerFactory = ...
                options.ErrorHandlerFactory = ...
                options.Filters ...
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

## Bind a service via contract or abstract class

``` c#
// contract
[ServiceContract]
public interface IMyService { }

// implementation 1
[Authorize]
internal sealed class MyService1 : IMyService {}

// implementation 2
[AllowAnonymous]
internal sealed class MyService2 : IMyService {}
```

To bind and configure the service:

- register `IMyService` implementation in your current dependency injection framework
- to register a configuration use the interface `IMyService`
- to bind the service use the interface `IMyService`

``` c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // dependency injection registration
        if (...)
        {
            services.AddTransient<IMyService, MyService1>();
        }
        else
        {
            services.AddTransient<IMyService, MyService2>();
        }

        // optional configuration, use interface
        services
            .AddServiceModelGrpcServiceOptions<IMyService>(options =>
            {
                // ...
            });
    }

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

At runtime, on gRPC service binding, ServiceModel.Grpc resolves the implementation of `IMyService` via current `IServiceProvider` in order to get the implementation type:

``` c#
IServiceProvider currentProvider;
Type implementationType = currentProvider.GetRequiredService<IMyService>().GetType();
```

The implementation type is used for service binding.

## Silent proxy generation (Reflection.Emit)

In this example SumAsync and Dispose will be ignored during the service binding, use log output to see warnings:

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
