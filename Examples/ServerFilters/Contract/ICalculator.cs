using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    ValueTask<int> SumAsync(int x, int y, CancellationToken token);

    [OperationContract]
    ValueTask<DivideByResult> DivideByAsync(int value, int divider, CancellationToken token);

    [OperationContract]
    ValueTask<(IAsyncEnumerable<int> Values, int Multiplier)> MultiplyByAsync(IAsyncEnumerable<int> values, int multiplier, CancellationToken token);
}