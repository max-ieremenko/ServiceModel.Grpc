# Duplex streaming operation

- response is a stream of data, represented by IAsyncEnumerable<> interface
- input is a stream of data, represented by IAsyncEnumerable<> interface
- additional input parameters are optional
- context is optional
- the order of stream and context parameters does not matter

``` c#
[OperationContract]
IAsyncEnumerable<TResponse> OperationName(IAsyncEnumerable<Request> stream, [T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);

[OperationContract]
Task<IAsyncEnumerable<TResponse>> OperationName(IAsyncEnumerable<Request> stream, [T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);

[OperationContract]
ValueTask<IAsyncEnumerable<TResponse>> OperationName(IAsyncEnumerable<Request> stream, [T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);
```

ServiceModel.Grpc framework supports external data in the response

``` c#
[OperationContract]
Task<(IAsyncEnumerable<TData> Stream, T1 arg1, T2 arg2, ..., TN argN)> OperationName(...);

[OperationContract]
ValueTask<(T1 arg1, T2 arg2, ..., TN argN, IAsyncEnumerable<TData> Stream)> OperationName(...);
```

The gRPC protocol does not support input parameters and external data in the response. The only way to pass it to client is by using request/response headers.
In case the operation contains external data in the request/response, ServiceModel.Grpc framework will automatically:

- serialize it with current IMarshallerFactory
- pass data in the binary request/response header

for example:

``` c#
// standard gRPC duplex streaming call
[OperationContract]
Task<IAsyncEnumerable<int>> MultiplyBy2(IAsyncEnumerable<int> values);

// here input multiplier value will be automatically passed in the binary request header
//   and the output multiplier value in the binary response header
[OperationContract]
Task<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier);
```
