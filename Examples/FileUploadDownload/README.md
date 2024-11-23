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

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean      | Error      | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |----------:|-----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          | **348.03 ms** | **104.839 ms** | **62.388 ms** |  **2.21** |    **0.43** |  **9000.0000** |         **-** | **33095.75 KB** |       **33.82** |
| RentedArray | 4096       | False          | 351.44 ms |  90.128 ms | 59.614 ms |  2.24 |    0.41 |  2000.0000 |         - |  8438.45 KB |        8.62 |
| HttpClient  | 4096       | False          | 158.43 ms |  22.574 ms | 14.931 ms |  1.01 |    0.13 |          - |         - |   978.52 KB |        1.00 |
|             |            |                |           |            |           |       |         |            |           |             |             |
| **Default**     | **4096**       | **True**           | **472.32 ms** |  **27.815 ms** | **16.552 ms** |  **1.61** |    **0.09** | **20000.0000** |         **-** | **72570.96 KB** |       **71.14** |
| RentedArray | 4096       | True           | 480.61 ms |  26.576 ms | 15.815 ms |  1.63 |    0.09 | 13000.0000 |         - | 47479.95 KB |       46.54 |
| HttpClient  | 4096       | True           | 294.59 ms |  22.904 ms | 13.630 ms |  1.00 |    0.06 |          - |         - |  1020.13 KB |        1.00 |
|             |            |                |           |            |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **False**          |  **91.19 ms** |   **9.515 ms** |  **6.294 ms** |  **1.91** |    **0.14** |  **6800.0000** |  **200.0000** | **26699.26 KB** |      **101.93** |
| RentedArray | 65536      | False          |  83.05 ms |  23.646 ms | 15.641 ms |  1.74 |    0.32 |   250.0000 |         - |  1174.09 KB |        4.48 |
| HttpClient  | 65536      | False          |  47.91 ms |   2.873 ms |  1.901 ms |  1.00 |    0.05 |          - |         - |   261.94 KB |        1.00 |
|             |            |                |           |            |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **True**           | **197.43 ms** |   **6.069 ms** |  **3.612 ms** |  **1.24** |    **0.04** | **18333.3333** | **2666.6667** | **74628.21 KB** |      **187.33** |
| RentedArray | 65536      | True           | 197.23 ms |   2.846 ms |  1.694 ms |  1.24 |    0.03 | 13000.0000 |  333.3333 | 49121.89 KB |      123.30 |
| HttpClient  | 65536      | True           | 159.51 ms |   7.568 ms |  4.503 ms |  1.00 |    0.04 |          - |         - |   398.38 KB |        1.00 |
|             |            |                |           |            |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **False**          |  **75.78 ms** |   **6.871 ms** |  **4.544 ms** |  **2.10** |    **0.17** |  **6400.0000** | **1200.0000** | **26456.74 KB** |      **185.85** |
| RentedArray | 81920      | False          |  67.86 ms |   7.284 ms |  4.818 ms |  1.88 |    0.17 |   142.8571 |         - |   926.58 KB |        6.51 |
| HttpClient  | 81920      | False          |  36.22 ms |   3.576 ms |  2.366 ms |  1.00 |    0.09 |          - |         - |   142.35 KB |        1.00 |
|             |            |                |           |            |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **True**           | **189.65 ms** |   **8.650 ms** |  **5.722 ms** |  **1.21** |    **0.05** | **16000.0000** | **5333.3333** | **69907.82 KB** |      **180.42** |
| RentedArray | 81920      | True           | 189.31 ms |   7.244 ms |  4.311 ms |  1.21 |    0.05 | 11333.3333 |  333.3333 | 44429.34 KB |      114.66 |
| HttpClient  | 81920      | True           | 156.72 ms |   8.617 ms |  5.128 ms |  1.00 |    0.04 |          - |         - |   387.48 KB |        1.00 |

