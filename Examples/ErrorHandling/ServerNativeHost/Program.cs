using System;
using Contract;
using Grpc.Core;
using Service;
using ServiceModel.Grpc.Interceptors;

namespace ServerNativeHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", ServiceConfiguration.ServiceNativeGrpcPort, ServerCredentials.Insecure)
                }
            };

            server.Services.AddServiceModelSingleton(
                new DebugService(),
                options =>
                {
                    // combine application and unexpected handlers into one handler
                    options.ErrorHandler = new ServerErrorHandlerCollection(
                        new ApplicationExceptionServerHandler(),
                        new UnexpectedExceptionServerHandler());
                });

            server.Start();

            Console.WriteLine("Press enter for exit...");
            Console.ReadLine();
        }
    }
}
