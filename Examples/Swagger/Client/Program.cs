using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            ServiceModel.Grpc.GrpcChannelExtensions.Http2UnencryptedSupport = true;
            var channel = new Channel("localhost", 5001, ChannelCredentials.Insecure);

            var clientFactory = new ClientFactory();
            var figureServiceProxy = clientFactory.CreateClient<IFigureService>(channel);

            var rectangle = figureServiceProxy.CreateRectangle(new Point(0, 0), 10, 20);
            var triangle = await figureServiceProxy.CreateTriangle(new Point(0, 0), new Point(2, 2), new Point(2, 0));

            // NotSupportedException: Point is not a 2d figure.
            try
            {
                await figureServiceProxy.CreatePoint(1, 1);
            }
            catch (RpcException ex) when(ex.StatusCode == StatusCode.Unknown)
            {
                Console.WriteLine(ex.Status.Detail);
            }

            Console.WriteLine("Rectangle area: {0}", figureServiceProxy.CalculateArea(rectangle));
            Console.WriteLine("Triangle area: {0}", figureServiceProxy.CalculateArea(triangle));

            var randomFigures = figureServiceProxy.CreateRandomFigures(3, default);
            await foreach (var figure in randomFigures)
            {
                Console.WriteLine("Created random {0}", figure.GetType().Name);
            }

            var smallest = await figureServiceProxy.FindSmallestFigure(AsAsyncEnumerable(rectangle, triangle));
            Console.WriteLine("Smallest one is {0}", smallest.GetType().Name);

            var areas = figureServiceProxy.CalculateAreas(AsAsyncEnumerable(rectangle, triangle));
            await foreach (var area in areas)
            {
                Console.WriteLine("Area is {0}", area);
            }

            Console.WriteLine("...");
            Console.ReadLine();
        }

        private static async IAsyncEnumerable<FigureBase> AsAsyncEnumerable(params FigureBase[] figures)
        {
            for (var i = 0; i < figures.Length; i++)
            {
                await Task.Yield();
                yield return figures[i];
            }
        }
    }
}
