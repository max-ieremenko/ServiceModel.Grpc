# ServiceModel.Grpc.Nerdbank.MessagePackMarshaller

`ServiceModel.Grpc` enables applications to communicate with gRPC services using a code-first approach (no [.proto files](https://learn.microsoft.com/en-us/aspnet/core/grpc/basics#proto-file)), helps to get around limitations of gRPC protocol like "only reference types", "exact one input", "no nulls", "no value-types". Provides exception handling. Helps to migrate existing WCF solution to gRPC with minimum effort.

`ServiceModel.Grpc.Nerdbank.MessagePackMarshaller` is package with `IMarshallerFactory` implementation, based on [Nerdbank.MessagePack serializer](https://github.com/AArnott/Nerdbank.MessagePack).

## Configure ClientFactory

Instruct `ClientFactory` to use `NerdbankMessagePackMarshallerFactory` as default marshaller for all clients.

```csharp
IClientFactory defaultClientFactory
    = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        // set NerdbankMessagePackMarshaller as default Marshaller
        MarshallerFactory = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<TShapeable>();
    });
```

Instruct `ClientFactory` to use `NerdbankMessagePackMarshallerFactory` for concrete client.

```csharp
// client factory with default (DataContractMarshallerFactory) marshaller
IClientFactory defaultClientFactory = new ClientFactory();

// set NerdbankMessagePackMarshaller only for ICalculator client
defaultClientFactory.AddClient<ICalculator>(options =>
{
    options.MarshallerFactory = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<TShapeable>();
});
```

## Configure [ServiceModel.Grpc.AspNetCore](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore)

Instruct ServiceModel.Grpc code-first to use `NerdbankMessagePackMarshallerFactory` as default marshaller for all endpoints.

```csharp
var builder = WebApplication.CreateBuilder();

// enable ServiceModel.Grpc code-first
builder.Services.AddServiceModelGrpc(options =>
{
    // set NerdbankMessagePackMarshaller as default Marshaller
    options.DefaultMarshallerFactory = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<TShapeable>();
});
```

Instruct ServiceModel.Grpc code-first to use `NerdbankMessagePackMarshallerFactory` for concrete endpoint.

```csharp
var builder = WebApplication.CreateBuilder();

// enable ServiceModel.Grpc code-first with default (DataContractMarshallerFactory) marshaller
builder.Services.AddServiceModelGrpc();

// set NerdbankMessagePackMarshaller only for Calculator endpoint
builder.Services.AddServiceModelGrpcServiceOptions<Calculator>(options =>
{
    options.MarshallerFactory = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<TShapeable>();
});
```

## Configure [ServiceModel.Grpc.SelfHost](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost)

Instruct ServiceModel.Grpc code-first to use `NerdbankMessagePackMarshallerFactory`.

```csharp
var server = new Grpc.Core.Server();

// set NerdbankMessagePackMarshaller for Calculator endpoint
server.Services.AddServiceModelTransient(
    ()=> new Calculator(),
    options =>
    {
        options.MarshallerFactory = new NerdbankMessagePackMarshallerFactory(TypeShapeProvider);
    });
```

## Links

- [Documentation](https://max-ieremenko.github.io/ServiceModel.Grpc)
- [ServiceModel.Grpc GitHub](https://github.com/max-ieremenko/ServiceModel.Grpc)
- [Example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/NerdbankMessagePackMarshaller.AOT)
