using System;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace Client
{
    public static class Program
    {
        private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // set ProtobufMarshaller as default Marshaller
            MarshallerFactory = ProtobufMarshallerFactory.Default
        });

        public static async Task Main(string[] args)
        {
            try
            {
                await Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("...");
            Console.ReadLine();
        }

        private static async Task Run()
        {
            var channel = new Channel("localhost", ServiceConfiguration.Port, ChannelCredentials.Insecure);

            var personService = DefaultClientFactory.CreateClient<IPersonService>(channel);
            var person = await personService.CreatePerson("John X", DateTime.Today.AddYears(-20));

            Console.WriteLine("Name: {0}", person.Name);
            Console.WriteLine("BirthDay: {0}", person.BirthDay);
            Console.WriteLine("CreatedBy: {0}", person.CreatedBy);
        }
    }
}
