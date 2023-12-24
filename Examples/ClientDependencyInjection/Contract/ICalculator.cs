using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    Task<long> Sum(int x, int y, CancellationToken token = default);
}