# ServiceModel.Grpc.ProtoBufMarshaller

`ServiceModel.Grpc` enables applications to communicate with gRPC services using a code-first approach (no [.proto files](https://learn.microsoft.com/en-us/aspnet/core/grpc/basics#proto-file)), helps to get around limitations of gRPC protocol like "only reference types", "exact one input", "no nulls", "no value-types". Provides exception handling. Helps to migrate existing WCF solution to gRPC with minimum effort.

`ServiceModel.Grpc.ProtoBufMarshaller` is package with `IMarshallerFactory` implementation, based on [protobuf-net serializer](https://github.com/protobuf-net/protobuf-net).

## Configure ClientFactory

Instruct `ClientFactory` to use `ProtobufMarshallerFactory` as default marshaller for all clients.

```csharp
IClientFactory defaultClientFactory
    = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        // set ProtobufMarshaller as default Marshaller
        MarshallerFactory = ProtobufMarshallerFactory.Default
    });
```

Instruct `ClientFactory` to use `ProtobufMarshallerFactory` for concrete client.

```csharp
// client factory with default (DataContractMarshallerFactory) marshaller
IClientFactory defaultClientFactory = new ClientFactory();

// set ProtobufMarshaller only for ICalculator client
defaultClientFactory.AddClient<ICalculator>(options =>
{
    options.MarshallerFactory = ProtobufMarshallerFactory.Default;
});
```

## Configure [ServiceModel.Grpc.AspNetCore](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore)

Instruct ServiceModel.Grpc code-first to use `ProtobufMarshallerFactory` as default marshaller for all endpoints.

```csharp
var builder = WebApplication.CreateBuilder();

// enable ServiceModel.Grpc code-first
builder.Services.AddServiceModelGrpc(options =>
{
    // set ProtobufMarshaller as default Marshaller
    options.DefaultMarshallerFactory = ProtobufMarshallerFactory.Default;
});
```

Instruct ServiceModel.Grpc code-first to use `ProtobufMarshallerFactory` for concrete endpoint.

```csharp
var builder = WebApplication.CreateBuilder();

// enable ServiceModel.Grpc code-first with default (DataContractMarshallerFactory) marshaller
builder.Services.AddServiceModelGrpc();

// set ProtobufMarshaller only for Calculator endpoint
builder.Services.AddServiceModelGrpcServiceOptions<Calculator>(options =>
{
    options.MarshallerFactory = ProtobufMarshallerFactory.Default;
});
```

## Configure [ServiceModel.Grpc.SelfHost](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost)

Instruct ServiceModel.Grpc code-first to use `ProtobufMarshallerFactory`.

```csharp
var server = new Grpc.Core.Server();

// set ProtobufMarshaller for Calculator endpoint
server.Services.AddServiceModelTransient(
    ()=> new Calculator(),
    options =>
    {
        options.MarshallerFactory = ProtobufMarshallerFactory.Default;
    });
```

## Links

- [Documentation](https://max-ieremenko.github.io/ServiceModel.Grpc)
- [ServiceModel.Grpc GitHub](https://github.com/max-ieremenko/ServiceModel.Grpc)
- [Example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ProtobufMarshaller)