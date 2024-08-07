# ServiceModel.Grpc server code generation

## Reflection.Emit by default

By default, all endpoints requested by gRPC service host are generated on demand via Reflection.Emit.
Reflection.Emit endpoint class is generated only once for the specific service.

example AspNetCore hosting:

``` c#
var app = WebApplication.CreateBuilder().Build();

// under the hood:
//   - generate gRPC endpoint class for MyService
//   - bind the service
app.MapGrpcService<MyService>();
```

example Grpc.Core.Server hosting:

``` c#
var server = new Grpc.Core.Server();

// under the hood:
//   - generate gRPC endpoint class for MyService
//   - bind the service
server.Services.AddServiceModelTransient(() => new MyService());
```

## C# source code generator

To enable source code generation:

- add reference to the package [ServiceModel.Grpc.DesignTime](https://www.nuget.org/packages/ServiceModel.Grpc.DesignTime)
- create a static partial class, the name doesn't matter
- the class is a placeholder for generated source code
- configure which endpoints should be generated via `ExportGrpcServiceAttribute`

AspNetCore hosting:

``` c#
[ExportGrpcService(typeof(MyService), GenerateAspNetExtensions = true)]
internal static partial class MyGrpcServices
{
    // generated code ...
    public static IServiceCollection AddMyServiceOptions(this IServiceCollection services, Action<ServiceModelGrpcServiceOptions<MyService>> configure) {}

    // generated code ...
    public static GrpcServiceEndpointConventionBuilder MapMyService(this IEndpointRouteBuilder builder) {}
}

var builder = WebApplication.CreateBuilder();

// optional configuration for MyService endpoint
builder.Services.AddMyServiceOptions(options => { /*...*/ });

var app = builder.Build();

// host MyService endpoint generated by ServiceModel.Grpc.DesignTime
app.MapMyService();
```

Grpc.Core.Server hosting:

``` c#
[ExportGrpcService(typeof(MyService), GenerateSelfHostExtensions = true)]
internal static partial class MyGrpcServices
{
    // generated code ...
    public static Server.ServiceDefinitionCollection AddMyService(this Server.ServiceDefinitionCollection services, Func<MyService> serviceFactory, Action<ServiceModelGrpcServiceOptions> configure = default) {}

    // generated code ...
    public static Server.ServiceDefinitionCollection AddMyService(this Server.ServiceDefinitionCollection services, MyService service, Action<ServiceModelGrpcServiceOptions> configure = default) {}
}

var server = new Grpc.Core.Server();

// host MyService endpoint generated by ServiceModel.Grpc.DesignTime
server.Services.AddMyService(() => new MyService());
```
