using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IGenericCalculator<TValue> : ICalculator<TValue>
    {
        [OperationContract]
        ValueTask<TValue> GetRandomValue();
    }
}
