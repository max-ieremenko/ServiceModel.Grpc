using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IDemoService
    {
        [OperationContract]
        Task<string> PingAsync();

        [OperationContract]
        Task<string> GetCurrentUserNameAsync();
    }
}
