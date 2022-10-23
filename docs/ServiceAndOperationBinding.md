# ServiceModel.Grpc service and operation bindings

Interface methods are a list of gRPC operations.

``` c#
[ServiceContract]
public interface IStandardCalculator
{
    [OperationContract]
    int Sum(int x, int y);
}

[ServiceContract]
public interface IScientificCalculator
{
    [OperationContract]
    int Multiply(int x, int y);
}
```

## hosting on the server-side

In the following example on a server start-up there will be 2 gRPC services available `IStandardCalculator` and `IScientificCalculator`.
`IDisposable` is ignored as it is not a service contract.

``` c#
internal sealed class CalculatorService : IStandardCalculator, IScientificCalculator, IDisposable
{
    // POST /IStandardCalculator/Sum
    public int Sum(int x, int y) { /*...*/ }

    // POST /IScientificCalculator/Multiply
    public int Multiply(int x, int y) { /*...*/ }
}

internal sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // enable ServiceModel.Grpc
        services.AddServiceModelGrpc();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            // bind CalculatorService
            endpoints.MapGrpcService<CalculatorService>();
        });
    }
}
```

## call from the client-side

``` c#
static readonly IClientFactory MyDefaultClientFactory = new ClientFactory(...);

static void Call()
{
    ChannelBase channel = ... // initialize channel

    // POST /IStandardCalculator/Sum
    MyDefaultClientFactory.CreateClient<IStandardCalculator>(channel).Sum(...);

    // POST /IScientificCalculator/Multiply
    MyDefaultClientFactory.CreateClient<IScientificCalculator>(channel).Multiply(...);
}
```

## Contract inheritance

Inheritance of interfaces, defined as `ServiceContract`, does not affect operation bindings.

``` c#
[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    int Sum(int x, int y);
}

[ServiceContract]
public interface IScientificCalculator : ICalculator
{
    [OperationContract]
    int Multiply(int x, int y);
}

internal sealed class ScientificCalculator : IScientificCalculator
{
    // POST /ICalculator/Sum
    public int Sum(int x, int y) { /*...*/ }

    // POST /IScientificCalculator/Multiply
    public int Multiply(int x, int y) { /*...*/ }
}
```

## Interface inheritance

If an interface is not marked as `ServiceContract`, the service name for each defined operation comes from the top `ServiceContract` interface.

``` c#
public interface ICalculator
{
    [OperationContract]
    int Sum(int x, int y);
}

[ServiceContract]
public interface IScientificCalculator : ICalculator
{
    [OperationContract]
    int Multiply(int x, int y);
}

internal sealed class ScientificCalculator : IScientificCalculator
{
    // POST /IScientificCalculator/Sum
    public int Sum(int x, int y) { /*...*/ }

    // POST /IScientificCalculator/Multiply
    public int Multiply(int x, int y) { /*...*/ }
}
```

[View InterfaceInheritance](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/InterfaceInheritance) example.

