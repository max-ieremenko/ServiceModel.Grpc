# ServiceModel.Grpc.SelfHost

`ServiceModel.Grpc` enables applications to communicate with gRPC services using a code-first approach (no [.proto files](https://learn.microsoft.com/en-us/aspnet/core/grpc/basics#proto-file)), helps to get around limitations of gRPC protocol like "only reference types", "exact one input", "no nulls", "no value-types". Provides exception handling. Helps to migrate existing WCF solution to gRPC with minimum effort.

`ServiceModel.Grpc.DesignTime` is package with C# code generator for client and server-side assets.

## Declare a service contract

A service contract is an interface decorated with the `ServiceContract` attribute. Interface methods decorated with the `OperationContract` attribute are treated as gRPC operations.

For example, following `ICalculator` contract with unary `Sum` operation and client streaming `MultiplyBy` operation:

```csharp
[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    Task<long> Sum(long x, int y, int z, CancellationToken token = default);

    [OperationContract]
    ValueTask<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token = default);
}
```

## Configure code generator for client proxies

For source code generation create a placeholder - a static partial class, name doesn’t matter. Configure which proxies should be generated via `ImportGrpcService` attribute.

```csharp
[ImportGrpcService(typeof(ICalculator))]
internal static partial class MyGrpcServices
{
    // generated code ...
    public static IClientFactory AddCalculatorClient(this IClientFactory clientFactory, Action<ServiceModelGrpcClientOptions> configure = null) {}
}
```

Instruct `ClientFactory` to use a proxy from generated code.

```csharp
IClientFactory clientFactory = new ClientFactory();

// register generated proxy for ICalculator
clientFactory.AddCalculatorClient();

// create an instance
ICalculator calculator = clientFactory.CreateClient<ICalculator>(channel);
```

## Configure code generator for server endpoints

Implement service.

```csharp
internal sealed class Calculator : ICalculator
{
    public Task<long> Sum(long x, int y, int z, CancellationToken token)
    {
        ...
    }

    public ValueTask<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token)
    {
        ...
    }
}
```

For source code generation create a placeholder - a static partial class, name doesn’t matter. Configure which endpoints should be generated via `ExportGrpcService` attribute.

### Generate code with extensions for [ServiceModel.Grpc.AspNetCore](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore).

```csharp
[ExportGrpcService(typeof(Calculator), GenerateAspNetExtensions = true)]
internal static partial class MyGrpcServices
{
    // generated code ...
    public static IServiceCollection AddCalculatorOptions(this IServiceCollection services, Action<ServiceModelGrpcServiceOptions<Calculator>> configure) {}

    // generated code ...
    public static GrpcServiceEndpointConventionBuilder MapCalculator(this IEndpointRouteBuilder builder) {}
}
```

Instruct `Grpc.AspNetCore.Server` to use a proxy from generated code.

```csharp
var builder = WebApplication.CreateBuilder();

// optional configuration for Calculator endpoint
builder.Services.AddCalculatorOptions(options => { ... });

var app = builder.Build();

// bind generated Calculator endpoint
app.MapCalculator();
```

### Generate code with extensions for [ServiceModel.Grpc.SelfHost](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost).

```csharp
[ExportGrpcService(typeof(Calculator), GenerateSelfHostExtensions = true)]
internal static partial class MyGrpcServices
{
    // generated code ...
    public static Server.ServiceDefinitionCollection AddCalculator(this Server.ServiceDefinitionCollection services, Func<Calculator> serviceFactory, Action<ServiceModelGrpcServiceOptions> configure = default) {}

    // generated code ...
    public static Server.ServiceDefinitionCollection AddCalculator(this Server.ServiceDefinitionCollection services, Calculator service, Action<ServiceModelGrpcServiceOptions> configure = default) {}
}
```

Instruct `Grpc.Core.Server` to use a proxy from generated code.

```csharp
var server = new Grpc.Core.Server();

// bind generated Calculator endpoint with configuration
server.Services.AddCalculator(
    () => new Calculator(),
    options => { ... });

// or bind generated Calculator endpoint with default configuration
server.Services.AddCalculator(() => new Calculator());
```

## Links

- [Documentation](https://max-ieremenko.github.io/ServiceModel.Grpc)
- [ServiceModel.Grpc GitHub](https://github.com/max-ieremenko/ServiceModel.Grpc)
- [Examples](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples)
