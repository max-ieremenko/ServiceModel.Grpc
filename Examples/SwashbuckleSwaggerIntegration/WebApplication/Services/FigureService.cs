using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication.Services
{
    internal sealed class FigureService : IFigureService
    {
        private static int _figureIndex;

        public Task<Triangle> CreateTriangle(Point vertex1, Point vertex2, Point vertex3, CancellationToken token)
        {
            return Task.FromResult(new Triangle
            {
                Name = GetFigureUniqueName(nameof(Triangle)),
                Vertex1 = vertex1,
                Vertex2 = vertex2,
                Vertex3 = vertex3
            });
        }

        public Rectangle CreateRectangle(Point vertexLeftTop, int width, int height)
        {
            return new Rectangle
            {
                Name = GetFigureUniqueName(nameof(Triangle)),
                VertexLeftTop = vertexLeftTop,
                Width = width,
                Height = height
            };
        }

        public double CalculateArea(FigureBase figure)
        {
            if (figure is Triangle triangle)
            {
                return triangle.Area();
            }

            if (figure is Rectangle rectangle)
            {
                return rectangle.Area();
            }

            throw new NotImplementedException();
        }

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

        public async IAsyncEnumerable<double> CalculateAreas(IAsyncEnumerable<FigureBase> figures, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var figure in figures.WithCancellation(token))
            {
                yield return CalculateArea(figure);
            }
        }

        private static string GetFigureUniqueName(string name)
        {
            var index = Interlocked.Increment(ref _figureIndex);
            return name + index.ToString(CultureInfo.InvariantCulture);
        }
    }
}
