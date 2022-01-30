using System.ServiceModel;
using System.Threading.Tasks;

// ReSharper disable OperationContractWithoutServiceContract

namespace Contract
{
    // remove [ServiceContract]
    public interface ICalculator<TValue> : IRemoteService
    {
        [OperationContract]
        Task<TValue> Sum(TValue x, TValue y);

        [OperationContract]
        ValueTask<TValue> Multiply(TValue x, TValue y);
    }
}
