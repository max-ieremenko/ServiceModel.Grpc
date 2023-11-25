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

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          | **323.17 ms** | **49.405 ms** | **25.840 ms** |  **2.55** |    **0.20** | **10000.0000** |         **-** | **34811.52 KB** |       **35.53** |
| RentedArray | 4096       | False          | 309.66 ms | 72.777 ms | 43.309 ms |  2.45 |    0.34 |  2000.0000 |         - |  9806.09 KB |       10.01 |
| HttpClient  | 4096       | False          | 126.54 ms |  3.632 ms |  2.162 ms |  1.00 |    0.00 |          - |         - |   979.72 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **4096**       | **True**           | **707.82 ms** | **11.565 ms** |  **6.882 ms** |  **2.13** |    **0.04** | **20000.0000** |         **-** | **71328.57 KB** |       **63.86** |
| RentedArray | 4096       | True           | 709.82 ms | 17.025 ms |  8.904 ms |  2.13 |    0.05 | 13000.0000 |         - | 45962.09 KB |       41.15 |
| HttpClient  | 4096       | True           | 332.77 ms | 10.299 ms |  6.129 ms |  1.00 |    0.00 |          - |         - |  1116.89 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **False**          | **100.23 ms** |  **8.923 ms** |  **5.902 ms** |  **1.94** |    **0.14** |  **6750.0000** | **2250.0000** | **26741.13 KB** |       **85.19** |
| RentedArray | 65536      | False          |  96.92 ms | 18.150 ms | 12.005 ms |  1.87 |    0.22 |   250.0000 |         - |  1151.59 KB |        3.67 |
| HttpClient  | 65536      | False          |  51.79 ms |  3.200 ms |  2.116 ms |  1.00 |    0.00 |          - |         - |    313.9 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **True**           | **322.59 ms** | **12.163 ms** |  **7.238 ms** |  **1.24** |    **0.03** | **19000.0000** | **1000.0000** | **74443.98 KB** |      **188.45** |
| RentedArray | 65536      | True           | 319.27 ms | 21.394 ms | 14.151 ms |  1.23 |    0.03 | 13000.0000 | 1000.0000 | 48929.55 KB |      123.86 |
| HttpClient  | 65536      | True           | 260.10 ms | 10.127 ms |  6.699 ms |  1.00 |    0.00 |          - |         - |   395.04 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **False**          |  **76.50 ms** |  **4.749 ms** |  **3.141 ms** |  **1.96** |    **0.09** |  **6400.0000** |  **800.0000** | **26551.92 KB** |      **160.05** |
| RentedArray | 81920      | False          |  68.19 ms |  3.541 ms |  1.852 ms |  1.73 |    0.08 |   166.6667 |         - |   988.25 KB |        5.96 |
| HttpClient  | 81920      | False          |  39.04 ms |  2.282 ms |  1.509 ms |  1.00 |    0.00 |          - |         - |   165.89 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **True**           | **287.48 ms** | **12.215 ms** |  **7.269 ms** |  **1.12** |    **0.03** | **15500.0000** | **4500.0000** | **69892.27 KB** |      **179.18** |
| RentedArray | 81920      | True           | 311.89 ms | 15.005 ms |  9.925 ms |  1.22 |    0.04 | 11000.0000 | 1000.0000 | 44421.39 KB |      113.88 |
| HttpClient  | 81920      | True           | 256.06 ms |  8.924 ms |  5.903 ms |  1.00 |    0.00 |          - |         - |   390.06 KB |        1.00 |

