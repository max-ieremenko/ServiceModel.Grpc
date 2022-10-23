using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Swashbuckle.AspNetCore.Annotations;

namespace SwashbuckleWebApplication.Services;

internal sealed class FigureService : IFigureService
{
    [SwaggerOperation("Create a triangle")]
    public Task<Triangle> CreateTriangle(Point vertex1, Point vertex2, Point vertex3, CancellationToken token)
    {
        return Task.FromResult(new Triangle
        {
            Vertex1 = vertex1,
            Vertex2 = vertex2,
            Vertex3 = vertex3
        });
    }

    public Rectangle CreateRectangle(Point vertexLeftTop, int width, int height)
    {
        return new Rectangle
        {
            VertexLeftTop = vertexLeftTop,
            Width = width,
            Height = height
        };
    }

    // this method demonstrates gRPC exception handling, see services.AddGrpc(options => options.EnableDetailedErrors = true);
    public Task<Point> CreatePoint(int x, int y)
    {
        throw new NotSupportedException("Point is not a 2d figure.");
    }

    public double CalculateArea(FigureBase figure)
    {
        return figure.GetArea();
    }

    // call to this method from Swagger UI is not supported
    public async Task<FigureBase> FindSmallestFigure(IAsyncEnumerable<FigureBase> figures, CancellationToken token)
    {
        FigureBase result = null;
        var resultArea = .0;

        await foreach (var figure in figures.WithCancellation(token))
        {
            var area = CalculateArea(figure);
            if (result == null || area > resultArea)
            {
                result = figure;
            }
        }

        return result;
    }

    // call to this method from Swagger UI is not supported
    public async IAsyncEnumerable<FigureBase> CreateRandomFigures(int count, [EnumeratorCancellation] CancellationToken token)
    {
        for (var i = 0; i < count; i++)
        {
            if ((i % 2 == 0))
            {
                var triangle = await CreateTriangle(new Point(0, 0), new Point(5, 5), new Point(5, 0), token);
                yield return triangle;
            }
            else
            {
                yield return CreateRectangle(new Point(0, 0), 100, 100);
            }
        }
    }

    // call to this method from Swagger UI is not supported
    public async IAsyncEnumerable<double> CalculateAreas(IAsyncEnumerable<FigureBase> figures, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var figure in figures.WithCancellation(token))
        {
            yield return CalculateArea(figure);
        }
    }
}