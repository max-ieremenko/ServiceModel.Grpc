using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;
using Shouldly;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("---- gRPC client ----");
        using var channel = GrpcChannel.ForAddress("http://localhost:5001");
        var clientFactory = new ClientFactory();
        await RunUnaryAsync(clientFactory.CreateClient<ICalculator>(channel));
        await RunUnaryAsync(clientFactory.CreateClient<IFigureService>(channel));
        await RunStreamingAsync(clientFactory.CreateClient<IFigureService>(channel));

        Console.WriteLine("---- Swagger UI client ----");
        using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        await RunUnaryAsync(new CalculatorSwaggerClient(httpClient));
        await RunUnaryAsync(new FigureServiceSwaggerClient(httpClient));

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task RunUnaryAsync(ICalculator calculator)
    {
        var randomNumber = await calculator.GetRandomNumber();
        Console.WriteLine($"Random number: {randomNumber}");

        var sum = await calculator.Sum(100, 10, 1);
        Console.WriteLine($"100 + 10 + 1 = {sum}");
        sum.ShouldBe(111);
    }

    private static async Task RunUnaryAsync(IFigureService figureService)
    {
        var triangle = await figureService.CreateTriangle(new Point(0, 0), new Point(2, 2), new Point(2, 0));
        Console.WriteLine($"Triangle: {triangle.Vertex1} {triangle.Vertex2} {triangle.Vertex3}");
        triangle.Vertex1.ShouldBe(new Point(0, 0));
        triangle.Vertex2.ShouldBe(new Point(2, 2));
        triangle.Vertex3.ShouldBe(new Point(2, 0));

        var rectangle = figureService.CreateRectangle(new Point(0, 0), 10, 20);
        Console.WriteLine($"Rectangle: {rectangle.VertexLeftTop} {rectangle.Height}x{rectangle.Width}");
        rectangle.VertexLeftTop.ShouldBe(new Point(0, 0));
        rectangle.Width.ShouldBe(10);
        rectangle.Height.ShouldBe(20);

        var rectangleArea = figureService.CalculateArea(rectangle);
        Console.WriteLine($"Rectangle area: {rectangleArea}");
        rectangleArea.ShouldBe(200);

        var triangleArea = figureService.CalculateArea(triangle);
        Console.WriteLine($"Triangle area: {triangleArea}", triangleArea);
        triangleArea.ShouldBe(1.999, .001);

        // NotSupportedException: Point is not a 2d figure.
        var ex = await Should.ThrowAsync<RpcException>(figureService.CreatePoint(1, 1));
        ex.StatusCode.ShouldBe(StatusCode.Unknown);
    }

    private static async Task RunStreamingAsync(IFigureService figureService)
    {
        var randomFigures = figureService.CreateRandomFigures(3);
        await foreach (var figure in randomFigures)
        {
            Console.WriteLine($"Created random {figure.GetType().Name}");
        }

        var rectangle = new Rectangle
        {
            VertexLeftTop = new Point(0, 0),
            Width = 10,
            Height = 20
        };
        var triangle = new Triangle
        {
            Vertex1 = new Point(0, 0),
            Vertex2 = new Point(2, 2),
            Vertex3 = new Point(2, 0)
        };

        var smallest = await figureService.FindSmallestFigure(AsAsyncEnumerable(rectangle, triangle));
        Console.WriteLine($"Smallest one is {smallest?.GetType().Name}");
        smallest.ShouldBeOfType<Triangle>();

        var areas = figureService.CalculateAreas(AsAsyncEnumerable(rectangle, triangle));
        await foreach (var area in areas)
        {
            Console.WriteLine($"Area is {area}");
        }
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