using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;
using Shouldly;

namespace Client
{
    public sealed class ClientCalls
    {
        public IClientFactory ClientFactory { get; } = new ClientFactory();

        public async Task CallPersonService(int port)
        {
            var channel = new Channel("localhost", port, ChannelCredentials.Insecure);
            var proxy = ClientFactory.CreateClient<IPersonService>(channel);

            Console.WriteLine("Invoke CreatePerson");
            InvokeCreatePerson(proxy);

            Console.WriteLine("Invoke CreatePersonAsync");
            await InvokeCreatePersonAsync(proxy);

            Console.WriteLine("Invoke FindMostOldestPerson");
            await InvokeFindMostOldestPerson(proxy);

            Console.WriteLine("Invoke FindPersonOlderThan");
            await InvokeFindPersonOlderThan(proxy);

            Console.WriteLine("Invoke GetRandomPersons");
            await InvokeGetRandomPersons(proxy);

            Console.WriteLine("Invoke GetRandomPersonUntilCancel");
            await InvokeGetRandomPersonUntilCancel(proxy);

            Console.WriteLine("Invoke OrderPersonsByAge");
            await InvokeOrderPersonsByAge(proxy);

            Console.WriteLine("Invoke FindPersonsYoungerThan");
            await InvokeFindPersonsYoungerThan(proxy);
        }

        private static void InvokeCreatePerson(IPersonService proxy)
        {
            var personFullName = "full name";
            var personDateOfBirth = DateTime.Now.AddYears(-20).Date;
            var personAddress = new Address
            {
                City = "city name",
                Country = "country name",
                PostCode = "12345",
                Street = "street name"
            };

            Person person;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                person = proxy.CreatePerson(personFullName, personDateOfBirth, new[] { personAddress }, cancellationTokenSource.Token);
            }

            person.FullName.ShouldBe(personFullName);
            person.DateOfBirth.ShouldBe(personDateOfBirth);
            person.Addresses.Count.ShouldBe(1);
            person.Addresses[0].Country.ShouldBe(personAddress.Country);
            person.Addresses[0].City.ShouldBe(personAddress.City);
            person.Addresses[0].PostCode.ShouldBe(personAddress.PostCode);
            person.Addresses[0].Street.ShouldBe(personAddress.Street);
        }

        private static async Task InvokeCreatePersonAsync(IPersonService proxy)
        {
            var personFullName = "full name";
            var personDateOfBirth = DateTime.Now.AddYears(-20).Date;
            var personAddress = new Address
            {
                City = "city name",
                Country = "country name",
                PostCode = "12345",
                Street = "street name"
            };

            Person person;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                person = await proxy.CreatePersonAsync(personFullName, personDateOfBirth, new[] { personAddress }, cancellationTokenSource.Token);
            }

            person.FullName.ShouldBe(personFullName);
            person.DateOfBirth.ShouldBe(personDateOfBirth);
            person.Addresses.Count.ShouldBe(1);
            person.Addresses[0].Country.ShouldBe(personAddress.Country);
            person.Addresses[0].City.ShouldBe(personAddress.City);
            person.Addresses[0].PostCode.ShouldBe(personAddress.PostCode);
            person.Addresses[0].Street.ShouldBe(personAddress.Street);
        }

        private static async Task InvokeFindMostOldestPerson(IPersonService proxy)
        {
            var persons = new[]
            {
                new Person
                {
                    FullName = "person 20 years old",
                    DateOfBirth = DateTime.Now.AddYears(-20)
                },
                new Person
                {
                    FullName = "person 30 years old",
                    DateOfBirth = DateTime.Now.AddYears(-30)
                },
                new Person
                {
                    FullName = "person 40 years old",
                    DateOfBirth = DateTime.Now.AddYears(-40)
                }
            };

            Person mostOldestPerson;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                mostOldestPerson = await proxy.FindMostOldestPerson(persons.AsAsyncEnumerable(), cancellationTokenSource.Token);
            }

            mostOldestPerson.FullName.ShouldBe("person 40 years old");
        }
        
        private static async Task InvokeFindPersonOlderThan(IPersonService proxy)
        {
            var persons = new[]
            {
                new Person
                {
                    FullName = "person 20 years old",
                    DateOfBirth = DateTime.Now.AddYears(-20)
                },
                new Person
                {
                    FullName = "person 30 years old",
                    DateOfBirth = DateTime.Now.AddYears(-30)
                },
                new Person
                {
                    FullName = "person 40 years old",
                    DateOfBirth = DateTime.Now.AddYears(-40)
                }
            };

            Person person;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                person = await proxy.FindPersonOlderThan(persons.AsAsyncEnumerable(), TimeSpan.FromDays(35 * 360), cancellationTokenSource.Token);
            }

            person.FullName.ShouldBe("person 40 years old");
        }

        private static async Task InvokeGetRandomPersons(IPersonService proxy)
        {
            IList<Person> persons;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var response = proxy.GetRandomPersons(2, TimeSpan.FromDays(20 * 360), cancellationTokenSource.Token);
                persons = await response.ToListAsync();
            }

            persons.Count.ShouldBe(2);
        }

        private static async Task InvokeGetRandomPersonUntilCancel(IPersonService proxy)
        {
            var persons = new List<Person>();
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));

                var response = proxy.GetRandomPersonUntilCancel(cancellationTokenSource.Token);

                try
                {
                    await foreach (var person in response)
                    {
                        persons.Add(person);
                    }
                }
                catch (RpcException ex) when(ex.StatusCode == StatusCode.Cancelled)
                {
                }
            }

            persons.Count.ShouldBeGreaterThan(0);
        }

        private static async Task InvokeOrderPersonsByAge(IPersonService proxy)
        {
            var persons = new[]
            {
                new Person
                {
                    FullName = "person 40 years old",
                    DateOfBirth = DateTime.Now.AddYears(-40)
                },
                new Person
                {
                    FullName = "person 30 years old",
                    DateOfBirth = DateTime.Now.AddYears(-30)
                },
                new Person
                {
                    FullName = "person 20 years old",
                    DateOfBirth = DateTime.Now.AddYears(-20)
                }
            };

            IList<Person> orderedPersons;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var response = proxy.OrderPersonsByAge(persons.AsAsyncEnumerable(), cancellationTokenSource.Token);
                orderedPersons = await response.ToListAsync();
            }

            orderedPersons.Count.ShouldBe(3);
            orderedPersons[0].FullName = "person 20 years old";
            orderedPersons[1].FullName = "person 30 years old";
            orderedPersons[2].FullName = "person 40 years old";
        }

        private static async Task InvokeFindPersonsYoungerThan(IPersonService proxy)
        {
            var persons = new[]
            {
                new Person
                {
                    FullName = "person 40 years old",
                    DateOfBirth = DateTime.Now.AddYears(-40)
                },
                new Person
                {
                    FullName = "person 30 years old",
                    DateOfBirth = DateTime.Now.AddYears(-30)
                },
                new Person
                {
                    FullName = "person 20 years old",
                    DateOfBirth = DateTime.Now.AddYears(-20)
                }
            };

            IList<Person> youngPersons;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var response = proxy.FindPersonsYoungerThan(persons.AsAsyncEnumerable(), TimeSpan.FromDays(25 * 360), cancellationTokenSource.Token);
                youngPersons = await response.ToListAsync();
            }

            youngPersons.Count.ShouldBe(1);
            youngPersons[0].FullName = "person 20 years old";
        }
    }
}
