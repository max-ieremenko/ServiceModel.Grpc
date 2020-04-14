using System;
using Contract;
using Grpc.Core;

namespace ServerSelfHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", ServiceConfiguration.Port, ServerCredentials.Insecure)
                }
            };

            server.Services.AddServiceModelSingleton(
                new PersonService(),
                options =>
                {
                    // set ProtobufMarshaller as default Marshaller
                    options.MarshallerFactory = ProtobufMarshallerFactory.Default;
                });

            server.Start();

            Console.WriteLine("Press enter for exit...");
            Console.ReadLine();
        }
    }
}
