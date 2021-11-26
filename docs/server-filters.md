# ServiceModel.Grpc server filters

Server filter is a hook for service method invocation, it can work together with gRPC server interceptors, but it is not interceptor.

see [example](Examples/https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ServerFilters)

``` c#
public sealed class MyServerFilter : IServerFilter
{
    public ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        // take control before all others filters in the stack and the service method
        try
        {
            // invoke all other filters in the stack and the service method
            await next().ConfigureAwait(false);

            // take control after all others filters in the stack and the service method
        }
        catch
        {
            // handle the exception
            throw;
        }
        finally
        {
        }
    }
}
```

## The server filters can work together with gRPC server interceptors.

For example, at runtime there are gRPC native `interceptor1`, gRPC native `interceptor2`, server `filter1` and server `filter2`.
Together with gRPC server interceptors the execution stack for `service method` looks like this:

`gRPC interceptor1` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;`gRPC interceptor2` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`server filter1` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`server filter2` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`service method`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`server filter2` takes control after\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`server filter1` takes control after\
&nbsp;&nbsp;&nbsp;&nbsp;`gRPC interceptor2` takes control after\
`gRPC interceptor1` takes control after

## Server filters registration

The registration has

- `order`: the integer number, determines the order of execution of filters.
- `factory`: the filter instance factory `Func<IServiceProvider, IServerFilter>`.

A filter can be attached

- via configuration globally to all services
- via configuration to a specific service
- via attribute for a service
- via attribute for a method

### in asp.net core server (ServiceModel.Grpc.AspNetCore)

``` c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // decide the filter lifetime
    services.AddSingleton<MySingletonFilter>();

    // attach the filter globally to all methods from all services
    services.AddServiceModelGrpc(options =>
    {
        options.Filters.Add(1, provider => provider.GetRequiredService<MySingletonFilter>());
        
        options.Filters.Add(2, _ => new MyTransientFilter());
    });

    // or attach the filter to all methods from MyService
    services.AddServiceModelGrpcServiceOptions<MyService>(options =>
    {
        options.Filters.Add(1, provider => provider.GetRequiredService<MySingletonFilter>());

        options.Filters.Add(2, _ => new MyTransientFilter());
    });
}
```

### in Grpc.Core.Server (ServiceModel.Grpc.SelfHost)

``` c#
Grpc.Core.Server server = ...;

services.AddTransient<MyService> = ...;
services.AddSingleton<MySingletonFilter>();

IServiceProvider serviceProvider = ...;

// attach the filter to all methods from MyService
server.Services.AddServiceModel<MyService>(
    serviceProvider,
    options =>
    {
        options.Filters.Add(1, provider => provider.GetRequiredService<MySingletonFilter>());

        options.Filters.Add(2, _ => new MyTransientFilter());
    });
```

### in the source code via `ServerFilterAttribute`

``` c#
public sealed class MyServerFilterAttribute : ServerFilterAttribute
{
    public MyServerFilterAttribute(int order)
        : base(order)
    {
    }

    public override ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        // filter logic here
        // ...
        return next();
    }
}

// attach the filter to all methods
[MyServerFilter(1)]
class MyService : IMyService
{
    // attach the filter to the method
    [MyServerFilter(1)]
    public Task MyMethod(...) { }
}
```

### in the source code via `ServerFilterRegistrationAttribute`

``` c#
public sealed class MyServerFilterRegistrationAttribute : ServerFilterRegistrationAttribute
{
    public MyServerFilterRegistration(int order)
        : base(order)
    {
    }

    public override IServerFilter CreateFilter(IServiceProvider serviceProvider)
    {
        return new MyServerFilter();

        // or
        return serviceProvider.GetRequiredService<MyServerFilter>();
    }
}

// attach the filter to all methods
[MyServerFilterRegistration(1)]
class MyService : IMyService
{
    // attach the filter to the method
    [MyServerFilterRegistration(1)]
    public Task MyMethod(...) { }
}
```

