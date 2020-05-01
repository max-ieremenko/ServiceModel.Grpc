# ServiceModel.Grpc

`ServiceModel.Grpc` enables applications to communicate with gRPC services using code-first approach, helps to get around some limitations of gRPC protocol like "only reference types", "exact one input", "no nulls". Helps to migrate existing WCF solution to gRPC with minimum effort.

### standard gRPC approach

``` proto
message SumRequest {
	int64 x = 1;
	int32 y = 2;
	int32 z = 3;
}

message SumResponse {
	int64 result = 1;
}

service Calculator {
    rpc Sum (SumRequest) returns (SumResponse);
```

``` c#
public SumResponse Sum(SumRequest request)
{
    return new SumResponse { Result = request.X + request.Y + request.Z };
}
```

### ServiceModel.Grpc code-first approach

``` c#
[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    long Sum(long x, int y, int z);
}

public sealed class Calculator : ICalculator
{
    public long Sum(long x, int y, int z)
    {
        return x + y + z;
    }
}
```

Is ServiceModel.Grpc compatible with standard gRPC? [Yes](https://max-ieremenko.github.io/ServiceModel.Grpc/CompatibilityWithNativegRPC.html).

## Support

The first place to start using ServiceModel.Grpc is [create a gRPC client and server tutorial](https://max-ieremenko.github.io/ServiceModel.Grpc/CreateClientAndServerASPNETCore.html).

To migrate an existing WCF solution to a gRPC with ServiceModel.Grpc [here is](https://max-ieremenko.github.io/ServiceModel.Grpc/MigrateWCFServiceTogRPC.html) tutorial.

For additional information refer to [docs](https://max-ieremenko.github.io/ServiceModel.Grpc/).

If you have discovered a bug or have a feature suggestion, feel free to create an issue on Github.
I look forward to your feedback.

## NuGet feed

-----
Name | Package | Supported platforms | Description
-----| :------ |:------------------- | :----------
ServiceModel.Grpc | [![Version](https://img.shields.io/nuget/vpre/ServiceModel.Grpc.svg)](https://www.nuget.org/packages/ServiceModel.Grpc) | net461, netstandard2.0/2.1 | main internal functionality, basic Grpc.Core.Api extensions, ClientFactory
ServiceModel.Grpc.AspNetCore | [![Version](https://img.shields.io/nuget/vpre/ServiceModel.Grpc.AspNetCore.svg)](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore) | .net core 3.0 | Grpc.AspNetCore.Server extensions
ServiceModel.Grpc.SelfHost | [![Version](https://img.shields.io/nuget/vpre/ServiceModel.Grpc.SelfHost.svg)](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost) | net461, netstandard2.0/2.1 | Grpc.Core extensions for self-hosted Grpc.Core.Server
ServiceModel.Grpc.ProtoBufMarshaller | [![Version](https://img.shields.io/nuget/vpre/ServiceModel.Grpc.ProtoBufMarshaller.svg)](https://www.nuget.org/packages/ServiceModel.Grpc.ProtoBufMarshaller) | net461, netstandard2.0/2.1 | marshaller factory based on protobuf-net