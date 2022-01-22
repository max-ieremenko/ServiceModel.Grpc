using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IPersonService
    {
        // blocking unary call Create
        Person Create(string name, DateTime birthDay);

        // async unary call Create
        [OperationContract]
        Task<Person> CreateAsync(string name, DateTime birthDay);

        // blocking unary call Update
        Person Update(Person person, string newName, DateTime newBirthDay);

        // async unary call Update
        [OperationContract]
        ValueTask<Person> UpdateAsync(Person person, string newName, DateTime newBirthDay, CancellationToken token);
    }
}
