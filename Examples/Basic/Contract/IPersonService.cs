using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IPersonService
    {
        // blocking unary call
        [OperationContract]
        Person CreatePerson(string fullName, DateTime dateOfBirth, IList<Address> addresses, CancellationToken token = default);

        // async unary call
        [OperationContract]
        Task<Person> CreatePersonAsync(string fullName, DateTime dateOfBirth, IList<Address> addresses, CancellationToken token = default);

        // async client streaming call
        [OperationContract]
        Task<Person> FindMostOldestPerson(IAsyncEnumerable<Person> persons, CancellationToken token = default);

        // async client streaming call with external parameters
        [OperationContract]
        Task<Person> FindPersonOlderThan(IAsyncEnumerable<Person> persons, TimeSpan age, CancellationToken token = default);

        // async server streaming call
        [OperationContract]
        IAsyncEnumerable<Person> GetRandomPersons(int count, TimeSpan minAge, CancellationToken token = default);

        // async server streaming call
        [OperationContract]
        IAsyncEnumerable<Person> GetRandomPersonUntilCancel(CancellationToken token);

        // async server streaming call with external response parameters
        [OperationContract]
        Task<(int Count, Address Address, IAsyncEnumerable<Person> Persons)> GetListOfStreetResidents(string country, string city, string street, CancellationToken token = default);

        // async duplex streaming call
        [OperationContract]
        IAsyncEnumerable<Person> OrderPersonsByAge(IAsyncEnumerable<Person> persons, CancellationToken token = default);

        // async duplex streaming call with external parameters
        [OperationContract]
        IAsyncEnumerable<Person> FindPersonsYoungerThan(IAsyncEnumerable<Person> persons, TimeSpan age, CancellationToken token = default);

        // async duplex streaming call with external request and response parameters
        [OperationContract]
        Task<(Address Address, IAsyncEnumerable<Person> Persons)> FilterPersonsByAddress(IAsyncEnumerable<Person> persons, string country, string city, string street, CancellationToken token = default);
    }
}
