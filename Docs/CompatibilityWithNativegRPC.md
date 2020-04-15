# ServiceModel.Grpc compatibility with native gRPC

To make ServiceModel.Grpc compatible with native gRPC:
- configure to use protobuf serialization for data marshalling
- serialization contracts of .net objects must be compatible with .proto contract

## Demo app

Small app to demonstrate [compatibility withNative gRPC](/Examples/CompatibilityWithNativegRPC).
The application is configured to use [protobuf-net](https://www.nuget.org/packages/protobuf-net/) for data serialization.

## serialization contracts

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

more details can be found at [protobuf-net](https://github.com/protobuf-net/protobuf-net).