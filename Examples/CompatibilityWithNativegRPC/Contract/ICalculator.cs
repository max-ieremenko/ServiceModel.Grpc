using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract(Name = "CalculatorNative")]
    public interface ICalculator
    {
        [OperationContract(Name = "Sum")]
        Task<long> SumAsync(long x, int y, int z);

        [OperationContract(Name = "SumValues")]
        Task<long> SumValuesAsync(IAsyncEnumerable<int> values);

        [OperationContract(Name = "Range")]
        IAsyncEnumerable<int> Range(int start, int count);

        [OperationContract(Name = "MultiplyBy2")]
        IAsyncEnumerable<int> MultiplyBy2(IAsyncEnumerable<int> values);
    }
}
