using System.Threading.Tasks;
using Client;
using Grpc.Core;
using Service;
using ServiceModel.Grpc.Configuration;

namespace Demo.ServerSelfHost
{
    public static class Program
    {
        private const int Port = 8083;

        public static async Task Main(string[] args)
        {
            var server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", Port, ServerCredentials.Insecure)
                }
            };

            server.Services.AddServiceModelSingleton(
                new PersonService(),
                options =>
                {
                    // set MessagePackMarshaller as default Marshaller
                    options.MarshallerFactory = MessagePackMarshallerFactory.Default;
                });

            server.Start();

            try
            {
                await ClientCalls.CallPersonService(Port);
            }
            finally
            {
                await server.ShutdownAsync();
            }
        }
    }
}
