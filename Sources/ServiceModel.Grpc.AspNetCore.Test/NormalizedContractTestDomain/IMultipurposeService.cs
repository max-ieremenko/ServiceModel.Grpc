using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.AspNetCore.NormalizedContractTestDomain
{
    [ServiceContract]
    public interface IMultipurposeService
    {
        [OperationContract]
        string Concat(string value, CallContext context = default);

        [OperationContract]
        Task<string> ConcatAsync(string value, CallContext context = default);

        [OperationContract]
        IAsyncEnumerable<string> RepeatValue(string value, int count, CallContext context = default);

        [OperationContract]
        Task<long> SumValues(IAsyncEnumerable<int> values, CallContext context = default);

        [OperationContract]
        IAsyncEnumerable<string> ConvertValues(IAsyncEnumerable<int> values, CallContext context = default);
    }
}
