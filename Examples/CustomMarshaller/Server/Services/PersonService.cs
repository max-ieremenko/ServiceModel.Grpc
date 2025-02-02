using System;
using System.Threading.Tasks;
using Contract;

namespace Server.Services;

internal sealed class PersonService : IPersonService
{
    public Task<Person> CreatePerson(string name, DateTime birthDay)
    {
        return Task.FromResult(new Person
        {
            Name = name,
            BirthDay = birthDay
        });
    }
}