#### uploading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |-----------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          |   **374.0 ms** |  **74.35 ms** |  **49.18 ms** |  **1.56** |    **0.20** | **10000.0000** |         **-** | **35120.43 KB** |       **20.76** |
| RentedArray | 4096       | False          |   352.2 ms |  81.15 ms |  48.29 ms |  1.47 |    0.19 |  2000.0000 |         - |   9638.2 KB |        5.70 |
| HttpClient  | 4096       | False          |   239.7 ms |  10.16 ms |   6.04 ms |  1.00 |    0.03 |          - |         - |  1691.47 KB |        1.00 |
|             |            |                |            |           |           |       |         |            |           |             |             |
| **Default**     | **4096**       | **True**           |   **534.7 ms** |  **15.43 ms** |   **9.19 ms** |  **0.35** |    **0.13** | **21000.0000** | **1000.0000** | **77750.82 KB** |       **79.12** |
| RentedArray | 4096       | True           |   537.7 ms |  12.23 ms |   7.28 ms |  0.35 |    0.13 | 23000.0000 |         - | 78265.59 KB |       79.65 |
| HttpClient  | 4096       | True           | 1,654.5 ms | 527.59 ms | 348.97 ms |  1.08 |    0.46 |          - |         - |   982.64 KB |        1.00 |
|             |            |                |            |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **False**          |   **151.0 ms** |  **22.82 ms** |  **13.58 ms** |  **0.87** |    **0.11** |  **6666.6667** |  **666.6667** | **26506.95 KB** |       **29.87** |
| RentedArray | 65536      | False          |   134.6 ms |  16.81 ms |  11.12 ms |  0.77 |    0.10 |          - |         - |   937.72 KB |        1.06 |
| HttpClient  | 65536      | False          |   176.1 ms |  29.74 ms |  17.70 ms |  1.01 |    0.14 |          - |         - |   887.28 KB |        1.00 |
|             |            |                |            |           |           |       |         |            |           |             |             |
| **Default**     | **65536**      | **True**           |   **378.0 ms** |  **50.57 ms** |  **33.45 ms** |  **0.62** |    **0.05** | **19000.0000** | **2000.0000** | **74494.55 KB** |      **117.47** |
| RentedArray | 65536      | True           |   199.4 ms |   9.74 ms |   6.44 ms |  0.33 |    0.01 | 19333.3333 | 3333.3333 | 74410.41 KB |      117.34 |
| HttpClient  | 65536      | True           |   610.1 ms |  12.27 ms |   8.11 ms |  1.00 |    0.02 |          - |         - |   634.15 KB |        1.00 |
|             |            |                |            |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **False**          |   **112.2 ms** |  **13.58 ms** |   **8.98 ms** |  **0.78** |    **0.12** |  **6250.0000** |  **750.0000** | **26383.27 KB** |       **31.18** |
| RentedArray | 81920      | False          |   100.2 ms |  16.92 ms |  11.19 ms |  0.70 |    0.12 |          - |         - |   825.91 KB |        0.98 |
| HttpClient  | 81920      | False          |   145.9 ms |  31.71 ms |  20.97 ms |  1.02 |    0.19 |          - |         - |   846.23 KB |        1.00 |
|             |            |                |            |           |           |       |         |            |           |             |             |
| **Default**     | **81920**      | **True**           |   **192.0 ms** |   **7.22 ms** |   **4.30 ms** |  **0.32** |    **0.01** | **16333.3333** | **6000.0000** | **69801.18 KB** |      **143.58** |
| RentedArray | 81920      | True           |   188.9 ms |   7.71 ms |   4.59 ms |  0.31 |    0.01 | 17666.6667 | 5000.0000 | 69722.47 KB |      143.42 |
| HttpClient  | 81920      | True           |   605.3 ms |   9.79 ms |   6.48 ms |  1.00 |    0.01 |          - |         - |   486.15 KB |        1.00 |

### Grpc.Net.Client.Web with Grpc.AspNetCore.Web

Base line is HttpClient.

