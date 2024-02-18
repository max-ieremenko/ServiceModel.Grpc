using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface IPersonService
{
    [OperationContract]
    Task<Person?> Get(int personId);

    [OperationContract]
    Task<IList<Person>> GetAll();
}