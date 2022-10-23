using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface ICounterService
{
    [OperationContract]
    ValueTask<long> IncrementCountAsync();

    [OperationContract]
    ValueTask<long> AccumulateCountAsync(IAsyncEnumerable<int> amounts);

    [OperationContract]
    IAsyncEnumerable<long> CountdownAsync();
}