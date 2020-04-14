# ServiceModel.Grpc

`ServiceModel.Grpc` enables applications to communicate with gRPC services using code-first approach, helps to get around some limitations of gRPC protocol like "only reference types", "exact one input", "no nulls":

### standard gRPC approach

``` proto
message SumRequest {
	int x = 1;
	int y = 2;
	int z = 3;
}

message SumResponse {
	long result = 1;
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
    long Sum(int x, int y, int z);
}

public class Calculator : ICalculator
{
    public long Sum(int x, int y, int z)
    {
        return x + y + z;
    }
}
```

## To start using ServiceModel.Grpc

The first place to start using ServiceModel.Grpc is [getting started example](Docs/GettingStarted).

For additional examples refer to [docs](Docs).


## Packages

-----
Package | Supported platforms | Description
------- | :------------------ | :----------
[![Version](https://img.shields.io/nuget/v/ServiceModel.Grpc.svg)](https://www.nuget.org/packages/ServiceModel.Grpc) | net461, netstandard2.0/2.1 | basic Grpc.Core.Api extensions, ClientFactory
[![Version](https://img.shields.io/nuget/v/ServiceModel.Grpc.AspNetCore.svg)](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore) | .net core 3.0 | Grpc.AspNetCore.Server extensions
[![Version](https://img.shields.io/nuget/v/ServiceModel.Grpc.SelfHost.svg)](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost) | net461, netstandard2.0/2.1 | Grpc.Core extensions for self-hosted Grpc.Core.Server