# Migrate from WCF FaultContract to a gRPC global error handling with ServiceModel.Grpc

This page proposes a solution how to migrate an existing WCF FaultContract exception handling to gRPC error handling with minimum effort.

How to migrate an existing WCF service and client is described [here](MigrateWCFServiceTogRPC.md).

How to activate and configure custom error handling in gRPC is described [here](global-error-handling.md).

[View sample code](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFFaultContractTogRpc).

The existing contract and service implementation.

``` c#
[ServiceContract]
public interface IDebugService
{
    // always throws FaultException<ApplicationExceptionFaultDetail>
    [OperationContract]
    [FaultContract(typeof(ApplicationExceptionFaultDetail))]
    Task ThrowApplicationException(string message);

    // always throws FaultException<InvalidOperationExceptionFaultDetail>
    [OperationContract]
    [FaultContract(typeof(InvalidOperationExceptionFaultDetail))]
    Task ThrowInvalidOperationException(string message);
}
```

The existing client calls

``` c#
IDebugService proxy = ...;

private static async Task CallThrowApplicationException(IDebugService proxy)
{
    Console.WriteLine("WCF call ThrowApplicationException");

    try
    {
        await proxy.ThrowApplicationException("some message");
    }
    catch (FaultException<ApplicationExceptionFaultDetail> ex)
    {
        Console.WriteLine("  Error message: {0}", ex.Detail.Message);
    }
}

private static async Task CallThrowInvalidOperationException(IDebugService proxy)
{
    Console.WriteLine("WCF call ThrowInvalidOperationException");

    try
    {
        await proxy.ThrowInvalidOperationException("some message");
    }
    catch (FaultException<InvalidOperationExceptionFaultDetail> ex)
    {
        Console.WriteLine("  Error message: {0}", ex.Detail.Message);
        Console.WriteLine("  StackTrace: {0}", ex.Detail.StackTrace);
    }
}

```

The goal is to migrate to gRPC without any changes in existing service and client implementations.

## Create server error handler

To provide fault detail from server to client via gRPC, a server error handler is required. The handler processes only 2 specific exceptions and ignores others:

``` c#
internal sealed class FaultExceptionServerHandler : ServerErrorHandlerBase
{
    protected override ServerFaultDetail? ProvideFaultOrIgnoreCore(ServerCallInterceptorContext context, Exception error)
    {
        // handle FaultException<ApplicationExceptionFaultDetail>
        if (error is FaultException<ApplicationExceptionFaultDetail> appFault)
        {
            // pass detail to a client call
            return new ServerFaultDetail
            {
                Detail = appFault.Detail
            };
        }

        // handle FaultException<InvalidOperationExceptionFaultDetail>
        if (error is FaultException<InvalidOperationExceptionFaultDetail> opFault)
        {
            // pass detail to a client call
            return new ServerFaultDetail
            {
                Detail = opFault.Detail
            };
        }

        // ignore other error
        return null;
    }
}
```

## Create client error handler

To provide fault detail from server to client via gRPC, a server error handler is required. The handler processes only 2 specific exceptions and ignores others:

``` c#
internal sealed class FaultExceptionClientHandler : ClientErrorHandlerBase
{
    protected override void ThrowOrIgnoreCore(ClientCallInterceptorContext context, ClientFaultDetail detail)
    {
        // handle ApplicationExceptionFaultDetail
        if (detail.Detail is ApplicationExceptionFaultDetail appDetail)
        {
            throw new FaultException<ApplicationExceptionFaultDetail>(appDetail);
        }

        // handle InvalidOperationExceptionFaultDetail
        if (detail.Detail is InvalidOperationExceptionFaultDetail opDetail)
        {
            throw new FaultException<InvalidOperationExceptionFaultDetail>(opDetail);
        }

        // ignore other errors
    }
}
```

## Configure global error handling in asp.net core server

Register a global error handler.

``` c#
// DebugModule.cs
container.RegisterType<IServerErrorHandler, FaultExceptionServerHandler>(new ContainerControlledLifetimeManager());

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // enable ServiceModel.Grpc
    services.AddServiceModelGrpc(options =>
    {
        // register server error handler
        options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
    });
}
```

## Configure global error handling in Grpc.Core.Server

Attach the error handler to DebugService.

``` c#
Server server = ...;

server.Services.AddServiceModelTransient(
    container.Resolve<Func<DebugService>>(),
    options =>
    {
        // register server error handler
        options.ErrorHandler = container.Resolve<IServerErrorHandler>();
    });
```

## Configure global error handling in client

Register a global error handler.

``` c#
private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
{
    // register client error handler
    ErrorHandler = new FaultExceptionClientHandler()
});
```

## Run the application

[View sample code](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFFaultContractTogRpc).

## Clean-up

After the migration is done. FaultContract attributes can be removed from the service contract.

``` c#
[ServiceContract]
public interface IDebugService
{
    [OperationContract]
    // [FaultContract(typeof(ApplicationExceptionFaultDetail))]
    Task ThrowApplicationException(string message);

    [OperationContract]
    // [FaultContract(typeof(InvalidOperationExceptionFaultDetail))]
    Task ThrowInvalidOperationException(string message);
}
```
