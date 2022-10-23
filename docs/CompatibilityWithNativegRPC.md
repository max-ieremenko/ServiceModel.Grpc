# ServiceModel.Grpc compatibility with native gRPC

To make ServiceModel.Grpc calls compatible with native gRPC:

- configure to use protobuf serialization for data marshalling
- service and method names must match
- .net object serialization contracts must be compatible with the .proto contract.

## Demo app

Small app to demonstrate [compatibility with Native gRPC](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/CompatibilityWithNativegRPC).
The application is configured to use [protobuf-net](https://www.nuget.org/packages/protobuf-net/) for data serialization.

## Service and method names

.proto Calculator

``` proto
message SumRequest {
    int32 x = 1;
    int32 y = 2;
}

message SumResponse {
    int64 result = 1;
}

// service name: Calculator
service Calculator {
    // operation name Sum
    rpc Sum (SumRequest) returns (SumResponse);
}
```

ServiceModel.Grpc contract

``` c#
// service name: Calculator
[ServiceContract(Name = "Calculator")]
public interface ICalculator
{
    // operation name: Sum
    // parameter names don't matter, order and types matter
    [OperationContract(Name = "Sum")]
    Task<long> SumAsync(int x, int y);
}
```

For additional information refer to [service and operation names](ServiceAndOperationName.md) and [service and operation bindings](ServiceAndOperationBinding.md).

## Serialization

.proto Person

``` proto
message Person {
    string firstName = 1;
    string secondName = 2;
    int32 Age = 3;
}
```

c# Person decorated with data contract

``` c#
[DataContract]
public class Person
{
    [DataMember(Order = 1)]
    public string FirstName { get; set; }

    [DataMember(Order = 2)]
    public string SecondName { get; set; }

    [DataMember(Order = 3)]
    public int Age { get; set; }
}
```

c# Person decorated with proto contract

``` c#
[ProtoContract]
public class Person
{
    [ProtoMember(1)]
    public string FirstName { get; set; }

    [ProtoMember(2)]
    public string SecondName { get; set; }

    [ProtoMember(3)]
    public int Age { get; set; }
}
```

For additional information refer to [protobuf-net](https://github.com/protobuf-net/protobuf-net).