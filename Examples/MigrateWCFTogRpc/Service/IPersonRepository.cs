using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;

namespace Service;

public interface IPersonRepository
{
    Task<Person?> LoadByIdAsync(int id);

    Task<IList<Person>> LoadAllAsync();
}