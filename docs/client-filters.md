# ServiceModel.Grpc client filters

The client filter is a hook for contract method invocation, it can work together with gRPC client interceptors, but it is not an interceptor.

see [example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ClientFilters)

``` c#
public sealed class MyClientFilter : IClientFilter
{
    public void Invoke(IClientFilterContext context, Action next)
    {
        // take control before all others filters in the stack and the blocking service call
        try
        {
            // invoke all other filters in the stack and call the service
            next();

            // take control after all others filters in the stack and the service call
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

    public ValueTask InvokeAsync(IClientFilterContext context, Func<ValueTask> next)
    {
        // take control before all others filters in the stack and the async service call
        try
        {
            // invoke all other filters in the stack and call the service
            await next().ConfigureAwait(false);

            // take control after all others filters in the stack and the service call
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

## The client filters can work together with gRPC client interceptors.

For example, at runtime there are gRPC native `interceptor1`, gRPC native `interceptor2`, client `filter1` and client `filter2`.
Together with gRPC client interceptors the execution stack for `service call` looks like this:

`client filter1` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;`client filter2` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`gRPC interceptor1` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`gRPC interceptor2` takes control and calls `next`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`service call`\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`gRPC interceptor2` takes control after\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`gRPC interceptor1` takes control after\
&nbsp;&nbsp;&nbsp;&nbsp;`client filter2` takes control after\
`client filter1` takes control after


## Client filters registration

The registration has

- `order`: the integer number, determines the order of execution of filters.
- `factory`: the filter instance factory `Func<IServiceProvider, IClientFilter>`.

A filter can be attached

- via configuration globally to all clients
- via configuration to a specific client

``` c#
var services = new ServiceCollection();

// decide the filter lifetime
services.AddSingleton<MySingletonFilter>();

IClientFactory clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
{
    ServiceProvider = services.BuildServiceProvider(),
    Filters =
    {
        // attach the filter globally to all calls from all clients
        { 1, provider => provider.GetRequiredService<MySingletonFilter>() }
    }
});

clientFactory.AddClient<IMyContract>(options =>
{
    // attach the filter to all calls from IMyContract client
    options.Filters.Add(2, _ => new MyTransientFilter())
})
```

## Client filters context

The context is represented by interface `IClientFilterContext`:

``` c#
void IClientFilter.Invoke(IClientFilterContext context, Action next);

ValueTask IClientFilter.InvokeAsync(IClientFilterContext context, Func<ValueTask> next);

public interface IClientFilterContext
{
    // Gets the gRPC method metadata.
    IMethod Method { get; }

    // Gets gRPC CallOptions
    CallOptions CallOptions { get; }

    // Gets your service provider.
    IServiceProvider ServiceProvider { get; }

    // Gets a dictionary that can be used by the various filters of this call to store arbitrary state.
    IDictionary<object, object?> UserState { get; }

    // Gets the the contract method declaration.
    MethodInfo ContractMethodInfo { get; }

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
```

- context.ContractMethodInfo: IMyService.Call
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
```

- context.ContractMethodInfo: IMyService.Call
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
```

- context.ContractMethodInfo: IMyService.Call
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
```

- context.ContractMethodInfo: IMyService.Call
- context.Request.Count: 2 (arg1, arg2), where arg1 is `(int)context.Request["arg1"]` or `(int)context.Request[0]`
- context.Request.Stream: IAsyncEnumerable\<int\> (input stream)
- context.Response.Count: 2 (Metadata1, Metadata2), where Metadata1 is `(string)context.Response["Metadata1"]` or `(string)context.Response[0]`
- context.Response.Stream: IAsyncEnumerable\<string\> (output stream)

## The filter has complete control over the service call

In the following example, the filter replaces the service call `SumAsync(1, 2) => 3`.

``` c#
[ServiceContract]
interface IMyService
{
    [OperationContract]
    ValueTask<int> SumAsync(int x, int y);
}

internal sealed class SumAsyncClientFilter : IClientFilter
{
    // SumAsync is async unary call
    public void Invoke(IClientFilterContext context, Action next) => next();

    public ValueTask InvokeAsync(IClientFilterContext context, Func<ValueTask> next)
    {
        // ignore all calls, except SumAsync
        if (context.ContractMethodInfo.Name != nameof(IMyService.SumAsync))
        {
            return next();
        }

        var x = (int)context.Request["x"];
        var y = (int)context.Request["y"];

        // do not invoke the real call to SumAsync
        // await next().ConfigureAwait(false);

        context.Response["result"] = x + y;
        
        return ValueTask.CompletedTask;
    }
}
```

## Blocking call and Async call

Depending on the contract method definition, ServiceModel.Grpc infrastructure invokes `IClientFilter.Invoke` or `IClientFilter.InvokeAsync`. In general, if a method\`s return type is Task or ValueTask then `InvokeAsync` otherwise `Invoke`.

``` c#
[ServiceContract]
interface IMyService
{
    // IClientFilter.Invoke
    [OperationContract]
    [void|TResult] BlockingCall();

    // IClientFilter.InvokeAsync
    [OperationContract]
    [Task|ValueTask|Task<TResult>|ValueTask<TResult>] AsyncCall();

    // IClientFilter.InvokeAsync
    [OperationContract]
    [Task|ValueTask|Task<TResult>|ValueTask<TResult>] ClientStreamingCall(IAsyncEnumerable<TData> stream);

    // IClientFilter.Invoke
    [OperationContract]
    IAsyncEnumerable<TData> ServerStreamingCall();

    // IClientFilter.InvokeAsync
    [OperationContract]
    [Task<IAsyncEnumerable<TData>>|ValueTask<IAsyncEnumerable<TData>>] ServerStreamingCall();
    
    // IClientFilter.Invoke
    [OperationContract]
    IAsyncEnumerable<TData> DuplexStreamingCall(IAsyncEnumerable<TData> stream);

    // IClientFilter.InvokeAsync
    [OperationContract]
    [Task<IAsyncEnumerable<TData>>|ValueTask<IAsyncEnumerable<TData>>] ServerStreamingCall(IAsyncEnumerable<TData> stream);
}
```