# ServiceModel.Grpc service and operation names

By default a contract interface name is a gRPC service name and contract method name is gRPC method name, namespace and assembly name of interface does not not matter:

``` c#
// service name: ICalculator
[ServiceContract]
public interface ICalculator
{
    // method: POST /ICalculator/Sum
    [OperationContract]
    int Sum(int x, int y);

    // method: POST /ICalculator/SumAsync
    [OperationContract]
    Task<int> SumAsync(int x, int y);
}
```

in some cases it may lead to a naming conflicts, in the following example there are 2 methods with gRPC method name `/ICalculator/Sum`:

``` c#
// service name: ICalculator
[ServiceContract]
public interface ICalculator
{
    // method: POST /ICalculator/Sum
    [OperationContract]
    int Sum(int x, int y);

    // method: POST /ICalculator/Sum
    [OperationContract]
    int Sum(int x, int y, int z);
}
```

To resolve the conflict use `OperationContractAttribute.Name`:

``` c#
// service name: ICalculator
[ServiceContract]
public interface ICalculator
{
    // method: POST /ICalculator/Sum2Values
    [OperationContract(Name = "Sum2Values")]
    int Sum(int x, int y);

    // method: POST /ICalculator/Sum3Values
    [OperationContract(Name = "Sum3Values")]
    int Sum(int x, int y, int z);
}
```

in the following example there is a conflict of service names `ICalculator`:

``` c#
namespace Standard
{
    // service name: ICalculator
    [ServiceContract]
    public interface ICalculator { }

}

namespace Scientific
{
    // service name: ICalculator
    [ServiceContract]
    public interface ICalculator { }

}
```

To resolve the conflict use `ServiceContractAttribute.Name` and/or `ServiceContractAttribute.Namespace`:

``` c#
namespace Standard
{
    // options 1: service name: Standard.Calculator
    [ServiceContract(Name = "Standard.Calculator")]

    // options 2: service name: Standard.Calculator
    [ServiceContract(Name = "Calculator", Namespace = "Standard")]

    // options 3: service name: Standard.ICalculator
    [ServiceContract(Namespace = "Standard")]
    public interface ICalculator { }
}
```

## Generic interface as a service contract

If a service contract interface is generic, generic arguments become a part of a gRPC service name.
The final name is a concatenation of the service name and generic argument type names, separated by `-` symbol:

``` c#
[ServiceContract]
public interface ICalculator<TValue>
{
    [OperationContract]
    Task<TValue> Sum(TValue x, TValue y);
}

internal sealed class CalculatorDouble : ICalculator<double>
{
    // POST: /ICalculator-Double/Sum
    public Task<double> Sum(double x, double y) => x + y;
}

internal sealed class CalculatorNullableInt32 : ICalculator<int?>
{
    // POST: /ICalculator-Nullable-Int32/Sum
    public Task<int?> Sum(int? x, int? y) => x + y;
}
```

`ICalculator<double>` interface has gRPC service name `ICalculator-Double`.

`ICalculator<int?>` interface has gRPC service name `ICalculator-Nullable-Int32`.

The name of generic argument is 'DataContractAttribute.Name', if is defined, or type name:

``` c#
[DataContract(Name = "MyValue")]
public class Value
{
}

internal sealed class CalculatorValue : ICalculator<Value>
{
    // POST: /ICalculator-MyValue/Sum
    public Task<Value> Sum(Value x, Value y) => x + y;
}
```