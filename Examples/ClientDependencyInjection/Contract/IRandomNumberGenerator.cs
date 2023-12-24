using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface IRandomNumberGenerator
{
    [OperationContract]
    Task<int> NextInt32(CancellationToken token = default);
}