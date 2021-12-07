# File upload/download example

Transferring [taxi-fare-test.csv](Files/taxi-fare-test.csv) file between client and server with and without compression. The file size is 23.9 MB, compressed with gzip fastest is about 5.68 MB.

Client and server are hosted in one process, no network latency (localhost).

Comparison of 3 approaches.

## Transferring methods

### "Default"

Use gRPC client and server streaming. Allocate `byte[]` and skip serialization/deserialization: write/read directly into/from gRPC `SerializationContext`, see [DemoMarshallerFactory](Contract/DemoMarshallerFactory.cs).

``` c#
// [NonSerialized]
byte[]

Task<FileMetadata> UploadAsync(IAsyncEnumerable<byte[]> stream, string fileName, CancellationToken token = default);

ValueTask<(IAsyncEnumerable<byte[]> Stream, FileMetadata Metadata)> DownloadAsync(string filePath, int maxBufferSize, CancellationToken token = default);
```

### "RentedArray"

Use gRPC client and server streaming. Use ArrayPool to allocate `byte[]` and skip serialization/deserialization: write/read directly into/from gRPC `SerializationContext`, see [DemoMarshallerFactory](Contract/DemoMarshallerFactory.cs).

``` c#
// [NonSerialized]
public class RentedArray
{
    public static RentedArray Rent(int length, ArrayPool<byte> pool)
    {
        var array = pool.Rent(length);
        // ...
    }
}

[OperationContract]
Task<FileMetadata> UploadAsync(IAsyncEnumerable<RentedArray> stream, string fileName, CancellationToken token);

[OperationContract]
ValueTask<(IAsyncEnumerable<RentedArray> Stream, FileMetadata Metadata)> DownloadAsync(string filePath, int maxBufferSize, CancellationToken token);
```

### "HttpClient"

Use HttpClient, pure HTTP/1.1, no gRPC.

## Benchmark scenarios

### Grpc.Net.Client with Grpc.AspNetCore.Server

Base line is HttpClient.

#### downloading

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
|      Method | BufferSize | UseCompression |      Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|------------ |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|----------:|
|     **Default** |       **4096** |          **False** | **134.96 ms** | **37.515 ms** | **24.814 ms** |  **1.81** |    **0.31** |  **8000.0000** |         **-** | **26,504 KB** |
| RentedArray |       4096 |          False | 133.22 ms | 41.785 ms | 27.638 ms |  1.79 |    0.34 |          - |         - |  2,104 KB |
|  HttpClient |       4096 |          False |  76.12 ms |  1.617 ms |  0.962 ms |  1.00 |    0.00 |   166.6667 |         - |    935 KB |
|             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |       **4096** |           **True** | **433.71 ms** |  **5.978 ms** |  **3.954 ms** |  **1.77** |    **0.02** | **16000.0000** |         **-** | **67,685 KB** |
| RentedArray |       4096 |           True | 436.56 ms |  3.087 ms |  2.042 ms |  1.78 |    0.01 | 10000.0000 |         - | 43,400 KB |
|  HttpClient |       4096 |           True | 245.34 ms |  1.408 ms |  0.931 ms |  1.00 |    0.00 |          - |         - |  1,108 KB |
|             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |      **65536** |          **False** |  **48.06 ms** |  **0.563 ms** |  **0.373 ms** |  **1.44** |    **0.03** |  **5900.0000** | **1200.0000** | **25,603 KB** |
| RentedArray |      65536 |          False |  48.44 ms |  0.805 ms |  0.421 ms |  1.45 |    0.03 |   200.0000 |         - |  1,017 KB |
|  HttpClient |      65536 |          False |  33.36 ms |  1.254 ms |  0.746 ms |  1.00 |    0.00 |    62.5000 |         - |    303 KB |
|             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |      **65536** |           **True** | **211.76 ms** |  **3.861 ms** |  **2.554 ms** |  **1.08** |    **0.01** | **16000.0000** | **1000.0000** | **71,351 KB** |
| RentedArray |      65536 |           True | 209.71 ms |  2.228 ms |  1.474 ms |  1.07 |    0.01 | 11000.0000 | 2000.0000 | 46,833 KB |
|  HttpClient |      65536 |           True | 196.26 ms |  2.411 ms |  1.435 ms |  1.00 |    0.00 |          - |         - |    406 KB |
|             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |      **81920** |          **False** |  **49.18 ms** |  **0.383 ms** |  **0.200 ms** |  **1.87** |    **0.08** |  **5800.0000** |  **500.0000** | **25,407 KB** |
| RentedArray |      81920 |          False |  44.89 ms |  0.547 ms |  0.325 ms |  1.71 |    0.08 |   181.8182 |         - |    908 KB |
|  HttpClient |      81920 |          False |  26.16 ms |  1.767 ms |  1.169 ms |  1.00 |    0.00 |          - |         - |    180 KB |
|             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |      **81920** |           **True** | **206.94 ms** |  **4.793 ms** |  **3.170 ms** |  **1.09** |    **0.02** | **15000.0000** | **1000.0000** | **66,927 KB** |
| RentedArray |      81920 |           True | 206.38 ms |  3.322 ms |  2.198 ms |  1.09 |    0.01 | 10000.0000 | 1000.0000 | 42,432 KB |
|  HttpClient |      81920 |           True | 190.12 ms |  1.884 ms |  1.246 ms |  1.00 |    0.00 |          - |         - |    388 KB |

