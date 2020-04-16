# ServiceModel.Grpc compatibility with native gRPC

To make ServiceModel.Grpc compatible with native gRPC:
- configure to use protobuf serialization for data marshalling
- service and method names must match
- serialization contracts of .net objects must be compatible with .proto contract

## Demo app

Small app to demonstrate [compatibility with Native gRPC](/Examples/CompatibilityWithNativegRPC).
The application is configured to use [protobuf-net](https://www.nuget.org/packages/protobuf-net/) for data serialization.

## Service and method names

.proto Person

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
    // method name Sum
    rpc Sum (SumRequest) returns (SumResponse);
}
```

ServiceModel.Grpc contract

```C#
[ServiceContract(Name = "Calculator")] // service name: Calculator
public interface ICalculator
{
    [OperationContract(Name = "Sum")] // method name: Sum
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

```C#
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

```C#
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