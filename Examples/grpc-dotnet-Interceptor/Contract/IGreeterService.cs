using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IGreeterService
    {
        [OperationContract]
        Task<string> SayHelloAsync(string name);

        [OperationContract]
        IAsyncEnumerable<string> SayHellosAsync(string name, CancellationToken token);
    }
}