#### downloading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | Mode        | BufferSize | UseCompression | Mean      | Error      | StdDev     | Median    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |------------ |----------- |--------------- |----------:|-----------:|-----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Default**     | **GrpcWeb**     | **4096**       | **False**          | **336.85 ms** |  **25.966 ms** |  **15.452 ms** | **338.25 ms** |  **1.39** |    **0.13** |  **7000.0000** |         **-** | **26985.65 KB** |       **27.60** |
| RentedArray | GrpcWeb     | 4096       | False          | 275.15 ms |  60.107 ms |  39.757 ms | 267.28 ms |  1.13 |    0.18 |          - |         - |  1609.31 KB |        1.65 |
| HttpClient  | GrpcWeb     | 4096       | False          | 244.69 ms |  34.334 ms |  22.710 ms | 237.13 ms |  1.01 |    0.12 |          - |         - |   977.91 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **4096**       | **True**           | **382.14 ms** |  **14.099 ms** |   **7.374 ms** | **382.98 ms** |  **1.28** |    **0.10** | **19000.0000** |         **-** | **70086.39 KB** |       **68.74** |
| RentedArray | GrpcWeb     | 4096       | True           | 402.40 ms |  16.662 ms |   9.915 ms | 401.12 ms |  1.35 |    0.11 | 13000.0000 |         - | 45004.72 KB |       44.14 |
| HttpClient  | GrpcWeb     | 4096       | True           | 299.65 ms |  36.797 ms |  21.897 ms | 298.78 ms |  1.01 |    0.10 |          - |         - |  1019.58 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **65536**      | **False**          |  **53.54 ms** |   **3.381 ms** |   **2.236 ms** |  **52.99 ms** |  **1.11** |    **0.06** |  **6333.3333** |  **666.6667** | **26052.11 KB** |       **99.09** |
| RentedArray | GrpcWeb     | 65536      | False          |  48.60 ms |   3.635 ms |   2.163 ms |  49.03 ms |  1.00 |    0.06 |   100.0000 |         - |   619.78 KB |        2.36 |
| HttpClient  | GrpcWeb     | 65536      | False          |  48.49 ms |   3.499 ms |   2.082 ms |  48.10 ms |  1.00 |    0.06 |          - |         - |   262.92 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **65536**      | **True**           | **171.31 ms** |   **6.603 ms** |   **4.368 ms** | **172.00 ms** |  **1.03** |    **0.04** | **18666.6667** | **1666.6667** | **74272.73 KB** |      **188.66** |
| RentedArray | GrpcWeb     | 65536      | True           | 168.21 ms |   5.880 ms |   3.499 ms | 168.62 ms |  1.01 |    0.03 | 13000.0000 |  333.3333 | 48762.14 KB |      123.86 |
| HttpClient  | GrpcWeb     | 65536      | True           | 166.73 ms |   6.444 ms |   4.262 ms | 167.29 ms |  1.00 |    0.03 |          - |         - |   393.69 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **81920**      | **False**          |  **50.27 ms** |   **3.007 ms** |   **1.573 ms** |  **50.87 ms** |  **1.39** |    **0.07** |  **6222.2222** |  **444.4444** | **25930.93 KB** |      **182.11** |
| RentedArray | GrpcWeb     | 81920      | False          |  42.74 ms |   3.321 ms |   1.976 ms |  42.46 ms |  1.19 |    0.07 |    90.9091 |         - |   493.89 KB |        3.47 |
| HttpClient  | GrpcWeb     | 81920      | False          |  36.13 ms |   2.470 ms |   1.634 ms |  35.84 ms |  1.00 |    0.06 |          - |         - |   142.39 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWeb**     | **81920**      | **True**           | **196.46 ms** |  **22.708 ms** |  **15.020 ms** | **203.39 ms** |  **1.02** |    **0.09** | **16666.6667** | **5666.6667** | **69628.48 KB** |      **181.68** |
| RentedArray | GrpcWeb     | 81920      | True           | 190.10 ms |  19.351 ms |  11.515 ms | 193.81 ms |  0.98 |    0.07 | 11666.6667 | 5333.3333 | 44180.95 KB |      115.28 |
| HttpClient  | GrpcWeb     | 81920      | True           | 193.98 ms |  14.860 ms |   9.829 ms | 194.95 ms |  1.00 |    0.07 |          - |         - |   383.25 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **4096**       | **False**          | **421.71 ms** |  **56.347 ms** |  **37.270 ms** | **412.23 ms** |  **2.18** |    **0.23** |  **8000.0000** |         **-** | **28106.52 KB** |       **28.77** |
| RentedArray | GrpcWebText | 4096       | False          | 268.88 ms |  11.550 ms |   6.873 ms | 268.39 ms |  1.39 |    0.10 |          - |         - |   1610.4 KB |        1.65 |
| HttpClient  | GrpcWebText | 4096       | False          | 194.34 ms |  21.215 ms |  12.625 ms | 194.21 ms |  1.00 |    0.09 |          - |         - |   977.05 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **4096**       | **True**           | **457.29 ms** |  **17.913 ms** |  **11.849 ms** | **455.97 ms** |  **1.34** |    **0.11** | **19000.0000** |         **-** |  **71077.8 KB** |       **68.37** |
| RentedArray | GrpcWebText | 4096       | True           | 550.09 ms |  91.936 ms |  60.810 ms | 543.55 ms |  1.61 |    0.21 | 13000.0000 |         - | 45998.31 KB |       44.25 |
| HttpClient  | GrpcWebText | 4096       | True           | 343.03 ms |  44.615 ms |  29.510 ms | 330.57 ms |  1.01 |    0.11 |          - |         - |  1039.59 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **65536**      | **False**          | **288.71 ms** |  **29.991 ms** |  **19.837 ms** | **292.60 ms** |  **3.51** |    **1.80** |  **6000.0000** | **2000.0000** | **26421.75 KB** |      **100.10** |
| RentedArray | GrpcWebText | 65536      | False          | 100.32 ms |  10.275 ms |   6.796 ms | 102.55 ms |  1.22 |    0.63 |          - |         - |   582.96 KB |        2.21 |
| HttpClient  | GrpcWebText | 65536      | False          | 143.47 ms | 197.101 ms | 130.370 ms |  67.47 ms |  1.75 |    1.91 |          - |         - |   263.95 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **65536**      | **True**           | **317.41 ms** |  **25.830 ms** |  **17.085 ms** | **311.11 ms** |  **0.96** |    **0.05** | **19000.0000** | **1000.0000** | **74357.12 KB** |      **164.65** |
| RentedArray | GrpcWebText | 65536      | True           | 299.15 ms |  33.117 ms |  21.905 ms | 289.07 ms |  0.90 |    0.07 | 13000.0000 | 1000.0000 | 48848.18 KB |      108.16 |
| HttpClient  | GrpcWebText | 65536      | True           | 331.38 ms |  12.056 ms |   7.974 ms | 329.47 ms |  1.00 |    0.03 |          - |         - |   451.61 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **81920**      | **False**          | **468.93 ms** |  **60.343 ms** |  **39.913 ms** | **471.71 ms** | **10.06** |    **0.99** |  **6000.0000** | **1000.0000** | **26220.48 KB** |      **184.63** |
| RentedArray | GrpcWebText | 81920      | False          | 121.86 ms |  11.992 ms |   7.932 ms | 119.89 ms |  2.61 |    0.22 |          - |         - |   518.23 KB |        3.65 |
| HttpClient  | GrpcWebText | 81920      | False          |  46.75 ms |   4.631 ms |   2.756 ms |  46.79 ms |  1.00 |    0.08 |          - |         - |   142.02 KB |        1.00 |
|             |             |            |                |           |            |            |           |       |         |            |           |             |             |
| **Default**     | **GrpcWebText** | **81920**      | **True**           | **307.70 ms** |  **71.043 ms** |  **42.276 ms** | **301.25 ms** |  **1.91** |    **0.25** | **16000.0000** | **3000.0000** | **69691.02 KB** |      **153.74** |
| RentedArray | GrpcWebText | 81920      | True           | 310.44 ms |  65.514 ms |  43.333 ms | 292.21 ms |  1.93 |    0.26 | 11000.0000 | 5000.0000 | 44207.66 KB |       97.52 |
| HttpClient  | GrpcWebText | 81920      | True           | 160.96 ms |   7.335 ms |   3.836 ms | 161.79 ms |  1.00 |    0.03 |          - |         - |    453.3 KB |        1.00 |

