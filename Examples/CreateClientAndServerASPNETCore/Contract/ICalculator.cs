using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        Task<int> SumAsync(int x, int y, int z);
    }
}
