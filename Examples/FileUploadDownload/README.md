# File upload/download example

Transferring [taxi-fare-test.csv](Files/taxi-fare-test.csv) file between client and server with and without compression. The file size is 23.9 MB, compressed with gzip fastest is about 5.68 MB.

Client and server are hosted in one process, no network latency (localhost).

## Transferring methods

### Grpc

Use `Grpc.Net.Client` and `Grpc.AspNetCore.Server` streaming. Allocate `byte[]` and skip serialization/deserialization: write/read directly into/from gRPC `SerializationContext`, see [DemoMarshallerFactory](Contract/DemoMarshallerFactory.cs).

``` c#
// [NonSerialized]
byte[]

Task<FileMetadata> UploadAsync(IAsyncEnumerable<byte[]> stream, string fileName, CancellationToken token = default);

ValueTask<(IAsyncEnumerable<byte[]> Stream, FileMetadata Metadata)> DownloadAsync(string filePath, int maxBufferSize, CancellationToken token = default);
```

### Http

Use `HttpClient`, pure HTTP/1.1, no gRPC.

## Benchmarks

Base line is Http.

### downloading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]   : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method | BufferSize | UseCompression | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------- |----------- |--------------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Grpc**   | **4096**       | **False**          | **359.77 ms** | **74.523 ms** | **44.348 ms** |  **2.81** |    **0.47** | **10000.0000** |         **-** | **34364.13 KB** |       **33.94** |
| Http   | 4096       | False          | 129.81 ms | 25.694 ms | 16.995 ms |  1.01 |    0.18 |          - |         - |  1012.63 KB |        1.00 |
|        |            |                |           |           |           |       |         |            |           |             |             |
| **Grpc**   | **4096**       | **True**           | **498.63 ms** | **23.969 ms** | **14.264 ms** |  **1.95** |    **0.06** | **20000.0000** |         **-** | **72753.79 KB** |       **69.22** |
| Http   | 4096       | True           | 255.90 ms |  8.727 ms |  4.564 ms |  1.00 |    0.02 |          - |         - |  1050.99 KB |        1.00 |
|        |            |                |           |           |           |       |         |            |           |             |             |
| **Grpc**   | **65536**      | **False**          | **103.91 ms** | **12.095 ms** |  **8.000 ms** |  **1.16** |    **0.32** |  **6750.0000** |  **250.0000** | **26719.41 KB** |      **100.42** |
| Http   | 65536      | False          |  96.52 ms | 43.744 ms | 28.934 ms |  1.08 |    0.43 |          - |         - |   266.08 KB |        1.00 |
|        |            |                |           |           |           |       |         |            |           |             |             |
| **Grpc**   | **65536**      | **True**           | **207.31 ms** | **15.606 ms** | **10.322 ms** |  **1.20** |    **0.06** | **17750.0000** | **4500.0000** | **74626.62 KB** |      **182.89** |
| Http   | 65536      | True           | 172.44 ms |  6.096 ms |  4.032 ms |  1.00 |    0.03 |          - |         - |   408.04 KB |        1.00 |
|        |            |                |           |           |           |       |         |            |           |             |             |
| **Grpc**   | **81920**      | **False**          |  **77.93 ms** |  **3.456 ms** |  **2.057 ms** |  **1.91** |    **0.40** |  **6333.3333** | **1333.3333** | **26482.66 KB** |      **183.56** |
| Http   | 81920      | False          |  42.89 ms | 16.399 ms | 10.847 ms |  1.05 |    0.34 |          - |         - |   144.27 KB |        1.00 |
|        |            |                |           |           |           |       |         |            |           |             |             |
| **Grpc**   | **81920**      | **True**           | **202.12 ms** | **12.619 ms** |  **8.347 ms** |  **1.21** |    **0.06** | **16000.0000** | **5333.3333** | **69908.35 KB** |      **174.04** |
| Http   | 81920      | True           | 167.64 ms |  6.697 ms |  4.429 ms |  1.00 |    0.04 |          - |         - |   401.69 KB |        1.00 |

### uploading

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]   : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

Job=ShortRun  Platform=X64  Force=True  
Server=False  IterationCount=10  LaunchCount=1  
RunStrategy=Throughput  WarmupCount=2  

```
| Method | BufferSize | UseCompression | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0       | Gen1      | Allocated   | Alloc Ratio |
|------- |----------- |--------------- |---------:|---------:|---------:|------:|--------:|-----------:|----------:|------------:|------------:|
| **Grpc**   | **4096**       | **False**          | **441.4 ms** | **72.89 ms** | **48.21 ms** |  **1.36** |    **0.17** | **10000.0000** |         **-** | **35472.19 KB** |       **16.40** |
| Http   | 4096       | False          | 326.1 ms | 38.74 ms | 23.06 ms |  1.00 |    0.09 |          - |         - |   2162.3 KB |        1.00 |
|        |            |                |          |          |          |       |         |            |           |             |             |
| **Grpc**   | **4096**       | **True**           | **575.0 ms** | **48.11 ms** | **31.82 ms** |  **0.75** |    **0.04** | **22000.0000** |         **-** |  **77985.1 KB** |       **79.57** |
| Http   | 4096       | True           | 763.5 ms | 12.52 ms |  7.45 ms |  1.00 |    0.01 |          - |         - |   980.13 KB |        1.00 |
|        |            |                |          |          |          |       |         |            |           |             |             |
| **Grpc**   | **65536**      | **False**          | **123.9 ms** | **21.43 ms** | **14.18 ms** |  **0.59** |    **0.13** |  **6750.0000** | **1500.0000** | **26535.02 KB** |       **30.44** |
| Http   | 65536      | False          | 218.3 ms | 60.02 ms | 39.70 ms |  1.03 |    0.27 |          - |         - |    871.7 KB |        1.00 |
|        |            |                |          |          |          |       |         |            |           |             |             |
| **Grpc**   | **65536**      | **True**           | **198.6 ms** | **23.29 ms** | **15.40 ms** |  **0.31** |    **0.02** | **18750.0000** | **2500.0000** |  **74503.3 KB** |      **148.54** |
| Http   | 65536      | True           | 636.5 ms | 18.06 ms | 10.75 ms |  1.00 |    0.02 |          - |         - |   501.58 KB |        1.00 |
|        |            |                |          |          |          |       |         |            |           |             |             |
| **Grpc**   | **81920**      | **False**          | **149.3 ms** | **59.80 ms** | **35.58 ms** |  **0.75** |    **0.18** |  **6500.0000** | **1000.0000** | **26381.31 KB** |       **30.87** |
| Http   | 81920      | False          | 201.3 ms | 33.42 ms | 17.48 ms |  1.01 |    0.12 |          - |         - |   854.63 KB |        1.00 |
|        |            |                |          |          |          |       |         |            |           |             |             |
| **Grpc**   | **81920**      | **True**           | **176.9 ms** | **31.81 ms** | **16.64 ms** |  **0.28** |    **0.03** | **16500.0000** | **5250.0000** | **69802.57 KB** |      **143.35** |
| Http   | 81920      | True           | 626.6 ms | 10.65 ms |  7.05 ms |  1.00 |    0.02 |          - |         - |   486.93 KB |        1.00 |