#### uploading

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |------------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          |   **387.90 ms** | **62.950 ms** | **41.637 ms** |  **1.33** |    **0.14** | **11000.0000** |         **-** | **35826.09 KB** |       **16.91** |
| RentedArray | 4096       | False          |   358.76 ms | 18.902 ms |  9.886 ms |  1.21 |    0.04 |  2000.0000 |         - | 10581.55 KB |        4.99 |
| HttpClient  | 4096       | False          |   296.50 ms | 10.736 ms |  5.615 ms |  1.00 |    0.00 |          - |         - |  2118.86 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **4096**       | **True**           |   **804.77 ms** |  **9.999 ms** |  **5.230 ms** |  **0.68** |    **0.01** | **21000.0000** |         **-** | **75375.71 KB** |       **79.65** |
| RentedArray | 4096       | True           |   813.17 ms |  8.501 ms |  5.059 ms |  0.69 |    0.00 | 22000.0000 | 1000.0000 | 75791.55 KB |       80.09 |
| HttpClient  | 4096       | True           | 1,182.16 ms |  8.423 ms |  5.571 ms |  1.00 |    0.00 |          - |         - |   946.36 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **False**          |   **114.06 ms** | **20.491 ms** | **10.717 ms** |  **1.20** |    **0.08** |  **6666.6667** | **2000.0000** | **26538.08 KB** |       **31.02** |
| RentedArray | 65536      | False          |   111.55 ms | 18.228 ms | 12.057 ms |  1.18 |    0.13 |          - |         - |   962.54 KB |        1.13 |
| HttpClient  | 65536      | False          |    94.52 ms |  6.688 ms |  4.423 ms |  1.00 |    0.00 |          - |         - |   855.38 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **True**           |   **291.30 ms** | **13.902 ms** |  **9.195 ms** |  **0.28** |    **0.01** | **19000.0000** | **1000.0000** | **74304.73 KB** |      **154.42** |
| RentedArray | 65536      | True           |   295.92 ms | 10.185 ms |  6.737 ms |  0.28 |    0.01 | 19000.0000 | 3000.0000 |  74211.5 KB |      154.23 |
| HttpClient  | 65536      | True           | 1,053.71 ms | 11.137 ms |  7.366 ms |  1.00 |    0.00 |          - |         - |   481.17 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **False**          |   **111.68 ms** | **13.805 ms** |  **9.131 ms** |  **1.17** |    **0.14** |  **6500.0000** |  **750.0000** | **26391.61 KB** |       **31.41** |
| RentedArray | 81920      | False          |   104.14 ms | 10.613 ms |  7.020 ms |  1.09 |    0.09 |          - |         - |   837.88 KB |        1.00 |
| HttpClient  | 81920      | False          |    96.06 ms |  8.650 ms |  5.722 ms |  1.00 |    0.00 |          - |         - |   840.16 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **True**           |   **301.24 ms** | **18.230 ms** | **12.058 ms** |  **0.29** |    **0.01** | **16000.0000** | **5000.0000** | **69781.95 KB** |      **143.61** |
| RentedArray | 81920      | True           |   279.92 ms |  7.996 ms |  4.182 ms |  0.27 |    0.00 | 18000.0000 | 5500.0000 | 69702.03 KB |      143.44 |
| HttpClient  | 81920      | True           | 1,046.26 ms |  4.965 ms |  2.597 ms |  1.00 |    0.00 |          - |         - |   485.93 KB |        1.00 |

### Grpc.Net.Client.Web with Grpc.AspNetCore.Web

Base line is HttpClient.

