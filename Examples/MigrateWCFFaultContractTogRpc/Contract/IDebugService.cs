using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface IDebugService
{
    [OperationContract]
    [FaultContract(typeof(ApplicationExceptionFaultDetail))]
    Task ThrowApplicationException(string message);

    [OperationContract]
    [FaultContract(typeof(InvalidOperationExceptionFaultDetail))]
    Task ThrowInvalidOperationException(string message);
}