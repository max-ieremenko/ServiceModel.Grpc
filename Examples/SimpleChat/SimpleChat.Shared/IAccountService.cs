using System.ServiceModel;
using System.Threading.Tasks;
using System.Threading;

namespace SimpleChat.Shared;

[ServiceContract]
public interface IAccountService
{
    [OperationContract]
    Task<string> ResolveTokenAsync(string userName, CancellationToken cancellationToken = default);
}