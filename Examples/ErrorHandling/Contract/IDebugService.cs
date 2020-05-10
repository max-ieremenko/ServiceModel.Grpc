using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IDebugService
    {
        // always throws ApplicationException with a specific message
        [OperationContract]
        Task ThrowApplicationException(string message);

        // randomly throws InvalidOperationException or NotSupportedException with a specific message
        [OperationContract]
        Task ThrowRandomException(string message);
    }
}