#### downloading

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | Mode        | BufferSize | UseCompression | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |------------ |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **GrpcWeb**     | **4096**       | **False**          | **147.93 ms** | **20.721 ms** | **10.838 ms** |  **1.24** |    **0.11** |  **7000.0000** |         **-** | **26971.11 KB** |       **27.58** |
| RentedArray | GrpcWeb     | 4096       | False          | 151.62 ms | 70.290 ms | 41.828 ms |  1.28 |    0.38 |          - |         - |  1597.31 KB |        1.63 |
| HttpClient  | GrpcWeb     | 4096       | False          | 119.49 ms |  9.875 ms |  5.876 ms |  1.00 |    0.00 |          - |         - |      978 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **4096**       | **True**           | **596.92 ms** | **13.339 ms** |  **8.823 ms** |  **1.82** |    **0.04** | **20000.0000** |         **-** | **69975.52 KB** |       **62.67** |
| RentedArray | GrpcWeb     | 4096       | True           | 594.54 ms | 12.112 ms |  7.207 ms |  1.82 |    0.03 | 12000.0000 |         - |  44245.8 KB |       39.63 |
| HttpClient  | GrpcWeb     | 4096       | True           | 327.59 ms |  9.237 ms |  5.496 ms |  1.00 |    0.00 |          - |         - |  1116.58 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **65536**      | **False**          |  **57.70 ms** |  **3.834 ms** |  **2.282 ms** |  **1.12** |    **0.06** |  **6625.0000** |  **375.0000** | **26169.82 KB** |       **83.25** |
| RentedArray | GrpcWeb     | 65536      | False          |  52.47 ms |  3.695 ms |  2.199 ms |  1.02 |    0.06 |   111.1111 |         - |    715.7 KB |        2.28 |
| HttpClient  | GrpcWeb     | 65536      | False          |  51.51 ms |  3.193 ms |  2.112 ms |  1.00 |    0.00 |          - |         - |   314.35 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **65536**      | **True**           | **272.00 ms** |  **7.523 ms** |  **4.477 ms** |  **0.88** |    **0.05** | **19000.0000** | **2000.0000** | **74149.15 KB** |      **185.09** |
| RentedArray | GrpcWeb     | 65536      | True           | 272.85 ms |  9.206 ms |  6.089 ms |  0.88 |    0.05 | 13000.0000 |  500.0000 | 48636.94 KB |      121.41 |
| HttpClient  | GrpcWeb     | 65536      | True           | 309.13 ms | 24.317 ms | 16.084 ms |  1.00 |    0.00 |          - |         - |    400.6 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **81920**      | **False**          |  **61.37 ms** |  **4.647 ms** |  **3.074 ms** |  **1.45** |    **0.06** |  **6250.0000** | **1250.0000** | **25968.76 KB** |      **155.11** |
| RentedArray | GrpcWeb     | 81920      | False          |  51.86 ms |  3.554 ms |  2.351 ms |  1.23 |    0.06 |   100.0000 |         - |   533.78 KB |        3.19 |
| HttpClient  | GrpcWeb     | 81920      | False          |  42.31 ms |  5.191 ms |  3.433 ms |  1.00 |    0.00 |          - |         - |   167.43 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **81920**      | **True**           | **263.88 ms** |  **6.101 ms** |  **4.035 ms** |  **1.02** |    **0.02** | **15500.0000** | **5000.0000** | **69658.57 KB** |      **178.96** |
| RentedArray | GrpcWeb     | 81920      | True           | 262.74 ms |  6.372 ms |  3.792 ms |  1.02 |    0.02 | 11500.0000 |  500.0000 | 44178.01 KB |      113.50 |
| HttpClient  | GrpcWeb     | 81920      | True           | 258.15 ms |  9.593 ms |  6.345 ms |  1.00 |    0.00 |          - |         - |   389.24 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **4096**       | **False**          | **363.98 ms** | **10.681 ms** |  **7.065 ms** |  **3.18** |    **0.10** |  **9000.0000** |         **-** | **29035.36 KB** |       **29.72** |
| RentedArray | GrpcWebText | 4096       | False          | 201.11 ms | 40.348 ms | 24.010 ms |  1.75 |    0.22 |          - |         - |  1626.68 KB |        1.67 |
| HttpClient  | GrpcWebText | 4096       | False          | 114.83 ms |  5.730 ms |  3.410 ms |  1.00 |    0.00 |          - |         - |   976.98 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **4096**       | **True**           | **667.13 ms** | **32.868 ms** | **21.740 ms** |  **2.01** |    **0.07** | **19000.0000** |         **-** | **71028.75 KB** |       **63.85** |
| RentedArray | GrpcWebText | 4096       | True           | 674.30 ms | 14.481 ms |  8.618 ms |  2.02 |    0.06 | 13000.0000 |         - | 46187.93 KB |       41.52 |
| HttpClient  | GrpcWebText | 4096       | True           | 331.81 ms | 16.349 ms | 10.814 ms |  1.00 |    0.00 |          - |         - |  1112.42 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **65536**      | **False**          | **247.79 ms** | **43.136 ms** | **28.532 ms** |  **4.39** |    **0.59** |  **6000.0000** | **1000.0000** | **26479.41 KB** |       **84.44** |
| RentedArray | GrpcWebText | 65536      | False          |  88.24 ms |  7.451 ms |  4.928 ms |  1.56 |    0.13 |          - |         - |   580.89 KB |        1.85 |
| HttpClient  | GrpcWebText | 65536      | False          |  56.56 ms |  4.065 ms |  2.689 ms |  1.00 |    0.00 |          - |         - |    313.6 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **65536**      | **True**           | **318.59 ms** |  **9.199 ms** |  **4.811 ms** |  **1.22** |    **0.03** | **18000.0000** | **1000.0000** | **74222.05 KB** |      **188.13** |
| RentedArray | GrpcWebText | 65536      | True           | 333.65 ms | 20.271 ms | 13.408 ms |  1.28 |    0.04 | 13000.0000 |         - | 48716.63 KB |      123.48 |
| HttpClient  | GrpcWebText | 65536      | True           | 260.52 ms |  9.831 ms |  5.850 ms |  1.00 |    0.00 |          - |         - |   394.54 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **81920**      | **False**          | **235.03 ms** | **17.348 ms** | **11.474 ms** |  **6.35** |    **0.26** |  **6000.0000** | **2000.0000** | **26286.03 KB** |      **157.28** |
| RentedArray | GrpcWebText | 81920      | False          |  86.07 ms |  8.671 ms |  5.736 ms |  2.35 |    0.12 |          - |         - |   470.26 KB |        2.81 |
| HttpClient  | GrpcWebText | 81920      | False          |  37.41 ms |  1.601 ms |  0.837 ms |  1.00 |    0.00 |          - |         - |   167.13 KB |        1.00 |
|             |             |            |                |           |           |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **81920**      | **True**           | **321.46 ms** | **24.680 ms** | **16.324 ms** |  **1.25** |    **0.05** | **15000.0000** | **5000.0000** | **69717.29 KB** |      **179.53** |
| RentedArray | GrpcWebText | 81920      | True           | 316.19 ms | 11.792 ms |  6.167 ms |  1.23 |    0.02 | 11000.0000 | 5000.0000 | 44236.48 KB |      113.92 |
| HttpClient  | GrpcWebText | 81920      | True           | 256.94 ms |  9.817 ms |  6.493 ms |  1.00 |    0.00 |          - |         - |   388.32 KB |        1.00 |

