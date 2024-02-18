# ServiceModel.Grpc ASP.NET Core server configuration

## Program.cs

To configure services use the ConfigureServices method.
To bind service use Grpc.AspNetCore.Server binding in Configure method.

``` c#
var builder = WebApplication.CreateBuilder();

// Grpc.AspNetCore.Server configuration
builder.Services.AddGrpc(options =>
{
    options.ResponseCompressionLevel = CompressionLevel.Optimal;
    // ...
});

// enable ServiceModel.Grpc
builder.Services
    .AddServiceModelGrpc(options =>
    {
        options.DefaultMarshallerFactory = ...
        options.DefaultErrorHandlerFactory = ...
        options.Filters = ...
    });

// optional configuration for a specific service
builder.Services
    .AddServiceModelGrpcServiceOptions<MyService>(options =>
    {
        options.MarshallerFactory = ...
        options.ErrorHandlerFactory = ...
        options.Filters = ...
    });

var app = builder.Build();

// bind the service
app.MapGrpcService<MyService>();
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
var builder = WebApplication.CreateBuilder();

// option 1: via implementation `MyService`
{
    // dependency injection
    builder.Services.AddTransient<MyService>();

    // optional configuration
    builder.Services
        .AddServiceModelGrpcServiceOptions<MyService>(options =>
        {
            // ...
        });

    var app = builder.Build();

    // bind the service
    app.MapGrpcService<MyService>();
}

// option 2: via interface `IMyService`
{
    // dependency injection
    builder.Services.AddTransient<IMyService, MyService>();

    // optional configuration
    builder.Services
        .AddServiceModelGrpcServiceOptions<IMyService>(options =>
        {
            // ...
        });

    var app = builder.Build();

    // bind the service, use interface
    app.MapGrpcService<IMyService>();
}
```

In the case of option 2, at runtime, ServiceModel.Grpc resolves the implementation of `IMyService` via the current `IServiceProvider` in order to get the implementation type:

``` c#
IServiceProvider currentProvider;
Type implementationType = currentProvider.GetRequiredService<IMyService>().GetType();
```