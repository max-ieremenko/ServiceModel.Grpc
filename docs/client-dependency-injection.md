# ServiceModel.Grpc.Client.DependencyInjection

see [example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/ClientDependencyInjection)

## Integration with [Grpc.Net.ClientFactory](https://www.nuget.org/packages/Grpc.Net.ClientFactory)

``` c#
IServiceCollection services = ...

services
    // Grpc.Net.ClientFactory registration
    .AddGrpcClient<ICalculator>(options =>
    {
        options.Address = new Uri("https://localhost:5001");
    })
    // use ServiceModel.Grpc creator
    .ConfigureServiceModelGrpcClientCreator<ICalculator>((options, serviceProvider) =>
    {
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
    });

IServiceProvider serviceProvider = ...
ICalculator calculator = serviceProvider.GetRequiredService<ICalculator>();
```

## Simple client registration with custom gRPC channel

The channel will be resolved from `ServiceProvider`

``` c#
IServiceCollection services = ...;

// register channel
services.AddSingleton<ChannelBase>(GrpcChannel.ForAddress("http://localhost:5000"));

// register client
services
    .AddServiceModelGrpcClient<ICalculator>((options, serviceProvider) =>
    {
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
    });

IServiceProvider serviceProvider = ...
ICalculator calculator = serviceProvider.GetRequiredService<ICalculator>();
```

Provide the channel for client registration

``` c#
IServiceCollection services = ...;

// register client with channel
services
    .AddServiceModelGrpcClient<ICalculator>(
        (options, serviceProvider) =>
        {
            options.MarshallerFactory = ...
            options.ErrorHandler = ...
        },
        ChannelProviderFactory.Singleton(GrpcChannel.ForAddress("http://localhost:5000")));

IServiceProvider serviceProvider = ...
ICalculator calculator = serviceProvider.GetRequiredService<ICalculator>();
```

## ClientFactory registration with custom gRPC channel

The channel will be resolved from `ServiceProvider`

``` c#
IServiceCollection services = ...;

// register channel
services.AddSingleton<ChannelBase>(GrpcChannel.ForAddress("http://localhost:5000"));

services
    // configure ClientFactory
    .AddServiceModelGrpcClientFactory((options, serviceProvider) =>
    {
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
    })
    // register client
    .AddClient<ICalculator>();

IServiceProvider serviceProvider = ...
ICalculator calculator = serviceProvider.GetRequiredService<ICalculator>();
```

Provide the channel for factory registration

``` c#
IServiceCollection services = ...;

services
    // configure ClientFactory
    .AddServiceModelGrpcClientFactory((options, serviceProvider) =>
    {
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
    })
    // provide channel
    .ConfigureDefaultChannel(ChannelProviderFactory.Singleton(GrpcChannel.ForAddress("http://localhost:5000")))
    // register client
    .AddClient<ICalculator>();

IServiceProvider serviceProvider = ...
ICalculator calculator = serviceProvider.GetRequiredService<ICalculator>();
```