# Server streaming operation

- response is a stream of data, represented by IAsyncEnumerable<> interface
- input is optional
- context is optional
- the order of input and context parameters does not matter

``` c#
[OperationContract]
IAsyncEnumerable<TData> OperationName([T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);

[OperationContract]
Task<IAsyncEnumerable<TData>> OperationName([T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);

[OperationContract]
ValueTask<IAsyncEnumerable<TData>> OperationName([T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);
```

ServiceModel.Grpc framework supports external data in the response

``` c#
[OperationContract]
Task<(IAsyncEnumerable<TData> Stream, T1 arg1, T2 arg2, ..., TN argN)> OperationName(...);

[OperationContract]
ValueTask<(T1 arg1, T2 arg2, ..., TN argN, IAsyncEnumerable<TData> Stream)> OperationName(...);
```

The gRPC protocol does not support external data in the response. The only way to pass it to a client is by using HTTP response headers.
In case the operation contains external data in the response, ServiceModel.Grpc framework will automatically:

- serialize it with the current IMarshallerFactory
- pass data in the binary response header

for example:

``` c#
// standard gRPC server streaming call
[OperationContract]
Task<IAsyncEnumerable<int>> EnumerableRange(int start, int count);

// here values of Start and Count will be automatically passed in the binary response header
[OperationContract]
Task<(int Start, int Count, IAsyncEnumerable<int> Stream)> EnumerableRange(int start, int count);
```
