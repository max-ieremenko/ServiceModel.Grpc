# ServiceModel.Grpc

`ServiceModel.Grpc` enables applications to communicate with gRPC services using code-first approach, helps to get around some limitations of gRPC protocol like "only reference types", "exact one input", "no nulls".
Helps to migrate existing WCF solution to gRPC with minimum effort.

## Links

- getting started tutorial [create a gRPC client and server](CreateClientAndServerASPNETCore.md)
- [migrate from WCF to a gRPC tutorial](MigrateWCFServiceTogRPC.md)
- [service and operation names](ServiceAndOperationName.md)
- [service and operation bindings](ServiceAndOperationBinding.md)
- [client configuration](ClientConfiguration.md)
- [ASP.NET Core server configuration](ASPNETCoreServerConfiguration.md)
- [Grpc.Core server configuration](GrpcCoreServerConfiguration.md)
- [compatibility with native gRPC](CompatibilityWithNativegRPC.md)
- example [protobuf marshaller](/https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ProtobufMarshaller)

### standard gRPC approach

``` proto
message SumRequest {
	int64 x = 1;
	int32 y = 2;
	int32 z = 3;
}

message SumResponse {
	int64 result = 1;
}

service Calculator {
    rpc Sum (SumRequest) returns (SumResponse);
```

``` c#
public SumResponse Sum(SumRequest request)
{
    return new SumResponse { Result = request.X + request.Y + request.Z };
}
```

### ServiceModel.Grpc code-first approach

``` c#
[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    long Sum(long x, int y, int z);
}

public sealed class Calculator : ICalculator
{
    public long Sum(long x, int y, int z)
    {
        return x + y + z;
    }
}
```

## Data contracts

By default the DataContractSerializer is used for marshalling data between server an client. This behavior is configurable, see [ProtobufMarshaller example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ProtobufMarshaller).

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