#### uploading

Uploading is not implemented because of grpc-web limitations, see [gRPC-Web and streaming](https://learn.microsoft.com/en-us/aspnet/core/grpc/grpcweb?view=aspnetcore-8.0#grpc-web-and-streaming):  

*... clients don't support calling client streaming and bidirectional streaming methods ...* 

### Grpc.Core.Channel with Grpc.Core.Server

Base line is RentedArray.

#### downloading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |------------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          |   **612.37 ms** | **15.708 ms** | **10.390 ms** |  **0.99** |    **0.02** | **9000.0000** |         **-** | **29791.21 KB** |        **6.78** |
| RentedArray | 4096       | False          |   618.65 ms | 15.566 ms | 10.296 ms |  1.00 |    0.02 | 1000.0000 |         - |  4392.34 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **4096**       | **True**           | **1,833.44 ms** | **38.490 ms** | **25.459 ms** |  **0.99** |    **0.03** | **7000.0000** |         **-** | **29492.48 KB** |        **7.16** |
| RentedArray | 4096       | True           | 1,856.24 ms | 75.379 ms | 49.859 ms |  1.00 |    0.04 | 1000.0000 |         - |  4121.69 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **65536**      | **False**          |    **71.58 ms** |  **2.784 ms** |  **1.657 ms** |  **1.01** |    **0.03** | **6571.4286** |  **428.5714** | **25870.66 KB** |       **88.34** |
| RentedArray | 65536      | False          |    71.00 ms |  2.801 ms |  1.853 ms |  1.00 |    0.03 |         - |         - |   292.85 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **65536**      | **True**           | **1,397.41 ms** | **12.265 ms** |  **7.299 ms** |  **1.00** |    **0.01** | **6000.0000** | **2000.0000** | **25862.29 KB** |       **90.42** |
| RentedArray | 65536      | True           | 1,402.21 ms | 19.235 ms | 10.060 ms |  1.00 |    0.01 |         - |         - |   286.03 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **81920**      | **False**          |    **63.76 ms** |  **3.276 ms** |  **2.167 ms** |  **1.02** |    **0.04** | **6250.0000** | **1250.0000** | **25804.82 KB** |      **107.64** |
| RentedArray | 81920      | False          |    62.65 ms |  2.872 ms |  1.502 ms |  1.00 |    0.03 |         - |         - |   239.73 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **81920**      | **True**           | **1,459.01 ms** | **61.695 ms** | **40.808 ms** |  **1.04** |    **0.03** | **6000.0000** |         **-** | **25798.64 KB** |      **109.46** |
| RentedArray | 81920      | True           | 1,401.33 ms | 12.185 ms |  6.373 ms |  1.00 |    0.01 |         - |         - |   235.69 KB |        1.00 |

#### uploading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |------------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|------------:|
| **Default**     | **4096**       | **False**          |   **636.95 ms** | **18.546 ms** | **12.267 ms** |  **1.01** |    **0.02** | **9000.0000** |         **-** | **29667.71 KB** |        **6.99** |
| RentedArray | 4096       | False          |   629.92 ms |  9.516 ms |  5.663 ms |  1.00 |    0.01 | 1000.0000 |         - |  4243.98 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **4096**       | **True**           | **1,849.89 ms** | **89.932 ms** | **59.485 ms** |  **1.00** |    **0.04** | **7000.0000** |         **-** | **29392.16 KB** |        **7.32** |
| RentedArray | 4096       | True           | 1,857.95 ms | 76.662 ms | 50.707 ms |  1.00 |    0.04 |         - |         - |  4016.61 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **65536**      | **False**          |    **71.49 ms** |  **1.853 ms** |  **0.969 ms** |  **1.01** |    **0.03** | **6571.4286** |  **428.5714** | **25863.66 KB** |       **90.73** |
| RentedArray | 65536      | False          |    70.51 ms |  2.640 ms |  1.571 ms |  1.00 |    0.03 |         - |         - |   285.07 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **65536**      | **True**           | **1,480.96 ms** | **14.971 ms** |  **9.902 ms** |  **1.04** |    **0.02** | **6000.0000** | **2000.0000** |  **25854.7 KB** |       **93.31** |
| RentedArray | 65536      | True           | 1,421.43 ms | 42.640 ms | 28.203 ms |  1.00 |    0.03 |         - |         - |   277.09 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **81920**      | **False**          |    **64.08 ms** |  **2.686 ms** |  **1.776 ms** |  **1.04** |    **0.04** | **6250.0000** |  **875.0000** | **25796.46 KB** |      **110.74** |
| RentedArray | 81920      | False          |    61.79 ms |  3.297 ms |  1.962 ms |  1.00 |    0.04 |         - |         - |   232.94 KB |        1.00 |
|             |            |                |             |           |           |       |         |           |           |             |             |
| **Default**     | **81920**      | **True**           | **1,461.47 ms** | **48.508 ms** | **32.085 ms** |  **1.01** |    **0.04** | **6000.0000** |         **-** | **25790.24 KB** |       **99.24** |
| RentedArray | 81920      | True           | 1,448.77 ms | 65.137 ms | 43.084 ms |  1.00 |    0.04 |         - |         - |   259.88 KB |        1.00 |

### Grpc.Net.Client vs Grpc.Core.Channel, server is Grpc.AspNetCore.Server

Base line is HttpClient.

#### downloading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1     | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|---------:|------------:|------------:|
| **CoreChannel** | **4096**       | **False**          | **426.90 ms** | **34.474 ms** | **20.515 ms** |  **2.67** |    **0.17** |  **2000.0000** |        **-** |  **8119.62 KB** |        **8.31** |
| NetChannel  | 4096       | False          | 338.01 ms | 77.017 ms | 45.831 ms |  2.12 |    0.29 |  2000.0000 |        - |     8298 KB |        8.49 |
| HttpClient  | 4096       | False          | 159.91 ms | 12.618 ms |  7.509 ms |  1.00 |    0.06 |          - |        - |   977.11 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |          |             |             |
| **CoreChannel** | **4096**       | **True**           | **691.86 ms** | **13.398 ms** |  **8.862 ms** |  **2.30** |    **0.23** |  **4000.0000** |        **-** | **17633.27 KB** |       **17.07** |
| NetChannel  | 4096       | True           | 488.57 ms | 49.370 ms | 32.655 ms |  1.62 |    0.19 | 13000.0000 |        - | 46903.61 KB |       45.41 |
| HttpClient  | 4096       | True           | 304.24 ms | 50.840 ms | 33.628 ms |  1.01 |    0.15 |          - |        - |  1032.97 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |          |             |             |
| **CoreChannel** | **65536**      | **False**          |  **63.25 ms** |  **3.692 ms** |  **2.442 ms** |  **1.33** |    **0.09** |   **166.6667** |        **-** |   **735.14 KB** |        **2.75** |
| NetChannel  | 65536      | False          |  82.76 ms | 16.012 ms | 10.591 ms |  1.73 |    0.23 |   200.0000 |        - |  1149.04 KB |        4.30 |
| HttpClient  | 65536      | False          |  47.86 ms |  3.995 ms |  2.642 ms |  1.00 |    0.08 |          - |        - |   267.16 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |          |             |             |
| **CoreChannel** | **65536**      | **True**           | **244.74 ms** | **10.124 ms** |  **6.697 ms** |  **1.46** |    **0.05** |  **6000.0000** | **500.0000** | **23156.41 KB** |       **57.53** |
| NetChannel  | 65536      | True           | 197.84 ms | 10.870 ms |  7.190 ms |  1.18 |    0.05 | 12666.6667 | 333.3333 | 49113.48 KB |      122.02 |
| HttpClient  | 65536      | True           | 167.21 ms |  5.577 ms |  3.319 ms |  1.00 |    0.03 |          - |        - |   402.51 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |          |             |             |
| **CoreChannel** | **81920**      | **False**          |  **56.34 ms** |  **2.656 ms** |  **1.757 ms** |  **1.29** |    **0.23** |   **125.0000** |        **-** |   **594.16 KB** |        **4.18** |
| NetChannel  | 81920      | False          |  91.37 ms | 10.663 ms |  7.053 ms |  2.09 |    0.39 |   142.8571 |        - |    929.6 KB |        6.55 |
| HttpClient  | 81920      | False          |  45.20 ms | 13.318 ms |  8.809 ms |  1.03 |    0.26 |          - |        - |   141.99 KB |        1.00 |
|             |            |                |           |           |           |       |         |            |          |             |             |
| **CoreChannel** | **81920**      | **True**           | **234.20 ms** |  **9.033 ms** |  **5.375 ms** |  **1.44** |    **0.04** |  **4666.6667** | **333.3333** |  **18563.5 KB** |       **49.15** |
| NetChannel  | 81920      | True           | 191.19 ms |  8.316 ms |  5.501 ms |  1.18 |    0.04 | 11333.3333 | 333.3333 | 44428.98 KB |      117.63 |
| HttpClient  | 81920      | True           | 162.21 ms |  5.702 ms |  3.771 ms |  1.00 |    0.03 |          - |        - |   377.69 KB |        1.00 |

#### uploading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method      | BufferSize | UseCompression | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------------ |----------- |--------------- |------------:|-----------:|-----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **CoreChannel** | **4096**       | **False**          |   **598.09 ms** |  **25.746 ms** |  **15.321 ms** |  **2.32** |    **0.22** |          **-** |         **-** |  **3787.77 KB** |        **2.25** |
| NetChannel  | 4096       | False          |   368.86 ms |  63.182 ms |  41.791 ms |  1.43 |    0.20 |  2000.0000 |         - |  9819.16 KB |        5.84 |
| HttpClient  | 4096       | False          |   260.64 ms |  41.584 ms |  27.505 ms |  1.01 |    0.14 |          - |         - |  1681.47 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **CoreChannel** | **4096**       | **True**           | **1,555.78 ms** |  **49.615 ms** |  **32.817 ms** |  **2.10** |    **0.05** |  **8000.0000** |         **-** | **32111.82 KB** |       **32.95** |
| NetChannel  | 4096       | True           |   537.12 ms |  13.473 ms |   8.018 ms |  0.72 |    0.01 | 23000.0000 |         - | 78310.91 KB |       80.34 |
| HttpClient  | 4096       | True           |   741.60 ms |  13.708 ms |   9.067 ms |  1.00 |    0.02 |          - |         - |    974.7 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **CoreChannel** | **65536**      | **False**          |    **78.53 ms** |   **6.846 ms** |   **4.528 ms** |  **0.57** |    **0.07** |          **-** |         **-** |   **286.88 KB** |        **0.33** |
| NetChannel  | 65536      | False          |   101.65 ms |  12.397 ms |   8.200 ms |  0.73 |    0.09 |          - |         - |    950.5 KB |        1.10 |
| HttpClient  | 65536      | False          |   140.53 ms |  24.648 ms |  16.303 ms |  1.01 |    0.15 |          - |         - |   861.75 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **CoreChannel** | **65536**      | **True**           | **1,381.02 ms** |   **7.429 ms** |   **4.421 ms** |  **2.26** |    **0.03** |  **6000.0000** | **1000.0000** | **25886.74 KB** |       **51.59** |
| NetChannel  | 65536      | True           |   197.87 ms |   7.387 ms |   4.396 ms |  0.32 |    0.01 | 19333.3333 | 1666.6667 | 74410.83 KB |      148.30 |
| HttpClient  | 65536      | True           |   610.72 ms |  13.732 ms |   9.083 ms |  1.00 |    0.02 |          - |         - |   501.74 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **CoreChannel** | **81920**      | **False**          |    **69.23 ms** |   **3.267 ms** |   **2.161 ms** |  **0.52** |    **0.05** |          **-** |         **-** |   **234.58 KB** |        **0.28** |
| NetChannel  | 81920      | False          |   104.04 ms |  15.814 ms |   9.411 ms |  0.78 |    0.10 |          - |         - |   819.31 KB |        0.97 |
| HttpClient  | 81920      | False          |   134.63 ms |  19.955 ms |  13.199 ms |  1.01 |    0.14 |          - |         - |   846.62 KB |        1.00 |
|             |            |                |             |            |            |       |         |            |           |             |             |
| **CoreChannel** | **81920**      | **True**           | **1,379.71 ms** |   **9.096 ms** |   **5.413 ms** |  **1.52** |    **0.53** |  **6000.0000** |         **-** | **25803.16 KB** |       **41.43** |
| NetChannel  | 81920      | True           |   189.87 ms |   8.577 ms |   5.104 ms |  0.21 |    0.07 | 18000.0000 | 3666.6667 | 69724.77 KB |      111.96 |
| HttpClient  | 81920      | True           | 1,023.75 ms | 526.628 ms | 348.332 ms |  1.12 |    0.55 |          - |         - |   622.77 KB |        1.00 |
