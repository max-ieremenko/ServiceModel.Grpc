# ServiceModel.Grpc service and operation names

By default a contract interface name is a gRPC service name and contract method name is gRPC method name, namespace and assembly name of interface do not matter:

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

in some cases it may lead to a naming conflicts, in the following example there are 2 method with gRPC method name `/ICalculator/Sum`:

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
