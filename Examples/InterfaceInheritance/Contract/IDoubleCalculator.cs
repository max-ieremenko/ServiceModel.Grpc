using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IDoubleCalculator : ICalculator<double>
    {
        [OperationContract]
        ValueTask<double> GetRandomValue();
    }
}
