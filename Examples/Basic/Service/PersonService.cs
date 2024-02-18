using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Service;

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

    public async Task<Person?> FindMostOldestPerson(IAsyncEnumerable<Person> persons, CancellationToken token)
    {
        Person? result = null;
        await foreach (var person in persons.WithCancellation(token))
        {
            if (result == null || person.DateOfBirth < result.DateOfBirth)
            {
                result = person;
            }
        }

        return result;
    }

    public async Task<Person?> FindPersonOlderThan(IAsyncEnumerable<Person> persons, TimeSpan age, CancellationToken token)
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

    public async Task<(int Count, Address Address, IAsyncEnumerable<Person> Persons)> GetListOfStreetResidents(string country, string city, string street, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(100), token);

        var count = 3;
        var address = new Address
        {
            City = city,
            Country = country,
            Street = street
        };

        var persons = CreateStreetResidents(count, address, token);
        return (count, address, persons);
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

    public async Task<(Address Address, IAsyncEnumerable<Person> Persons)> FilterPersonsByAddress(IAsyncEnumerable<Person> persons, string country, string city, string street, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(100), token);

        var address = new Address
        {
            City = city,
            Country = country,
            Street = street
        };

        var filtered = FilterPersonsByAddress(persons, address, token);
        return (address, filtered);
    }

    private static async IAsyncEnumerable<Person> CreateStreetResidents(int top, Address address, [EnumeratorCancellation] CancellationToken token)
    {
        for (var i = 0; i < top; i++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), token);

            yield return new Person
            {
                FullName = Guid.NewGuid().ToString(),
                Addresses = new[] { address }
            };
        }
    }

    private static async IAsyncEnumerable<Person> FilterPersonsByAddress(IAsyncEnumerable<Person> persons, Address address, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var person in persons.WithCancellation(token).ConfigureAwait(false))
        {
            var match = person.Addresses != null && person.Addresses.Any(i =>
                string.Equals(i.City, address.City, StringComparison.OrdinalIgnoreCase)
                && string.Equals(i.Country, address.Country, StringComparison.OrdinalIgnoreCase)
                && string.Equals(i.Street, address.Street, StringComparison.OrdinalIgnoreCase));
            if (match)
            {
                yield return person;
            }
        }
    }
}