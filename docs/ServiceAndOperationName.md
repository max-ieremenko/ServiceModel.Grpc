# ServiceModel.Grpc service and operation names

By default, a contract interface name is a gRPC service name and a contract method name is gRPC operation name, namespace and assembly name of the interface does not matter:

``` c#
// service name: ICalculator
[ServiceContract]
public interface ICalculator
{
    // operation: POST /ICalculator/Sum
    [OperationContract]
    int Sum(int x, int y);

    // operation: POST /ICalculator/SumAsync
    [OperationContract]
    Task<int> SumAsync(int x, int y);
}
```

in some cases, it may lead to naming conflicts. In the following example there are 2 methods with gRPC method name `/ICalculator/Sum`:

``` c#
// service name: ICalculator
[ServiceContract]
public interface ICalculator
{
    // operation: POST /ICalculator/Sum
    [OperationContract]
    int Sum(int x, int y);

    // operation: POST /ICalculator/Sum
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
    // operation: POST /ICalculator/Sum2Values
    [OperationContract(Name = "Sum2Values")]
    int Sum(int x, int y);

    // operation: POST /ICalculator/Sum3Values
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

The name of the generic argument is 'DataContractAttribute.Name', if defined, or type name:

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

## ServiceContractAttribute and OperationContractAttribute

To use `ServiceContract` and `OperationContract` attributes, a project has to reference

- `System.ServiceModel.dll` assembly in .NET Framework
- [System.ServiceModel.Primitives](https://www.nuget.org/packages/System.ServiceModel.Primitives) NuGet package in .NET 6+

``` xml
<ItemGroup Condition="'$(TargetFramework)' == 'net462'">
    <Reference Include="System.ServiceModel" />
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' != 'net462'">
    <PackageReference Include="System.ServiceModel.Primitives" />
</ItemGroup>
```

If for some reason, having extra references is not an option, the attributes can be defined in your code:

``` cs
namespace System.ServiceModel;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ServiceContractAttribute : Attribute
{
    public string? Name { get; set; }

    public string? Namespace { get; set; }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class OperationContractAttribute : Attribute
{
    public string? Name { get; set; }
}
```

ServiceModel.Grpc recognizes them by the namespace `System.ServiceModel` and the names `ServiceContractAttribute` and `OperationContractAttribute`.