# ServiceModel.Grpc

`ServiceModel.Grpc` enables applications to communicate with gRPC services using code-first approach, helps to get around some limitations of gRPC protocol like "only reference types", "exact one input", "no nulls":

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

```C#
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

Is ServiceModel.Grpc compatible with standard gRPC? [Yes, it is](/Docs/CompatibilityWithNativegRPC.md).

## To start using ServiceModel.Grpc

The first place to start using ServiceModel.Grpc is [create a gRPC client and server example](Docs/CreateClientAndServerASPNETCore.md).

For additional examples refer to [docs](Docs).


## NuGet feed

-----
Package | Supported platforms | Description
------- | :------------------ | :----------
[![Version](https://img.shields.io/nuget/v/ServiceModel.Grpc.svg)](https://www.nuget.org/packages/ServiceModel.Grpc) | net461, netstandard2.0/2.1 | main internal functionality, basic Grpc.Core.Api extensions, ClientFactory
[![Version](https://img.shields.io/nuget/v/ServiceModel.Grpc.AspNetCore.svg)](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore) | .net core 3.0 | Grpc.AspNetCore.Server extensions
[![Version](https://img.shields.io/nuget/v/ServiceModel.Grpc.SelfHost.svg)](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost) | net461, netstandard2.0/2.1 | Grpc.Core extensions for self-hosted Grpc.Core.Server