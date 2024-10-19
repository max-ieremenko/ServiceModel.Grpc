using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface IGreeterService
{
    [OperationContract]
    string SayHello(string name);

    [OperationContract]
    Task<string> SayHelloAsync(string name);

    [OperationContract]
    IAsyncEnumerable<string> SayHellosAsync(string name, CancellationToken token);

    [OperationContract]
    Task<string> SayHelloToLotsOfBuddiesAsync(IAsyncEnumerable<string> names, CancellationToken token);

    [OperationContract]
    IAsyncEnumerable<string> SayHellosToLotsOfBuddiesAsync(IAsyncEnumerable<string> names, CancellationToken token);
}