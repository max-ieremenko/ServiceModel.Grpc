using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ConsoleClient.Internal;
using Contract;
using Grpc.Net.Client.Web;

namespace ConsoleClient
{
    public static class Program
    {
        public static async Task Main()
        {
            using (var tokenSource = new AppExitTokenSource())
            {
                // ServerAspNetHost must be up and running
                await CallAspNetHost(tokenSource.Token);

                // ServerAspNetHost must be up and running
                await CallAspNetHostAsGrpcWeb(tokenSource.Token);

                // ServerSelfHost must be up and running
                await CallServerSelfHost(tokenSource.Token);
            }
        }

        public static string FindDemoFile()
        {
            const string FileName = "Files/taxi-fare-test.csv";

            string result = null;

            var directory = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(directory))
            {
                var path = Path.Combine(directory, FileName);
                if (File.Exists(path))
                {
                    result = path;
                    break;
                }

                directory = Path.GetDirectoryName(directory);
            }

            if (result == null)
            {
                throw new InvalidOperationException(string.Format("{0} not found.", FileName));
            }

            return Path.GetFullPath(result);
        }

        private static async Task CallAspNetHost(CancellationToken token)
        {
            var factory = ClientCallsFactory
                .ForAspNetHost(ChannelType.GrpcNet)
                .WithCompression(false);

            await RunDemoAsync(factory.CreateFileService(), "gRPC byte[] marshaller", token);

            await RunDemoAsync(factory.CreateFileServiceRentedArray(), "gRPC RentedArray marshaller", token);

            await RunDemoAsync(ClientCallsFactory.CreateHttpClient(false), "Http client", token);
        }

        private static async Task CallAspNetHostAsGrpcWeb(CancellationToken token)
        {
            var factory = ClientCallsFactory
                .ForAspNetHost(GrpcWebMode.GrpcWeb)
                .WithCompression(false);

            await RunDemoAsync(factory.CreateFileService(), "gRPC byte[] marshaller", token);

            await RunDemoAsync(factory.CreateFileServiceRentedArray(), "gRPC RentedArray marshaller", token);
        }

        private static async Task CallServerSelfHost(CancellationToken token)
        {
            var factory = ClientCallsFactory
                .ForSelfHost(ChannelType.GrpcCore)
                .WithCompression(false);

            await RunDemoAsync(factory.CreateFileService(), "gRPC byte[] marshaller", token);

            await RunDemoAsync(factory.CreateFileServiceRentedArray(), "gRPC RentedArray marshaller", token);
        }

        private static async Task RunDemoAsync(IClientCalls calls, string demoName, CancellationToken token)
        {
            var filePath = FindDemoFile();

            await using (calls)
            {
                Console.WriteLine("----- {0} UploadFile -----", demoName);
                await calls.UploadFileAsync(filePath, StreamExtensions.StreamDefaultCopyBufferSize, token);

                Console.WriteLine("----- {0} DownloadFile -----", demoName);
                await calls.DownloadFileAsync(filePath, StreamExtensions.StreamDefaultCopyBufferSize, token);
            }
        }
    }
}