## Server filters context

The context is represented by interface `IServerFilterContext`:

``` c#
ValueTask IServerFilter.InvokeAsync(IServerFilterContext context, Func<ValueTask> next)

public interface IServerFilterContext
{
    // Gets the service instance.
    object ServiceInstance { get; }

    // Gets gRPC ServerCallContext
    ServerCallContext ServerCallContext { get; }

    // Gets your service provider.
    IServiceProvider ServiceProvider { get; }

    // Gets a dictionary that can be used by the various interceptors and handlers of this call to store arbitrary state.
    // The reference to ServerCallContext.UserState.
    IDictionary<object, object?> UserState { get; } // return ServerCallContext.UserState

    // Gets the the contract method declaration.
    MethodInfo ContractMethodInfo { get; }

    // Gets the the service method declaration.
    MethodInfo ServiceMethodInfo { get; }

    // Gets the control of the incoming request.
    IRequestContext Request { get; }

    // Gets the control of the outgoing response.
    IResponseContext Response { get; }
}
```

### Unary call context example

For more details see [unary operation](operation-unary.md)

``` c#
[ServiceContract]
interface IMyService
{
    [OperationContract]
    Task<string> Call(int arg1, int arg2, int arg3, CancellationToken token);
}

class MyService
{
    Task<string> Call(int renamedArg1, int renamedArg2, int renamedArg3, CancellationToken token) { ... }
}
```

- context.ServiceInstance: MyService instance
- context.ContractMethodInfo: IMyService.Call
- context.ServiceMethodInfo: MyService.Call
- context.Request.Count: 3 (arg1, arg2, arg3), where arg1 is `(int)context.Request["arg1"]` or `(int)context.Request[0]`
- context.Request.Stream: `null`, inapplicable
- context.Response.Count: 1 (result), where result is `(string)context.Response["result"]` or `(string)context.Response[0]`
- context.Response.Stream: `null`, inapplicable

### Client streaming call context example

For more details see [client streaming operation](operation-client-streaming.md)

``` c#
[ServiceContract]
interface IMyService
{
    [OperationContract]
    Task<string> Call(IAsyncEnumerable<int> stream, int arg1, int arg2, CancellationToken token);
}

class MyService
{
    Task<string> Call(IAsyncEnumerable<int> stream, int renamedArg1, int renamedArg2, CancellationToken token) { ... }
}
```

- context.ServiceInstance: MyService instance
- context.ContractMethodInfo: IMyService.Call
- context.ServiceMethodInfo: MyService.Call
- context.Request.Count: 2 (arg1, arg2), where arg1 is `(int)context.Request["arg1"]` or `(int)context.Request[0]`
- context.Request.Stream: IAsyncEnumerable\<int\> (input stream)
- context.Response.Count: 1 (result), where result is `(string)context.Response["result"]` or `(string)context.Response[0]`
- context.Response.Stream: `null`, inapplicable

### Server streaming call context example

For more details see [server streaming operation](operation-server-streaming.md)

``` c#
[ServiceContract]
interface IMyService
{
    [OperationContract]
    ValueTask<(IAsyncEnumerable<string> Stream, string Metadata1, int Metadata2)> Call(int arg1, int arg2, CancellationToken token);
}

class MyService
{
    ValueTask<(IAsyncEnumerable<string>, string, int)> Call(int renamedArg1, int renamedArg2, CancellationToken token) { ... }
}
```

- context.ServiceInstance: MyService instance
- context.ContractMethodInfo: IMyService.Call
- context.ServiceMethodInfo: MyService.Call
- context.Request.Count: 2 (arg1, arg2), where arg1 is `(int)context.Request["arg1"]` or `(int)context.Request[0]`
- context.Request.Stream: `null`, inapplicable
- context.Response.Count: 2 (Metadata1, Metadata2), where Metadata1 is `(string)context.Response["Metadata1"]` or `(string)context.Response[0]`
- context.Response.Stream: IAsyncEnumerable\<string\> (output stream)

