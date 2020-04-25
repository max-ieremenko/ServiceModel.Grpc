using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;

namespace Service
{
    public sealed class PersonRepository : IPersonRepository
    {
        public Task<Person> LoadByIdAsync(int id)
        {
            var person = id > 0 ? CreatePerson(id) : null;
            return Task.FromResult(person);
        }

        public Task<IList<Person>> LoadAllAsync()
        {
            IList<Person> result = new List<Person>();
            for (var i = 0; i < 10; i++)
            {
                result.Add(CreatePerson(i + 1));
            }

            return Task.FromResult(result);
        }

        private static Person CreatePerson(int id)
        {
            return new Person
            {
                Id = id,
                FirstName = "first name " + id,
                SecondName = "second name " + id
            };
        }
    }
}
