using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Service
{
    public sealed class PersonService : IPersonService
    {
        // method Create is not an operation contract
        Person IPersonService.Create(string name, DateTime birthDay) => throw new NotSupportedException();

        public async Task<Person> CreateAsync(string name, DateTime birthDay)
        {
            await Task.Delay(100);

            return new Person
            {
                Name = name,
                BirthDay = birthDay
            };
        }

        // method Update is not an operation contract
        Person IPersonService.Update(Person person, string newName, DateTime newBirthDay) => throw new NotSupportedException();

        public ValueTask<Person> UpdateAsync(Person person, string newName, DateTime newBirthDay, CancellationToken token)
        {
            person.Name = newName;
            person.BirthDay = newBirthDay;
            return new ValueTask<Person>(person);
        }
    }
}
