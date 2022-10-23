# Client streaming operation

- response is optional
- input is a stream of data, represented by IAsyncEnumerable<> interface
- additional input parameters are optional
- context is optional
- the order of stream, input and context parameters does not matter

``` c#
[OperationContract]
[Task<TResult>|Task] OperationName(IAsyncEnumerable<TData> stream, [T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);

[OperationContract]
[ValueTask<TResult>|ValueTask] OperationName(IAsyncEnumerable<TData> stream, [T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);
```

The gRPC protocol does not support input parameters. The only way to pass them to a server is by using HTTP request headers.
In case the operation contains external input ([T1 arg1, T2 arg2, ..., TN argN]), ServiceModel.Grpc framework will automatically:

- serialize them with the current IMarshallerFactory
- pass data in the binary request header

for example:

``` c#
// standard gRPC client streaming call
[OperationContract]
Task<int> MultiplyBy2(IAsyncEnumerable<int> values);

// here multiplier value will be automatically passed in the binary request header
[OperationContract]
Task<int> MultiplyBy(IAsyncEnumerable<int> values, int multiplier);
```