#### uploading

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
|      Method | BufferSize | UseCompression |        Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|------------ |----------- |--------------- |------------:|----------:|----------:|------:|--------:|-----------:|----------:|----------:|
|     **Default** |       **4096** |          **False** |   **235.58 ms** | **74.382 ms** | **49.199 ms** |  **1.70** |    **0.33** | **10000.0000** |         **-** | **34,099 KB** |
| RentedArray |       4096 |          False |   238.38 ms | 49.278 ms | 32.594 ms |  1.72 |    0.24 |  2000.0000 |         - |  9,775 KB |
|  HttpClient |       4096 |          False |   138.66 ms |  4.833 ms |  3.197 ms |  1.00 |    0.00 |   250.0000 |         - |  1,734 KB |
|             |            |                |             |           |           |       |         |            |           |           |
|     **Default** |       **4096** |           **True** |   **539.39 ms** | **12.261 ms** |  **8.110 ms** |  **0.52** |    **0.01** | **18000.0000** | **1000.0000** | **73,785 KB** |
| RentedArray |       4096 |           True |   549.72 ms |  7.546 ms |  4.991 ms |  0.53 |    0.00 | 19000.0000 |         - | 74,048 KB |
|  HttpClient |       4096 |           True | 1,046.07 ms |  3.290 ms |  2.176 ms |  1.00 |    0.00 |          - |         - |    965 KB |
|             |            |                |             |           |           |       |         |            |           |           |
|     **Default** |      **65536** |          **False** |   **127.07 ms** | **35.136 ms** | **23.240 ms** |  **2.09** |    **0.44** |  **5800.0000** |  **800.0000** | **25,642 KB** |
| RentedArray |      65536 |          False |   183.40 ms | 47.347 ms | 31.317 ms |  2.97 |    0.50 |   200.0000 |         - |  1,055 KB |
|  HttpClient |      65536 |          False |    61.51 ms |  4.514 ms |  2.686 ms |  1.00 |    0.00 |   142.8571 |         - |    928 KB |
|             |            |                |             |           |           |       |         |            |           |           |
|     **Default** |      **65536** |           **True** |   **244.84 ms** |  **3.161 ms** |  **1.653 ms** |  **0.25** |    **0.00** | **16000.0000** | **1000.0000** | **71,681 KB** |
| RentedArray |      65536 |           True |   246.20 ms |  5.069 ms |  3.353 ms |  0.25 |    0.00 | 17000.0000 | 4000.0000 | 71,615 KB |
|  HttpClient |      65536 |           True |   973.48 ms |  1.701 ms |  1.125 ms |  1.00 |    0.00 |          - |         - |    509 KB |
|             |            |                |             |           |           |       |         |            |           |           |
|     **Default** |      **81920** |          **False** |    **93.45 ms** | **28.042 ms** | **16.687 ms** |  **1.59** |    **0.29** |  **5000.0000** |         **-** | **25,608 KB** |
| RentedArray |      81920 |          False |   137.98 ms | 19.088 ms | 11.359 ms |  2.32 |    0.19 |   200.0000 |         - |    913 KB |
|  HttpClient |      81920 |          False |    59.58 ms |  3.073 ms |  1.607 ms |  1.00 |    0.00 |   111.1111 |         - |    913 KB |
|             |            |                |             |           |           |       |         |            |           |           |
|     **Default** |      **81920** |           **True** |   **234.93 ms** |  **4.392 ms** |  **2.905 ms** |  **0.24** |    **0.00** | **15000.0000** | **1000.0000** | **67,202 KB** |
| RentedArray |      81920 |           True |   236.11 ms |  4.141 ms |  2.166 ms |  0.24 |    0.00 | 15000.0000 | 3000.0000 | 67,134 KB |
|  HttpClient |      81920 |           True |   971.49 ms |  2.394 ms |  1.425 ms |  1.00 |    0.00 |          - |         - |    498 KB |

