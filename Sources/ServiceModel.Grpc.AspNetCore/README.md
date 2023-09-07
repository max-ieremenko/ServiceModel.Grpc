# ServiceModel.Grpc.AspNetCore

`ServiceModel.Grpc` enables applications to communicate with gRPC services using a code-first approach (no [.proto files](https://learn.microsoft.com/en-us/aspnet/core/grpc/basics#proto-file)), helps to get around limitations of gRPC protocol like "only reference types", "exact one input", "no nulls", "no value-types". Provides exception handling. Helps to migrate existing WCF solution to gRPC with minimum effort.

`ServiceModel.Grpc.AspNetCore` is package with code-first extensions for [Grpc.AspNetCore.Server](https://www.nuget.org/packages/Grpc.AspNetCore.Server).

## Declare a service contract and implement service

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

internal sealed class Calculator : ICalculator
{
    public Task<long> Sum(long x, int y, int z, CancellationToken token) => x + y + z;

    public ValueTask<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token)
    {
        var multiplicationResult = DoMultiplication(values, multiplier, token);
        return new ValueTask<(int, IAsyncEnumerable<int>)>((multiplier, multiplicationResult));
    }

    private static async IAsyncEnumerable<int> DoMultiplication(IAsyncEnumerable<int> values, int multiplier, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var value in values.WithCancellation(token))
        {
            yield return value * multiplier;
        }
    }
}
```

## Configure service

Enable ServiceModel.Grpc code-first and add service to the routing pipeline.

```csharp
var builder = WebApplication.CreateBuilder();

// optional Grpc.AspNetCore.Server configuration
builder.Services.AddGrpc(options =>
{
    // ...
});

// enable ServiceModel.Grpc code-first
builder.Services.AddServiceModelGrpc(options =>
{
    options.DefaultMarshallerFactory = ...
    options.DefaultErrorHandlerFactory = ...
    options.Filters = ...
});

var app = builder.Build();

// bind the service
app.MapGrpcService<Calculator>();

app.Run();
```

## Links

- [Documentation](https://max-ieremenko.github.io/ServiceModel.Grpc)
- [ServiceModel.Grpc GitHub](https://github.com/max-ieremenko/ServiceModel.Grpc)
- [Examples](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples)
