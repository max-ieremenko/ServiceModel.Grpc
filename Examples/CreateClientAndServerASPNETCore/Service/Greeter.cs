using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Service
{
    internal sealed class Greeter : IGreeter
    {
        public Task<string> SayHelloAsync(string personFirstName, string personSecondName, CancellationToken token)
        {
            return Task.FromResult(string.Format("Hello {0} {1}", personFirstName, personSecondName));
        }

        public Task<string> SayHelloToAsync(Person person, CancellationToken token)
        {
            return Task.FromResult(string.Format("Hello {0} {1}", person.FirstName, person.SecondName));
        }
    }
}