### Grpc.Net.Client.Web with Grpc.AspNetCore.Web

Base line is HttpClient.

#### downloading

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
|      Method |        Mode | BufferSize | UseCompression |      Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|------------ |------------ |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|----------:|
|     **Default** |     **GrpcWeb** |       **4096** |          **False** | **130.12 ms** | **19.401 ms** | **12.832 ms** |  **1.74** |    **0.15** |  **6000.0000** | **1000.0000** | **25,935 KB** |
| RentedArray |     GrpcWeb |       4096 |          False | 113.33 ms | 26.944 ms | 16.034 ms |  1.49 |    0.21 |          - |         - |  1,539 KB |
|  HttpClient |     GrpcWeb |       4096 |          False |  76.90 ms |  2.543 ms |  1.330 ms |  1.00 |    0.00 |   142.8571 |         - |    935 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |     **GrpcWeb** |       **4096** |           **True** | **412.46 ms** |  **3.996 ms** |  **2.643 ms** |  **1.68** |    **0.01** | **17000.0000** |         **-** | **67,378 KB** |
| RentedArray |     GrpcWeb |       4096 |           True | 420.75 ms |  4.060 ms |  2.686 ms |  1.72 |    0.01 | 11000.0000 |         - | 43,332 KB |
|  HttpClient |     GrpcWeb |       4096 |           True | 245.36 ms |  2.020 ms |  1.202 ms |  1.00 |    0.00 |          - |         - |  1,119 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |     **GrpcWeb** |      **65536** |          **False** |  **37.55 ms** |  **1.653 ms** |  **0.864 ms** |  **1.14** |    **0.03** |  **5714.2857** | **1571.4286** | **24,950 KB** |
| RentedArray |     GrpcWeb |      65536 |          False |  32.76 ms |  0.431 ms |  0.285 ms |  1.00 |    0.03 |   125.0000 |         - |    651 KB |
|  HttpClient |     GrpcWeb |      65536 |          False |  32.87 ms |  1.612 ms |  0.959 ms |  1.00 |    0.00 |          - |         - |    280 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |     **GrpcWeb** |      **65536** |           **True** | **208.10 ms** |  **3.586 ms** |  **2.372 ms** |  **1.08** |    **0.01** | **16000.0000** | **1000.0000** | **71,455 KB** |
| RentedArray |     GrpcWeb |      65536 |           True | 207.73 ms |  4.958 ms |  2.950 ms |  1.08 |    0.01 | 11000.0000 | 2000.0000 | 46,931 KB |
|  HttpClient |     GrpcWeb |      65536 |           True | 192.41 ms |  1.596 ms |  1.056 ms |  1.00 |    0.00 |          - |         - |    408 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |     **GrpcWeb** |      **81920** |          **False** |  **37.69 ms** |  **0.900 ms** |  **0.595 ms** |  **1.44** |    **0.04** |  **5461.5385** | **1538.4615** | **24,869 KB** |
| RentedArray |     GrpcWeb |      81920 |          False |  30.92 ms |  0.982 ms |  0.649 ms |  1.18 |    0.05 |    62.5000 |         - |    550 KB |
|  HttpClient |     GrpcWeb |      81920 |          False |  26.17 ms |  1.288 ms |  0.852 ms |  1.00 |    0.00 |          - |         - |    178 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** |     **GrpcWeb** |      **81920** |           **True** | **206.68 ms** |  **4.956 ms** |  **3.278 ms** |  **1.09** |    **0.02** | **15000.0000** | **1000.0000** | **67,000 KB** |
| RentedArray |     GrpcWeb |      81920 |           True | 203.05 ms |  3.776 ms |  2.498 ms |  1.07 |    0.02 | 10000.0000 |         - | 42,640 KB |
|  HttpClient |     GrpcWeb |      81920 |           True | 189.45 ms |  1.536 ms |  0.914 ms |  1.00 |    0.00 |          - |         - |    388 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** | **GrpcWebText** |       **4096** |          **False** | **306.11 ms** |  **5.176 ms** |  **3.080 ms** |  **3.73** |    **0.12** |  **9000.0000** |         **-** | **28,235 KB** |
| RentedArray | GrpcWebText |       4096 |          False | 156.18 ms |  9.734 ms |  5.792 ms |  1.91 |    0.09 |   333.3333 |         - |  1,547 KB |
|  HttpClient | GrpcWebText |       4096 |          False |  82.27 ms |  5.251 ms |  2.746 ms |  1.00 |    0.00 |   166.6667 |         - |    935 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** | **GrpcWebText** |       **4096** |           **True** | **503.35 ms** |  **4.160 ms** |  **2.476 ms** |  **1.95** |    **0.03** | **17000.0000** |         **-** | **68,649 KB** |
| RentedArray | GrpcWebText |       4096 |           True | 509.37 ms |  4.401 ms |  2.911 ms |  1.97 |    0.02 | 11000.0000 |         - | 44,354 KB |
|  HttpClient | GrpcWebText |       4096 |           True | 257.98 ms |  4.733 ms |  3.131 ms |  1.00 |    0.00 |          - |         - |  1,085 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** | **GrpcWebText** |      **65536** |          **False** | **184.70 ms** |  **1.285 ms** |  **0.850 ms** |  **5.59** |    **0.07** |  **5333.3333** | **1000.0000** | **25,449 KB** |
| RentedArray | GrpcWebText |      65536 |          False |  64.60 ms |  3.661 ms |  1.915 ms |  1.95 |    0.08 |          - |         - |    375 KB |
|  HttpClient | GrpcWebText |      65536 |          False |  33.10 ms |  0.886 ms |  0.463 ms |  1.00 |    0.00 |    62.5000 |         - |    316 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** | **GrpcWebText** |      **65536** |           **True** | **248.41 ms** |  **3.881 ms** |  **2.567 ms** |  **1.29** |    **0.01** | **16000.0000** | **1000.0000** | **71,527 KB** |
| RentedArray | GrpcWebText |      65536 |           True | 246.52 ms |  2.627 ms |  1.563 ms |  1.28 |    0.01 | 11000.0000 | 2000.0000 | 47,009 KB |
|  HttpClient | GrpcWebText |      65536 |           True | 192.30 ms |  0.914 ms |  0.605 ms |  1.00 |    0.00 |          - |         - |    410 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** | **GrpcWebText** |      **81920** |          **False** | **186.51 ms** |  **5.703 ms** |  **3.772 ms** |  **7.08** |    **0.27** |  **5000.0000** | **1000.0000** | **25,271 KB** |
| RentedArray | GrpcWebText |      81920 |          False |  53.55 ms |  0.559 ms |  0.370 ms |  2.03 |    0.06 |          - |         - |    430 KB |
|  HttpClient | GrpcWebText |      81920 |          False |  26.42 ms |  1.434 ms |  0.854 ms |  1.00 |    0.00 |          - |         - |    179 KB |
|             |             |            |                |           |           |           |       |         |            |           |           |
|     **Default** | **GrpcWebText** |      **81920** |           **True** | **247.57 ms** | **11.360 ms** |  **6.760 ms** |  **1.09** |    **0.13** | **15000.0000** | **1000.0000** | **67,060 KB** |
| RentedArray | GrpcWebText |      81920 |           True | 253.05 ms |  5.542 ms |  3.298 ms |  1.11 |    0.12 | 10000.0000 |  500.0000 | 42,575 KB |
|  HttpClient | GrpcWebText |      81920 |           True | 227.77 ms | 38.034 ms | 25.157 ms |  1.00 |    0.00 |          - |         - |    391 KB |

