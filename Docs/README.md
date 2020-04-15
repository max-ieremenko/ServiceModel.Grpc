# ServiceModel.Grpc

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
    Task<Person> CreatePerson(string name, DateTime birthDay);

    // method is not gRPC call
    Task<Person> ChangeName(Person person, string newName);
}
```

## gRPC calls

Depends on call type (Unary, ClientStreaming, ServerStreaming, DuplexStreaming),
as input can be zero or more data parameters and optional context parameters. Response is optional.

Context parameters are:
- ServiceModel.Grpc.CallContext
- Grpc.Core.CallOptions
- Grpc.Core.ServerCallContext
- System.Threading.CancellationToken

## Unary call examples

``` c#
[OperationContract]
void Ping();

[OperationContract]
Task PingAsync();

[OperationContract]
string StringConcat(string value1, int value2, ..., CallContext context = default);

[OperationContract]
Task<string> StringConcat(string value1, int value2, ..., ServerCallContext context = default);
```

## ClientStreaming call example

``` c#
[OperationContract]
Task<long> SumValues(IAsyncEnumerable<int> values, CancellationToken token = default);
```

## ServerStreaming call  example

``` c#
[OperationContract]
IAsyncEnumerable<int> EnumerableRange(int start, int count, CancellationToken token = default);
```

## DuplexStreaming example

``` c#
[OperationContract]
IAsyncEnumerable<long> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token = default);
```

## Limitations

- generic methods are not supported
- ref and out parameters are not supported
- ClientStreaming and DuplexStreaming do not support input data parameters