### Duplex streaming call context example

For more details see [duplex streaming operation](operation-duplex-streaming.md)

``` c#
[ServiceContract]
interface IMyService
{
    [OperationContract]
    ValueTask<(IAsyncEnumerable<string> Stream, string Metadata1, int Metadata2)> Call(IAsyncEnumerable<int> stream, int arg1, int arg2, CancellationToken token);
}

class MyService
{
    ValueTask<(IAsyncEnumerable<string>, string, int)> Call(IAsyncEnumerable<int> stream, int renamedArg1, int renamedArg2, CancellationToken token) { ... }
}
```

- context.ServiceInstance: MyService instance
- context.ContractMethodInfo: IMyService.Call
- context.ServiceMethodInfo: MyService.Call
- context.Request.Count: 2 (arg1, arg2), where arg1 is `(int)context.Request["arg1"]` or `(int)context.Request[0]`
- context.Request.Stream: IAsyncEnumerable\<int\> (input stream)
- context.Response.Count: 2 (Metadata1, Metadata2), where Metadata1 is `(string)context.Response["Metadata1"]` or `(string)context.Response[0]`
- context.Response.Stream: IAsyncEnumerable\<string\> (output stream)

## The filter has complete control over the method call

In the following example, the filter replaces the service method `SumAsync(1, 2) => 3`.

``` c#
[SumAsyncServerFilter]
public ValueTask<int> SumAsync(int x, int y)
{
    // the filter must handle the call
    throw new NotImplementedException();
}

internal sealed class SumAsyncServerFilterAttribute : ServerFilterAttribute
{
    public override ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        var x = (int)context.Request["x"];
        var y = (int)context.Request["y"];

        // do not invoke the real SumAsync
        // await next().ConfigureAwait(false);

        context.Response["result"] = x + y;
        
        return new ValueTask(Task.CompletedTask);
    }
}
```

In the following "HappyDebugging" example, the filter hacks the service method.\
Client call: `MultiplyBy(values: { 1, 2 }, multiplier: 3)`.\
Response for client: `values: { 11, 16 }, multiplier: 5`

- value 1: `(1 + 1) * (3 + 2) + 1` = 11
- value 2: `(2 + 1) * (3 + 2) + 1` = 16

``` c#
sealed class Calculator
{
    [HappyDebuggingServerFilter]
    public ValueTask<(IAsyncEnumerable<int> Values, int Multiplier)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token)
    {
        var output = DoMultiplyBy(values, multiplier, token);
        return new ValueTask<(IAsyncEnumerable<int>, int)>((output, multiplier));
    }

    private async IAsyncEnumerable<int> DoMultiplyBy(IAsyncEnumerable<int> values, int multiplier, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var value in values.WithCancellation(token).ConfigureAwait(false))
        {
            yield return value * multiplier;
        }
    }
}

sealed class HappyDebuggingServerFilterAttribute : ServerFilterAttribute
{
    public override async ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        var inputMultiplier = (int)context.Request["multiplier"];
        var inputValues = (IAsyncEnumerable<int>)context.Request.Stream;

        // increase multiplier by 2
        context.Request["multiplier"] = inputMultiplier + 2;

        // increase each input value by 1
        context.Request.Stream = IncreaseValuesBy1(inputValues, context.ServerCallContext.CancellationToken);

        // call Calculator.MultiplyBy
        await next().ConfigureAwait(false);

        var outputValues = (IAsyncEnumerable<int>)context.Response.Stream;

        // increase output value by 1
        context.Response.Stream = IncreaseValuesBy1(outputValues, context.ServerCallContext.CancellationToken);
    }

    private async IAsyncEnumerable<int> IncreaseValuesBy1(IAsyncEnumerable<int> values, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var value in values.WithCancellation(token).ConfigureAwait(false))
        {
            yield return value + 1;
        }
    }
}
```