# ServiceModel.Grpc global error handling in gRPC

ServiceModel.Grpc does not handle any errors by default.
The general information about error handling is [here](error-handling-general.md).

This page shows how to implement custom gRPC exception handling with the following behavior:

- if the server throws an ApplicationException, on the client I want to catch ApplicationException
- UnexpectedErrorException is defined in the client. If the server throws InvalidOperationException or NotSupportedException I want to catch UnexpectedErrorException and see detailed information from the server, such as full exception `ex.ToString()`, exception type, gRPC method name, etc.
- all other exceptions must be handled by gRPC API.

[View sample code](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ErrorHandling).

## Create a contract

The service contract:

``` c#
[ServiceContract]
public interface IDebugService
{
    // always throws ApplicationException
    [OperationContract]
    Task ThrowApplicationException(string message);

    // randomly throws InvalidOperationException or NotSupportedException
    [OperationContract]
    Task ThrowRandomException(string message);
}
```

UnexpectedErrorDetail is a data contract that carries detailed information about InvalidOperationException or NotSupportedException from server to client.

``` c#
[DataContract]
public class UnexpectedErrorDetail
{
    [DataMember]
    public string Message { get; set; }

    [DataMember]
    public string MethodName { get; set; }

    [DataMember]
    public string ExceptionType { get; set; }

    [DataMember]
    public string FullException { get; set; }
}
```

## Raise errors in gRPC server

The service implementation is simple

``` c#
public sealed class DebugService : IDebugService
{
    public Task ThrowApplicationException(string message)
    {
        throw new ApplicationException(message);
    }

    public Task ThrowRandomException(string message)
    {
        var randomValue = new Random(DateTime.Now.Millisecond).Next(0, 2);
        if (randomValue == 0)
        {
            throw new InvalidOperationException(message);
        }

        throw new NotSupportedException(message);
    }
}
```

## Catch errors in gRPC clients

``` c#
// IClientFactory DefaultClientFactory
var client = DefaultClientFactory.CreateClient<IDebugService>(...);

// catch ApplicationException
try
{
    await client.ThrowApplicationException("  application error occur");
}
catch (ApplicationException ex)
{
    Console.WriteLine(ex.Message);
}

// catch UnexpectedErrorException which is defined on client
try
{
    await client.ThrowRandomException("random error occur");
}
catch (UnexpectedErrorException ex)
{
    Console.WriteLine("  Message: {0}", ex.Detail.Message);
    Console.WriteLine("  ExceptionType: {0}", ex.Detail.ExceptionType);
    Console.WriteLine("  MethodName: {0}", ex.Detail.MethodName);
    Console.WriteLine("  FullException: {0} ...", new StringReader(ex.Detail.FullException).ReadLine());
}
```

## Create server error handlers

A server-side error handler is an implementation of interface `IServerErrorHandler` with one method `ProvideFaultOrIgnore`.
If the method returns `null`, the exception is processed by gRPC API, otherwise ServiceModel.Grpc throws a `RpcException` based on `ServerFaultDetail`.

To meet the requirements we implement two error handlers. The first one `ApplicationExceptionServerHandler` is responsible to process `ApplicationException`.

``` c#
public sealed class ApplicationExceptionServerHandler : IServerErrorHandler
{
    public ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
    {
        if (error is ApplicationException)
        {
            // provide a marker for the client exception handler
            return new ServerFaultDetail { Detail = "ApplicationException" };
        }

        // ignore other exceptions
        return null;
    }
}
```

The second one `UnexpectedExceptionServerHandler` is responsible to process `InvalidOperationException` and `NotSupportedException`.

``` c#
public sealed class UnexpectedExceptionServerHandler : IServerErrorHandler
{
    public ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
    {
        if (error is NotSupportedException || error is InvalidOperationException)
        {
            // provide detailed information for the client error handler
            var detail = new UnexpectedErrorDetail
            {
                Message = error.Message,
                ExceptionType = error.GetType().FullName,
                FullException = error.ToString(),
                MethodName = context.ServerCallContext.Method
            };

            return new ServerFaultDetail { Detail = detail };
        }

        // ignore other exceptions
        return null;
    }
}
```

