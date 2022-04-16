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

## Sync over async

`Sync over async` allows to define an operation as `async unary call` and `blocking unary call`.

Define the contract in the following manner:
- operation contract has to be defined as async
- the sync method is a decoration and is not a contract

``` c#
// blocking unary call is not operation contract
TResult DoSomething(T1 arg1, T2 arg2, CancellationToken token);

// async unary call is operation contract
[OperationContract]
Task<TResult> DoSomethingAsync(T1 arg1, T2 arg2, CancellationToken token);
```

Criteria:
- naming convention [DoSomething] + [Async]
- data input should be exactly the same: T1 arg1, T2 arg2
- return type should be exactly the same: TResult vs Task<TResult> or ValueTask<TResult>; void with Task or ValueTask
- context parameters matching is optional: DoSomething may or may not have CancellationToken

See [SyncOverAsync example](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/SyncOverAsync).
