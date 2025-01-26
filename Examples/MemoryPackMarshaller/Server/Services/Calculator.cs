using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Server.Services;

internal sealed class Calculator : ICalculator
{
    public Task<Rectangle> CreateRectangleAsync(Point leftTop, Point rightTop, Point rightBottom, Point leftBottom, CancellationToken cancellationToken) =>
        Task.FromResult(new Rectangle(leftTop, Length(leftTop, rightTop), Length(leftTop, leftBottom)));

    public Task<Number> GetAreaAsync(Rectangle rectangle, CancellationToken cancellationToken)
    {
        Number result = rectangle.Width.Value * rectangle.Height.Value;
        return Task.FromResult(result);
    }

    public Task<Point[]> GetVerticesAsync(Rectangle rectangle, CancellationToken cancellationToken)
    {
        Point[] points =
        [
            rectangle.LeftTop,
            (rectangle.LeftTop.X.Value + rectangle.Width.Value, rectangle.LeftTop.Y), // rightTop
            (rectangle.LeftTop.X.Value + rectangle.Width.Value, rectangle.LeftTop.Y.Value - rectangle.Height.Value), // rightBottom
            (rectangle.LeftTop.X.Value, rectangle.LeftTop.Y.Value - rectangle.Height.Value), // leftBottom
        ];
        return Task.FromResult(points);
    }

    private static int Length(Point a, Point b)
    {
        var x = a.X.Value - b.X.Value;
        var y = a.Y.Value - b.Y.Value;
        var result = Math.Sqrt(x * x + y * y);
        return (int)result;
    }
}