All other exceptions are handled by gRPC API.

## Create client error handlers

A client-side error handler is an implementation of interface `IClientErrorHandler` with one method `ThrowOrIgnore`.
The method can ignore the original `RpcException`, provided by gRPC API, or throws a custom exception to pass it to the caller.

To meet the requirements we implement two error handlers. The first one `ApplicationExceptionClientHandler` is responsible to process the marker `ApplicationException` provided by the server error handler.

``` c#
internal sealed class ApplicationExceptionClientHandler : IClientErrorHandler
{
    public void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail)
    {
        // if marker is ApplicationException
        if ((detail.Detail is string name) && name == "ApplicationException")
        {
            // throw custom exception
            throw new ApplicationException(detail.OriginalError.Status.Detail);
        }
    }
}
```

The second one `UnexpectedExceptionClientHandler` is responsible to process the marker `UnexpectedErrorDetail` provided by the server error handler.

``` c#
// custom exception with detailed information from server
public class UnexpectedErrorException : SystemException
{
    public UnexpectedErrorException(UnexpectedErrorDetail detail)
        : base(detail.Message)
    {
        Detail = detail;
    }

    public UnexpectedErrorDetail Detail { get; }
}

internal sealed class UnexpectedExceptionClientHandler : IClientErrorHandler
{
    public void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail)
    {
        // if marker is UnexpectedErrorDetail
        if (detail.Detail is UnexpectedErrorDetail unexpectedErrorDetail)
        {
            // throw custom exception
            throw new UnexpectedErrorException(unexpectedErrorDetail);
        }
    }
}
```

All other exceptions are handled by gRPC API.

## Configure global error handling in asp.net core server

An error handler can be attached globally, for all ServiceModel.Grpc services.

``` c#
var builder = WebApplication.CreateBuilder();

builder.Services
    .AddServiceModelGrpc(options =>
    {
        options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
    });
```

Or can be attached for a specific service.

``` c#
var builder = WebApplication.CreateBuilder();

builder.Services
    .AddServiceModelGrpcServiceOptions<DebugService>(options =>
    {
        options.ErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
    });
```

In case there is a global error handler and a handler for a specific service. The global one is ignored.

In this example, we register a global error handler.

``` c#
var builder = WebApplication.CreateBuilder();

builder.Services.AddSingleton<IServerErrorHandler>(_ =>
{
    // combine application and unexpected handlers into one handler
    var collection = new ServerErrorHandlerCollection(
        new ApplicationExceptionServerHandler(),
        new UnexpectedExceptionServerHandler());

    return collection;
});

builder.Services
    .AddServiceModelGrpc(options =>
    {
        options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
    });
```

## Configure global error handling in Grpc.Core.Server

An error handler can be attached for a specific service.

``` c#
Grpc.Core.Server server = ...;

server.Services.AddServiceModelSingleton(
    new DebugService(),
    options =>
    {
        // combine application and unexpected handlers into one handler
        options.ErrorHandler = new ServerErrorHandlerCollection(
            new ApplicationExceptionServerHandler(),
            new UnexpectedExceptionServerHandler());
    });

```

## Configure global error handling in client

An error handler can be attached globally, for all ServiceModel.Grpc service proxies.

``` c#
IClientFactory factory = new ClientFactory(new ServiceModelGrpcClientOptions
{
    ErrorHandler = ...
});
```

Or can be attached for a specific service.

``` c#
IClientFactory factory = new ClientFactory();
factory.AddClient<IDebugService>(options =>
{
    options.ErrorHandler = ...
});
```

In case there is a global error handler and a handler for a specific service. The global one is ignored.

In this example, we register a global error handler.

``` c#
private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
{
    // combine application and unexpected handlers into one handler
    ErrorHandler = new ClientErrorHandlerCollection(new ApplicationExceptionClientHandler(), new UnexpectedExceptionClientHandler())
});
```

## Run the application

[View sample code](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ErrorHandling).
