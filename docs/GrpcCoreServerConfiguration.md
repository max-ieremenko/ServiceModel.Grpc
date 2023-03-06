# ServiceModel.Grpc Grpc.Core server configuration

## AddServiceModel...

``` c#
var server = new Grpc.Core.Server();

server.Services.AddServiceModelSingleton<MyService>(
    new MyService(),
    options =>
    {
        // service configuration
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
        options.ServiceProvider = ...
        options.Filters ...
    });

server.Services.AddServiceModelTransient<MyService>(
    () => new MyService(),
    options =>
    {
        // service configuration
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
        options.ServiceProvider = ...
        options.Filters ...
    });

// register MyService in serviceProvider
IServiceProvider serviceProvider = ...;

server.Services.AddServiceModel<MyService>(
    serviceProvider,
    options =>
    {
        // service configuration
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
        options.Filters = ...
    });
```

## BindServiceModel...

``` c#
Grpc.Core.ServiceBinderBase serviceBinder = ...

serviceBinder.BindServiceModel<MyService>(
    () => new MyService(),
    options =>
    {
        // service configuration
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
        options.ServiceProvider = ...
        options.Filters = ...
    });

// register MyService in serviceProvider
IServiceProvider serviceProvider = ...;

serviceBinder.BindServiceModel<MyService>(
    serviceProvider,
    options =>
    {
        // service configuration
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
        options.Filters = ...
    });
```

#### ServiceModelGrpcServiceOptions

- IMarshallerFactory MarshallerFactory: by default is null (DataContractMarshallerFactory.Default)
- IServerErrorHandler ErrorHandler; by default is null (error handling by gRPC API)
- ILogger Logger: by default is null. To setup possible output provided by service binding
- IServiceProvider ServiceProvider: service provider instance
- FilterCollection\<IServerFilter\> Filters: collection of server filters