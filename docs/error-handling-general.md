# ServiceModel.Grpc error handling general information

ServiceModel.Grpc does not handle any errors by default, exception are handled by gRPC API (`RpcException`).

How to activate and configure custom error handling is described in this [tutorial](global-error-handling.md). This page explains how ServiceModel.Grpc error handling works under the hood. The implementation and technical details are the subject to be changed in the future.

## Briefly

Custom error handling should be configured on client and server. When it is done, all calls on client and server are intercepted by specific `Grpc.Core.Interceptors.Interceptor` implementation.

Server interceptor on any exception asks for `IServerErrorHandler.ProvideFaultOrIgnore`, if `ServerFaultDetail` is provided, the interceptor throws a `RpcException` with extra metadata. The metadata is transferring to the client.

Client interceptor catches only `RpcException`, when it is the case `IClientErrorHandler.ThrowOrIgnore` is responsible what to do with the exception. All metadata from server interceptor is available in ClientFaultDetail.

So, exception details are transferred as metadata via gRPC API (`RpcException`). This means the metadata is a set of http headers.

## Server-side error model

`IServerErrorHandler.ProvideFaultOrIgnore` can provide custom error model to transfer it to a client call.

``` c#
public struct ServerFaultDetail
{
    // optional StatusCode => RpcException.Status.StatusCode
    public StatusCode? StatusCode { get; set; }

    // optional Message => RpcException.Status.Detail
    public string Message { get; set; }

    // optional user defined error detail
    public object Detail { get; set; }

    // optional Trailers => RpcException.Trailers
    public Metadata Trailers { get; set; }
}
```

The ServerFaultDetail.Detail can be an instance of any user-defined class. When it is provided, the ServiceModel.Grpc:

- serializes it by the `IMarshallerFactory` associated with the current service to a `byte[]` and passes to a client call via binary header
- passes the type (`Detail.GetType()`) of instance to a client call via string header

### Example

``` c#
// user defined detail
[DataContract]
public class CustomDetail
{
    [DataMember]
    public string SomeProperty { get; set; }

    [DataMember]
    public string AnotherProperty { get; set; }
}

// user defined IServerErrorHandler.ProvideFaultOrIgnore
public ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
{
    return new ServerFaultDetail
    {
        Detail = new CustomDetail
        {
            SomeProperty = ...
            AnotherProperty = ...
        }
    };
}
```

## Client-side error model

`IClientErrorHandler.ThrowOrIgnore` must decide what to throw to a user code.

``` c#
public readonly struct ClientFaultDetail
{
    // original RpcException
    public RpcException OriginalError { get; }

    // detail provided by IServerErrorHandler.ProvideFaultOrIgnore on server
    public object Detail { get; }
}
```

When ServerFaultDetail.Detail is provided by server, the ServiceModel.Grpc:

- restores the detail type from string header
- de-serializes detail by the `IMarshallerFactory` associated with the current service from binary header

### Example

``` c#
// user defined IClientErrorHandler.ThrowOrIgnore
public void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail)
{
    var customDetail = (CustomDetail)detail.Detail;
    // ...
}
```

## Operation cancelled

There are few corner cases with custom exception handling when a call is cancelled.

The following code asks gRPC API to cancel call after 1 second.

``` c#
[ServiceContract]
public interface ISomeService
{
    [OperationContract]
    Task DoSomething(CancellationToken token);
}

// client call
var tokenSource = new CancellationTokenSource();
var client = IClientFactory.CreateClient<ISomeService>(...);

tokenSource.CancelAfter(TimeSpan.FromSeconds(1));
await client.DoSomething(tokenSource.Token);
```

What happens on client after 1 second if call is not finished?

On client the real gRPC call will be interrupted by gRPC API, so no metadata is available from server.

What happens on server after 1 second if call is not finished?

User defined server method will be finished according to the implementation, no metadata is provided to a client call by gRPC API.

ServiceModel.Grpc.Interceptors `ServerErrorHandlerBase` and `ClientErrorHandlerBase` handle this case properly. During the custom error handling configuration on client and server make sure that the root handlers are inherited from `ServerErrorHandlerBase` and `ClientErrorHandlerBase`.

``` c#
// client call
var tokenSource = new CancellationTokenSource();
var client = IClientFactory.CreateClient<ISomeService>(...);

tokenSource.CancelAfter(TimeSpan.FromSeconds(1));

try
{
    await client.DoSomething(tokenSource.Token);
}
catch (OperationCanceledException)
{
    ....
}
```
