# ServiceModel.Grpc.MessagePackMarshaller

`ServiceModel.Grpc` enables applications to communicate with gRPC services using a code-first approach (no [.proto files](https://learn.microsoft.com/en-us/aspnet/core/grpc/basics#proto-file)), helps to get around limitations of gRPC protocol like "only reference types", "exact one input", "no nulls", "no value-types". Provides exception handling. Helps to migrate existing WCF solution to gRPC with minimum effort.

`ServiceModel.Grpc.MessagePackMarshaller` is package with `IMarshallerFactory` implementation, based on [MessagePack serializer](https://github.com/MessagePack-CSharp/MessagePack-CSharp).

## Configure ClientFactory

Instruct `ClientFactory` to use `MessagePackMarshallerFactory` as default marshaller for all clients.

```csharp
IClientFactory defaultClientFactory
    = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        // set MessagePackMarshaller as default Marshaller
        MarshallerFactory = MessagePackMarshallerFactory.Default
    });
```

Instruct `ClientFactory` to use `MessagePackMarshallerFactory` for concrete client.

```csharp
// client factory with default (DataContractMarshallerFactory) marshaller
IClientFactory defaultClientFactory = new ClientFactory();

// set MessagePackMarshaller only for ICalculator client
defaultClientFactory.AddClient<ICalculator>(options =>
{
    options.MarshallerFactory = MessagePackMarshallerFactory.Default;
});
```

## Configure [ServiceModel.Grpc.AspNetCore](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore)

Instruct ServiceModel.Grpc code-first to use `MessagePackMarshallerFactory` as default marshaller for all endpoints.

```csharp
var builder = WebApplication.CreateBuilder();

// enable ServiceModel.Grpc code-first
builder.Services.AddServiceModelGrpc(options =>
{
    // set MessagePackMarshaller as default Marshaller
    options.DefaultMarshallerFactory = MessagePackMarshallerFactory.Default;
});
```

Instruct ServiceModel.Grpc code-first to use `MessagePackMarshallerFactory` for concrete endpoint.

```csharp
var builder = WebApplication.CreateBuilder();

// enable ServiceModel.Grpc code-first with default (DataContractMarshallerFactory) marshaller
builder.Services.AddServiceModelGrpc();

// set MessagePackMarshaller only for Calculator endpoint
builder.Services.AddServiceModelGrpcServiceOptions<Calculator>(options =>
{
    options.MarshallerFactory = MessagePackMarshallerFactory.Default;
});
```

## Configure [ServiceModel.Grpc.SelfHost](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost)

Instruct ServiceModel.Grpc code-first to use `MessagePackMarshallerFactory`.

```csharp
var server = new Grpc.Core.Server();

// set MessagePackMarshaller for Calculator endpoint
server.Services.AddServiceModelTransient(
    ()=> new Calculator(),
    options =>
    {
        options.MarshallerFactory = MessagePackMarshallerFactory.Default;
    });
```

## Links

- [Documentation](https://max-ieremenko.github.io/ServiceModel.Grpc)
- [ServiceModel.Grpc GitHub](https://github.com/max-ieremenko/ServiceModel.Grpc)
- [Example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MessagePackMarshaller)
- [Example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MessagePackMarshaller.AOT)
