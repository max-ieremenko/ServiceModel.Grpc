# Unary operation

- response is optional
- input is optional
- any number of input parameters
- context is optional
- the order of input and context parameters does not matter

``` c#
// blocking unary call
[OperationContract]
[TResult|void] OperationName([T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);

// async unary call
[OperationContract]
[Task<TResult>|Task] OperationName([T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);

// async unary call
[OperationContract]
[ValueTask<TResult>|ValueTask] OperationName([T1 arg1, T2 arg2, ..., TN argN], [CancellationToken|CallContext context]);
```
