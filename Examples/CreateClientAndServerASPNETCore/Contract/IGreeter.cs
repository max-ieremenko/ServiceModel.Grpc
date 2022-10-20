using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface IGreeter
{
    [OperationContract]
    Task<string> SayHelloAsync(string personFirstName, string personSecondName, CancellationToken token = default);

    [OperationContract]
    Task<string> SayHelloToAsync(Person person, CancellationToken token = default);
}