using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Service
{
    public sealed class PersonService : IPersonService
    {
        public Person CreatePerson(string fullName, DateTime dateOfBirth, IList<Address> addresses, CancellationToken token)
        {
            return new Person
            {
                FullName = fullName,
                DateOfBirth = dateOfBirth,
                Addresses = addresses
            };
        }

        public async Task<Person> CreatePersonAsync(string fullName, DateTime dateOfBirth, IList<Address> addresses, CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), token);

            return new Person
            {
                FullName = fullName,
                DateOfBirth = dateOfBirth,
                Addresses = addresses
            };
        }

        public async Task<Person> FindMostOldestPerson(IAsyncEnumerable<Person> persons, CancellationToken token)
        {
            Person result = null;
            await foreach (var person in persons.WithCancellation(token))
            {
                if (result == null || person.DateOfBirth < result.DateOfBirth)
                {
                    result = person;
                }
            }

            return result;
        }

        public async Task<Person> FindPersonOlderThan(IAsyncEnumerable<Person> persons, TimeSpan age, CancellationToken token)
        {
            await foreach (var person in persons.WithCancellation(token))
            {
                if ((DateTime.Now - person.DateOfBirth) > age)
                {
                    return person;
                }
            }

            return null;
        }

        public async IAsyncEnumerable<Person> GetRandomPersons(int count, TimeSpan minAge, [EnumeratorCancellation] CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), token);

            for (var i = 0; i < count; i++)
            {
                yield return new Person
                {
                    DateOfBirth = DateTime.Now.AddDays(-i) - minAge,
                    FullName = Guid.NewGuid().ToString()
                };
            }
        }

        public async IAsyncEnumerable<Person> GetRandomPersonUntilCancel([EnumeratorCancellation] CancellationToken token)
        {
            var index = 0;
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), token);

                yield return new Person
                {
                    DateOfBirth = DateTime.Now.AddDays(-index),
                    FullName = Guid.NewGuid().ToString()
                };

                index++;
            }
        }

        public async IAsyncEnumerable<Person> OrderPersonsByAge(IAsyncEnumerable<Person> persons, [EnumeratorCancellation] CancellationToken token)
        {
            var buffer = new List<Person>();
            await foreach (var person in persons.WithCancellation(token))
            {
                buffer.Add(person);
            }

            buffer.Sort((x, y) => x.DateOfBirth.CompareTo(y.DateOfBirth));

            foreach (var person in buffer)
            {
                yield return person;
            }
        }

        public async IAsyncEnumerable<Person> FindPersonsYoungerThan(IAsyncEnumerable<Person> persons, TimeSpan age, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var person in persons.WithCancellation(token))
            {
                if ((DateTime.Now - person.DateOfBirth) < age)
                {
                    yield return person;
                }
            }
        }
    }
}
