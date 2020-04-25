using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;

namespace Service
{
    public sealed class PersonService : IPersonService
    {
        public PersonService(IPersonRepository repository)
        {
            Repository = repository;
        }

        public IPersonRepository Repository { get; }
        
        public Task<Person> Get(int personId)
        {
            return Repository.LoadByIdAsync(personId);
        }

        public Task<IList<Person>> GetAll()
        {
            return Repository.LoadAllAsync();
        }
    }
}
