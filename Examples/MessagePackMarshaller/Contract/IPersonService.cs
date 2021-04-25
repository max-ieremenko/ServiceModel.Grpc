using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IPersonService
    {
        [OperationContract]
        Task<Person> CreatePerson(string name, DateTime birthDay);
    }
}
