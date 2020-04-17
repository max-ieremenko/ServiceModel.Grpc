# ServiceModel.Grpc

- getting started example [create a gRPC client and server](CreateClientAndServerASPNETCore.md)
- [service and operation names](ServiceAndOperationName.md)
- [service and operation bindings](ServiceAndOperationBinding.md)
- [client configuration](ClientConfiguration.md)
- [ASP.NET Core server configuration](ASPNETCoreServerConfiguration.md)
- [Grpc.Core server configuration](GrpcCoreServerConfiguration.md)
- [compatibility with native gRPC](CompatibilityWithNativegRPC.md)
- example [protobuf marshaller](/Examples/ProtobufMarshaller)

## Data contracts

By default the DataContractSerializer is used for marshalling data between server an client. This behavior is configurable, see [ProtobufMarshaller example](/Examples/ProtobufMarshaller).

``` c#
[DataContract]
public class Person
{
    [DataMember]
    public string Name { get; set;}

    [DataMember]
    public DateTime BirthDay { get; set;}
}
```

## Service contracts

Service contract is a public interface marked with ServiceContractAttribute.
Methods marked with OperationContractAttribute are gRPC calls.

> for net461 System.ServiceModel.dll, for netstandard package System.ServiceModel.Primitives

``` c#
[ServiceContract]
public interface IPersonService
{
    // gRPC call
    [OperationContract]
    Task Ping();

    // method is not gRPC call
    Task Ping();
}
```

## Operation contracts

Any operation in a service contract under the hood is one of gRPC method: Unary, ClientStreaming, ServerStreaming or DuplexStreaming.

#### Unary operation

Response is optional, any number of request parameters

``` c#
// blocking unary client call
[OperationContract]
int Sum(int x, int y);

// async unary client call
[OperationContract]
Task<string> SumAsync(int x, int y);
```

#### ClientStreaming operation

Response is optional

``` c#
// call is compatible with native gRPC
[OperationContract]
Task<long> SumValues(IAsyncEnumerable<int> values);
```

ServiceModel.Grpc supports any number of extra request parameters, but this call is not fully compatible with native gRPC call.

``` c#
// call is not fully compatible with native gRPC
[OperationContract]
Task<long> MultiplyByAndSumValues(IAsyncEnumerable<int> values, int multiplier);
```

#### ServerStreaming operation

Any number of request parameters

``` c#
[OperationContract]
IAsyncEnumerable<int> EnumerableRange(int start, int count);
```

#### DuplexStreaming operation

``` c#
// call is compatible with native gRPC
[OperationContract]
IAsyncEnumerable<int> MultiplyBy2(IAsyncEnumerable<int> values);
```

ServiceModel.Grpc supports any number of extra request parameters, but this call is not fully compatible with native gRPC call.

``` c#
// call is not fully compatible with native gRPC
[OperationContract]
IAsyncEnumerable<int> MultiplyBy(IAsyncEnumerable<int> values, int multiplier);
```

#### Context parameters

ServiceModel.Grpc.CallContext

``` c#
// contract
[OperationContract]
Task Ping(CallContext context = default);

// client
await client.Ping(new CallOptions(....));

// server
Task Ping(CallContext context)
{
    // take ServerCallContext
    Grpc.Core.ServerCallContext serverContext = context;
    var token = serverContext.CancellationToken;
    var requestHeaders = serverContext.RequestHeaders;
}
```

Grpc.Core.CallOptions

``` c#
// contract
[OperationContract]
Task Ping(CallOptions context = default);

// client
await client.Ping(new CallOptions(....));

// server
Task Ping(CallOptions context)
{
    // context is not available here (default)
}
```

Grpc.Core.ServerCallContext

``` c#
// contract
[OperationContract]
Task Ping(ServerCallContext context = default);

// client
await client.Ping();

// server
Task Ping(ServerCallContext context)
{
    var token = context.CancellationToken;
    var requestHeaders = context.RequestHeaders;
}
```

System.Threading.CancellationToken

``` c#
// contract
[OperationContract]
Task Ping(CancellationToken token = default);

// client
var tokenSource = new CancellationTokenSource();
await client.Ping(tokenSource.Token);

// server
Task Ping(CancellationToken token)
{
    if (!token.IsCancellationRequested)
    {
        // ...
    }
}
```

## Limitations

- generic methods are not supported
- ref and out parameters are not supported