#### uploading

Uploading is not implemented because of grpc-web limitations, see [gRPC-Web and streaming](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-6.0#grpc-web-and-streaming):  

*... clients don't support calling client streaming and bidirectional streaming methods ...* 

### Grpc.Core.Channel with Grpc.Core.Server

Base line is RentedArray.

#### downloading

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
|      Method | BufferSize | UseCompression |        Mean |      Error |    StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Allocated |
|------------ |----------- |--------------- |------------:|-----------:|----------:|------:|--------:|----------:|----------:|----------:|
|     **Default** |       **4096** |          **False** |   **358.18 ms** |   **6.917 ms** |  **4.575 ms** |  **1.02** |    **0.02** | **8000.0000** |         **-** | **28,459 KB** |
| RentedArray |       4096 |          False |   352.30 ms |   7.561 ms |  5.001 ms |  1.00 |    0.00 |         - |         - |  4,062 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |       **4096** |           **True** | **1,337.67 ms** | **136.077 ms** | **90.007 ms** |  **0.95** |    **0.09** | **9000.0000** |         **-** | **28,457 KB** |
| RentedArray |       4096 |           True | 1,411.86 ms | 117.042 ms | 77.416 ms |  1.00 |    0.00 |         - |         - |  4,062 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **65536** |          **False** |    **41.95 ms** |   **8.258 ms** |  **5.462 ms** |  **1.13** |    **0.15** | **5833.3333** |  **416.6667** | **24,862 KB** |
| RentedArray |      65536 |          False |    37.10 ms |   0.718 ms |  0.375 ms |  1.00 |    0.00 |         - |         - |    280 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **65536** |           **True** | **1,334.83 ms** |   **6.416 ms** |  **4.244 ms** |  **1.00** |    **0.00** | **5000.0000** | **1000.0000** | **24,864 KB** |
| RentedArray |      65536 |           True | 1,334.82 ms |   4.681 ms |  3.096 ms |  1.00 |    0.00 |         - |         - |    287 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **81920** |          **False** |    **38.25 ms** |   **6.419 ms** |  **4.246 ms** |  **1.23** |    **0.14** | **5800.0000** |  **133.3333** | **24,798 KB** |
| RentedArray |      81920 |          False |    31.06 ms |   0.666 ms |  0.440 ms |  1.00 |    0.00 |   31.2500 |         - |    230 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **81920** |           **True** | **1,384.10 ms** |  **89.714 ms** | **59.340 ms** |  **0.98** |    **0.08** | **5000.0000** | **1000.0000** | **24,799 KB** |
| RentedArray |      81920 |           True | 1,411.79 ms |  90.170 ms | 59.642 ms |  1.00 |    0.00 |         - |         - |    240 KB |

#### uploading

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
|      Method | BufferSize | UseCompression |        Mean |      Error |    StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Allocated |
|------------ |----------- |--------------- |------------:|-----------:|----------:|------:|--------:|----------:|----------:|----------:|
|     **Default** |       **4096** |          **False** |   **314.45 ms** |   **4.290 ms** |  **2.553 ms** |  **1.00** |    **0.02** | **8000.0000** |         **-** | **28,354 KB** |
| RentedArray |       4096 |          False |   313.99 ms |   5.032 ms |  3.328 ms |  1.00 |    0.00 |         - |         - |  3,957 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |       **4096** |           **True** | **1,320.64 ms** | **144.151 ms** | **95.347 ms** |  **0.95** |    **0.07** | **8000.0000** |         **-** | **28,354 KB** |
| RentedArray |       4096 |           True | 1,388.63 ms | 104.606 ms | 69.190 ms |  1.00 |    0.00 |         - |         - |  3,963 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **65536** |          **False** |    **36.78 ms** |   **1.766 ms** |  **1.051 ms** |  **0.98** |    **0.03** | **5857.1429** |  **785.7143** | **24,853 KB** |
| RentedArray |      65536 |          False |    37.56 ms |   0.614 ms |  0.406 ms |  1.00 |    0.00 |         - |         - |    271 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **65536** |           **True** | **1,389.68 ms** |  **91.018 ms** | **60.203 ms** |  **0.96** |    **0.04** | **5000.0000** | **1000.0000** | **24,857 KB** |
| RentedArray |      65536 |           True | 1,448.46 ms |   3.801 ms |  2.514 ms |  1.00 |    0.00 |         - |         - |    274 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **81920** |          **False** |    **37.99 ms** |   **6.515 ms** |  **4.309 ms** |  **1.20** |    **0.15** | **5785.7143** |   **71.4286** | **24,790 KB** |
| RentedArray |      81920 |          False |    32.27 ms |   4.185 ms |  2.490 ms |  1.00 |    0.00 |         - |         - |    222 KB |
|             |            |                |             |            |           |       |         |           |           |           |
|     **Default** |      **81920** |           **True** | **1,448.21 ms** |  **60.078 ms** | **39.738 ms** |  **1.05** |    **0.05** | **5000.0000** | **1000.0000** | **24,799 KB** |
| RentedArray |      81920 |           True | 1,384.04 ms |  85.777 ms | 56.736 ms |  1.00 |    0.00 |         - |         - |    225 KB |

### Grpc.Net.Client vs Grpc.Core.Channel, server is Grpc.AspNetCore.Server

Base line is HttpClient.

#### downloading

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
|      Method | BufferSize | UseCompression |      Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|------------ |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|----------:|
| **CoreChannel** |       **4096** |          **False** | **225.02 ms** | **11.710 ms** |  **7.745 ms** |  **2.90** |    **0.09** |          **-** |         **-** |  **3,297 KB** |
|  NetChannel |       4096 |          False | 135.01 ms | 40.095 ms | 26.520 ms |  1.77 |    0.30 |          - |         - |  2,145 KB |
|  HttpClient |       4096 |          False |  78.07 ms |  2.397 ms |  1.427 ms |  1.00 |    0.00 |   200.0000 |         - |    936 KB |
|             |            |                |           |           |           |       |         |            |           |           |
| **CoreChannel** |       **4096** |           **True** | **477.48 ms** | **14.665 ms** |  **9.700 ms** |  **1.90** |    **0.06** |  **3000.0000** |         **-** | **14,330 KB** |
|  NetChannel |       4096 |           True | 438.07 ms |  3.567 ms |  1.866 ms |  1.74 |    0.03 | 11000.0000 |         - | 43,381 KB |
|  HttpClient |       4096 |           True | 251.11 ms |  6.279 ms |  4.153 ms |  1.00 |    0.00 |          - |         - |  1,108 KB |
|             |            |                |           |           |           |       |         |            |           |           |
| **CoreChannel** |      **65536** |          **False** |  **39.81 ms** |  **0.741 ms** |  **0.490 ms** |  **1.23** |    **0.06** |    **76.9231** |         **-** |    **640 KB** |
|  NetChannel |      65536 |          False |  48.31 ms |  0.284 ms |  0.188 ms |  1.49 |    0.07 |   181.8182 |         - |  1,030 KB |
|  HttpClient |      65536 |          False |  32.56 ms |  2.513 ms |  1.496 ms |  1.00 |    0.00 |          - |         - |    281 KB |
|             |            |                |           |           |           |       |         |            |           |           |
| **CoreChannel** |      **65536** |           **True** | **218.03 ms** |  **7.324 ms** |  **4.845 ms** |  **1.13** |    **0.03** |  **4000.0000** |         **-** | **21,962 KB** |
|  NetChannel |      65536 |           True | 210.41 ms |  3.505 ms |  2.318 ms |  1.09 |    0.01 | 11000.0000 | 2000.0000 | 46,848 KB |
|  HttpClient |      65536 |           True | 192.27 ms |  1.021 ms |  0.675 ms |  1.00 |    0.00 |          - |         - |    412 KB |
|             |            |                |           |           |           |       |         |            |           |           |
| **CoreChannel** |      **81920** |          **False** |  **36.39 ms** |  **0.876 ms** |  **0.521 ms** |  **1.42** |    **0.03** |    **71.4286** |         **-** |    **518 KB** |
|  NetChannel |      81920 |          False |  44.21 ms |  0.309 ms |  0.184 ms |  1.72 |    0.02 |   181.8182 |         - |    854 KB |
|  HttpClient |      81920 |          False |  25.71 ms |  0.617 ms |  0.323 ms |  1.00 |    0.00 |    31.2500 |         - |    180 KB |
|             |            |                |           |           |           |       |         |            |           |           |
| **CoreChannel** |      **81920** |           **True** | **208.22 ms** |  **5.226 ms** |  **3.457 ms** |  **1.10** |    **0.02** |  **3000.0000** |         **-** | **17,636 KB** |
|  NetChannel |      81920 |           True | 205.56 ms |  4.479 ms |  2.665 ms |  1.09 |    0.01 | 10000.0000 | 1000.0000 | 42,434 KB |
|  HttpClient |      81920 |           True | 189.33 ms |  0.824 ms |  0.491 ms |  1.00 |    0.00 |          - |         - |    438 KB |

#### uploading

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
|      Method | BufferSize | UseCompression |        Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|------------ |----------- |--------------- |------------:|----------:|----------:|------:|--------:|-----------:|----------:|----------:|
| **CoreChannel** |       **4096** |          **False** |   **371.81 ms** |  **6.840 ms** |  **4.070 ms** |  **2.71** |    **0.08** |  **1000.0000** |         **-** |  **4,661 KB** |
|  NetChannel |       4096 |          False |   248.31 ms | 60.477 ms | 40.002 ms |  1.81 |    0.30 |  2000.0000 |         - |  9,753 KB |
|  HttpClient |       4096 |          False |   137.16 ms |  4.382 ms |  2.898 ms |  1.00 |    0.00 |   250.0000 |         - |  1,787 KB |
|             |            |                |             |           |           |       |         |            |           |           |
| **CoreChannel** |       **4096** |           **True** | **1,184.76 ms** |  **6.843 ms** |  **4.526 ms** |  **1.13** |    **0.00** |  **9000.0000** |         **-** | **31,985 KB** |
|  NetChannel |       4096 |           True |   538.63 ms | 12.651 ms |  8.368 ms |  0.51 |    0.01 | 19000.0000 |         - | 74,219 KB |
|  HttpClient |       4096 |           True | 1,047.47 ms |  3.066 ms |  2.028 ms |  1.00 |    0.00 |          - |         - |    969 KB |
|             |            |                |             |           |           |       |         |            |           |           |
| **CoreChannel** |      **65536** |          **False** |    **99.84 ms** | **27.737 ms** | **18.346 ms** |  **1.67** |    **0.32** |          **-** |         **-** |    **340 KB** |
|  NetChannel |      65536 |          False |   168.23 ms | 26.512 ms | 15.777 ms |  2.69 |    0.19 |   200.0000 |         - |  1,046 KB |
|  HttpClient |      65536 |          False |    61.60 ms |  4.394 ms |  2.298 ms |  1.00 |    0.00 |   166.6667 |         - |    929 KB |
|             |            |                |             |           |           |       |         |            |           |           |
| **CoreChannel** |      **65536** |           **True** | **1,341.30 ms** |  **3.696 ms** |  **2.444 ms** |  **1.37** |    **0.01** |  **5000.0000** | **1000.0000** | **24,981 KB** |
|  NetChannel |      65536 |           True |   247.47 ms |  4.031 ms |  2.667 ms |  0.25 |    0.00 | 17000.0000 | 4000.0000 | 71,615 KB |
|  HttpClient |      65536 |           True |   978.91 ms |  5.343 ms |  3.534 ms |  1.00 |    0.00 |          - |         - |    576 KB |
|             |            |                |             |           |           |       |         |            |           |           |
| **CoreChannel** |      **81920** |          **False** |    **65.71 ms** |  **6.724 ms** |  **4.001 ms** |  **1.06** |    **0.10** |          **-** |         **-** |    **277 KB** |
|  NetChannel |      81920 |          False |   122.06 ms | 26.638 ms | 17.619 ms |  1.94 |    0.35 |   200.0000 |         - |    913 KB |
|  HttpClient |      81920 |          False |    63.63 ms |  9.077 ms |  6.004 ms |  1.00 |    0.00 |   111.1111 |         - |    912 KB |
|             |            |                |             |           |           |       |         |            |           |           |
| **CoreChannel** |      **81920** |           **True** | **1,352.48 ms** |  **2.140 ms** |  **1.416 ms** |  **1.39** |    **0.00** |  **5000.0000** | **1000.0000** | **24,895 KB** |
|  NetChannel |      81920 |           True |   236.48 ms |  5.517 ms |  3.649 ms |  0.24 |    0.00 | 15000.0000 | 4000.0000 | 67,114 KB |
|  HttpClient |      81920 |           True |   973.06 ms |  1.798 ms |  1.189 ms |  1.00 |    0.00 |          - |         - |    497 KB |
