using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Server.Services;

internal sealed class Calculator : ICalculator
{
    public Task<Rectangle> CreateRectangleAsync(Point leftTop, Point rightTop, Point rightBottom, Point leftBottom, CancellationToken cancellationToken)
    {
        if (leftTop.Y.Value != rightTop.Y.Value
            || rightTop.X.Value != rightBottom.X.Value
            || rightBottom.Y.Value != leftBottom.Y.Value
            || leftBottom.X.Value != leftTop.X.Value)
        {
            throw new InvalidRectangleException(
                $"The figure '{leftTop} => {rightTop} => {rightBottom} => {leftBottom}' is invalid rectangle",
                [leftTop, rightTop, rightBottom, leftBottom]);
        }

        return Task.FromResult(new Rectangle(leftTop, Length(leftTop, rightTop), Length(leftTop, leftBottom)));
    }

    public Task<Number> GetAreaAsync(Rectangle rectangle, CancellationToken cancellationToken)
    {
        Number result = rectangle.Width.Value * rectangle.Height.Value;
        return Task.FromResult(result);
    }

    public async Task<Point[]> ShiftAsync(IAsyncEnumerable<Point> points, Number deltaX, Number deltaY, CancellationToken cancellationToken)
    {
        var result = new List<Point>();
        await foreach (var point in points.WithCancellation(cancellationToken))
        {
            result.Add((point.X.Value + deltaX.Value, point.Y.Value + deltaY.Value));
        }

        return result.ToArray();
    }

    public Task<(IAsyncEnumerable<Point> Points, Number Count)> GetVerticesAsync(Rectangle rectangle, CancellationToken cancellationToken)
    {
        var points = ExpandAsync(rectangle, cancellationToken);
        return Task.FromResult((points, (Number)4));
    }

    public async IAsyncEnumerable<Number> GetNumbersAsync(IAsyncEnumerable<Point> points, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var point in points.WithCancellation(cancellationToken))
        {
            yield return point.X;
            yield return point.Y;
        }
    }

    private static async IAsyncEnumerable<Point> ExpandAsync(Rectangle rectangle, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);
        yield return rectangle.LeftTop;
        yield return (rectangle.LeftTop.X.Value + rectangle.Width.Value, rectangle.LeftTop.Y); // rightTop
        yield return (rectangle.LeftTop.X.Value + rectangle.Width.Value, rectangle.LeftTop.Y.Value - rectangle.Height.Value); // rightBottom
        yield return (rectangle.LeftTop.X.Value, rectangle.LeftTop.Y.Value - rectangle.Height.Value); // leftBottom
    }

    private static int Length(Point a, Point b)
    {
        var x = a.X.Value - b.X.Value;
        var y = a.Y.Value - b.Y.Value;
        var result = Math.Sqrt(x * x + y * y);
        return (int)result;
    }
}