#### uploading

Uploading is not implemented because of grpc-web limitations, see [gRPC-Web and streaming](https://learn.microsoft.com/en-us/aspnet/core/grpc/grpcweb?view=aspnetcore-8.0#grpc-web-and-streaming):  

*... clients don't support calling client streaming and bidirectional streaming methods ...* 

### Grpc.Core.Channel with Grpc.Core.Server

Base line is RentedArray.

#### downloading

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |------------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          |   **629.18 ms** | **23.016 ms** | **13.697 ms** |  **1.00** |    **0.03** |  **9000.0000** |         **-** | **29741.66 KB** |        **6.90** |
| RentedArray | 4096       | False          |   629.15 ms | 10.697 ms |  7.075 ms |  1.00 |    0.00 |  1000.0000 |         - |  4308.78 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **4096**       | **True**           | **1,593.62 ms** | **73.985 ms** | **48.937 ms** |  **0.86** |    **0.03** | **11000.0000** |         **-** | **29495.27 KB** |        **7.16** |
| RentedArray | 4096       | True           | 1,861.84 ms | 60.837 ms | 36.203 ms |  1.00 |    0.00 |  1000.0000 |         - |  4120.49 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **False**          |    **70.82 ms** |  **2.398 ms** |  **1.586 ms** |  **1.06** |    **0.03** |  **6857.1429** | **1285.7143** |  **25872.7 KB** |       **87.30** |
| RentedArray | 65536      | False          |    66.77 ms |  2.158 ms |  1.428 ms |  1.00 |    0.00 |          - |         - |   296.38 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **True**           | **1,430.41 ms** | **60.081 ms** | **39.740 ms** |  **1.00** |    **0.04** |  **6000.0000** | **2000.0000** | **25860.77 KB** |       **90.14** |
| RentedArray | 65536      | True           | 1,428.20 ms | 59.777 ms | 39.539 ms |  1.00 |    0.00 |          - |         - |   286.91 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **False**          |    **63.51 ms** |  **1.776 ms** |  **1.175 ms** |  **1.05** |    **0.02** |  **6500.0000** | **1125.0000** | **25805.67 KB** |      **105.84** |
| RentedArray | 81920      | False          |    60.55 ms |  1.977 ms |  1.308 ms |  1.00 |    0.00 |          - |         - |   243.82 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **True**           | **1,400.55 ms** | **16.810 ms** |  **8.792 ms** |  **0.95** |    **0.02** |  **6000.0000** |         **-** | **25795.41 KB** |      **109.85** |
| RentedArray | 81920      | True           | 1,468.33 ms | 36.853 ms | 24.376 ms |  1.00 |    0.00 |          - |         - |   234.82 KB |        1.00 |

#### uploading

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |------------:|-----------:|-----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          |   **646.74 ms** |   **9.141 ms** |   **6.046 ms** |  **1.00** |    **0.01** |  **9000.0000** |         **-** | **29590.13 KB** |        **6.97** |
| RentedArray | 4096       | False          |   643.61 ms |   9.916 ms |   6.559 ms |  1.00 |    0.00 |  1000.0000 |         - |  4242.76 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **Default**     | **4096**       | **True**           | **1,620.29 ms** |  **56.620 ms** |  **37.451 ms** |  **0.81** |    **0.08** | **12000.0000** |         **-** | **29392.35 KB** |        **7.32** |
| RentedArray | 4096       | True           | 2,029.47 ms | 338.871 ms | 224.142 ms |  1.00 |    0.00 |          - |         - |  4017.91 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **Default**     | **65536**      | **False**          |    **72.41 ms** |   **2.735 ms** |   **1.627 ms** |  **1.01** |    **0.03** |  **6857.1429** |  **285.7143** | **25861.56 KB** |       **90.46** |
| RentedArray | 65536      | False          |    71.37 ms |   2.670 ms |   1.766 ms |  1.00 |    0.00 |          - |         - |    285.9 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **Default**     | **65536**      | **True**           | **1,475.59 ms** |  **56.640 ms** |  **33.705 ms** |  **1.02** |    **0.04** |  **6000.0000** | **2000.0000** | **25856.01 KB** |       **93.15** |
| RentedArray | 65536      | True           | 1,448.76 ms |  62.727 ms |  41.490 ms |  1.00 |    0.00 |          - |         - |   277.57 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **Default**     | **81920**      | **False**          |    **64.43 ms** |   **1.627 ms** |   **1.076 ms** |  **1.03** |    **0.03** |  **6500.0000** |  **875.0000** | **25797.91 KB** |      **109.81** |
| RentedArray | 81920      | False          |    62.52 ms |   2.135 ms |   1.412 ms |  1.00 |    0.00 |          - |         - |   234.93 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **Default**     | **81920**      | **True**           | **1,426.31 ms** |  **62.744 ms** |  **41.502 ms** |  **1.01** |    **0.04** |  **6000.0000** |         **-** |    **25788 KB** |      **113.23** |
| RentedArray | 81920      | True           | 1,412.62 ms |  47.480 ms |  28.255 ms |  1.00 |    0.00 |          - |         - |   227.74 KB |        1.00 |

### Grpc.Net.Client vs Grpc.Core.Channel, server is Grpc.AspNetCore.Server

Base line is HttpClient.

#### downloading

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **CoreChannel** | **4096**       | **False**          | **411.24 ms** | **40.826 ms** | **27.004 ms** |  **3.33** |    **0.21** |  **2000.0000** |         **-** |  **8206.52 KB** |        **8.41** |
| NetChannel  | 4096       | False          | 309.60 ms | 65.427 ms | 38.935 ms |  2.49 |    0.29 |  2000.0000 |         - |   9474.7 KB |        9.71 |
| HttpClient  | 4096       | False          | 123.73 ms | 10.306 ms |  6.817 ms |  1.00 |    0.00 |          - |         - |   975.76 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **CoreChannel** | **4096**       | **True**           | **776.61 ms** | **38.810 ms** | **25.671 ms** |  **2.38** |    **0.06** |  **3000.0000** |         **-** | **14728.14 KB** |       **13.23** |
| NetChannel  | 4096       | True           | 710.61 ms | 11.204 ms |  6.667 ms |  2.17 |    0.05 | 13000.0000 |         - | 46030.49 KB |       41.33 |
| HttpClient  | 4096       | True           | 328.00 ms | 13.128 ms |  7.812 ms |  1.00 |    0.00 |          - |         - |  1113.61 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **CoreChannel** | **65536**      | **False**          |  **66.60 ms** |  **3.407 ms** |  **2.253 ms** |  **1.29** |    **0.05** |   **142.8571** |         **-** |   **746.04 KB** |        **2.38** |
| NetChannel  | 65536      | False          |  92.41 ms | 10.809 ms |  7.150 ms |  1.79 |    0.12 |   250.0000 |         - |  1152.35 KB |        3.68 |
| HttpClient  | 65536      | False          |  51.58 ms |  2.932 ms |  1.940 ms |  1.00 |    0.00 |          - |         - |   312.97 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **CoreChannel** | **65536**      | **True**           | **299.80 ms** |  **5.558 ms** |  **3.308 ms** |  **1.16** |    **0.02** |  **5500.0000** |  **500.0000** | **22998.85 KB** |       **58.05** |
| NetChannel  | 65536      | True           | 327.81 ms | 17.172 ms | 11.359 ms |  1.27 |    0.03 | 12000.0000 | 2000.0000 | 48937.21 KB |      123.53 |
| HttpClient  | 65536      | True           | 258.18 ms |  8.066 ms |  4.800 ms |  1.00 |    0.00 |          - |         - |   396.17 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **CoreChannel** | **81920**      | **False**          |  **59.92 ms** |  **4.120 ms** |  **2.725 ms** |  **1.56** |    **0.09** |   **125.0000** |         **-** |   **600.33 KB** |        **3.59** |
| NetChannel  | 81920      | False          |  69.87 ms |  5.580 ms |  3.691 ms |  1.82 |    0.07 |   166.6667 |         - |  1003.44 KB |        6.00 |
| HttpClient  | 81920      | False          |  38.49 ms |  3.187 ms |  2.108 ms |  1.00 |    0.00 |          - |         - |    167.3 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |           |             |             |
| **CoreChannel** | **81920**      | **True**           | **290.93 ms** | **10.525 ms** |  **6.962 ms** |  **1.14** |    **0.02** |  **4500.0000** |  **500.0000** | **18548.43 KB** |       **47.74** |
| NetChannel  | 81920      | True           | 324.17 ms | 33.095 ms | 21.890 ms |  1.29 |    0.07 | 11000.0000 | 1000.0000 | 44431.94 KB |      114.35 |
| HttpClient  | 81920      | True           | 254.59 ms |  7.657 ms |  4.556 ms |  1.00 |    0.00 |          - |         - |   388.56 KB |        1.00 |

#### uploading

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |------------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **CoreChannel** | **4096**       | **False**          |   **637.82 ms** | **20.519 ms** | **12.210 ms** |  **2.12** |    **0.10** |  **1000.0000** |         **-** |  **4286.43 KB** |        **2.05** |
| NetChannel  | 4096       | False          |   369.40 ms | 44.404 ms | 29.371 ms |  1.24 |    0.09 |  2000.0000 |         - |  10006.5 KB |        4.79 |
| HttpClient  | 4096       | False          |   301.14 ms | 23.209 ms | 13.811 ms |  1.00 |    0.00 |          - |         - |  2090.03 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **CoreChannel** | **4096**       | **True**           | **1,484.24 ms** | **20.004 ms** | **13.231 ms** |  **1.25** |    **0.01** |  **9000.0000** |         **-** | **31809.13 KB** |       **33.62** |
| NetChannel  | 4096       | True           |   808.76 ms | 12.949 ms |  7.706 ms |  0.68 |    0.01 | 22000.0000 |         - | 75958.02 KB |       80.28 |
| HttpClient  | 4096       | True           | 1,187.51 ms | 17.150 ms |  8.970 ms |  1.00 |    0.00 |          - |         - |   946.22 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **CoreChannel** | **65536**      | **False**          |    **80.62 ms** |  **4.547 ms** |  **3.007 ms** |  **0.85** |    **0.07** |          **-** |         **-** |   **305.06 KB** |        **0.36** |
| NetChannel  | 65536      | False          |   113.53 ms | 10.268 ms |  6.792 ms |  1.19 |    0.11 |          - |         - |   965.99 KB |        1.13 |
| HttpClient  | 65536      | False          |    95.44 ms |  7.371 ms |  4.875 ms |  1.00 |    0.00 |          - |         - |   856.23 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **CoreChannel** | **65536**      | **True**           | **1,376.87 ms** | **12.393 ms** |  **8.197 ms** |  **1.31** |    **0.01** |  **6000.0000** | **1000.0000** | **25879.13 KB** |       **53.63** |
| NetChannel  | 65536      | True           |   294.45 ms | 13.142 ms |  8.692 ms |  0.28 |    0.01 | 19000.0000 | 3500.0000 | 74211.34 KB |      153.78 |
| HttpClient  | 65536      | True           | 1,053.40 ms |  6.539 ms |  4.325 ms |  1.00 |    0.00 |          - |         - |   482.58 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **CoreChannel** | **81920**      | **False**          |    **70.53 ms** |  **2.260 ms** |  **1.495 ms** |  **0.75** |    **0.03** |          **-** |         **-** |   **249.54 KB** |        **0.30** |
| NetChannel  | 81920      | False          |   107.56 ms |  8.817 ms |  5.832 ms |  1.15 |    0.07 |          - |         - |    831.2 KB |        0.99 |
| HttpClient  | 81920      | False          |    93.70 ms |  5.445 ms |  3.240 ms |  1.00 |    0.00 |          - |         - |    840.1 KB |        1.00 |
|             |            |                |             |           |           |       |         |            |           |             |             |
| **CoreChannel** | **81920**      | **True**           | **1,381.99 ms** |  **8.621 ms** |  **5.130 ms** |  **1.32** |    **0.01** |  **6000.0000** |         **-** | **25815.63 KB** |       **53.39** |
| NetChannel  | 81920      | True           |   284.71 ms | 13.595 ms |  8.992 ms |  0.27 |    0.01 | 17500.0000 | 5500.0000 | 69703.09 KB |      144.16 |
| HttpClient  | 81920      | True           | 1,046.93 ms |  6.212 ms |  3.696 ms |  1.00 |    0.00 |          - |         - |   483.52 KB |        1.00 |
