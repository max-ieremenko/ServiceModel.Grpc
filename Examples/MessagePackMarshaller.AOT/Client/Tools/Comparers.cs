using System;
using System.Collections.Generic;
using Contract;

namespace Client.Tools;

internal sealed class NumberComparer : IEqualityComparer<Number>
{
    public static readonly NumberComparer Default = new();

    public bool Equals(Number x, Number y) => x.Value == y.Value;

    public int GetHashCode(Number obj) => obj.Value;
}

internal sealed class PointComparer : IEqualityComparer<Point>
{
    public static readonly PointComparer Default = new();

    public bool Equals(Point? x, Point? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return NumberComparer.Default.Equals(x.X, y.X) && NumberComparer.Default.Equals(x.Y, y.Y);
    }

    public int GetHashCode(Point obj) => HashCode.Combine(NumberComparer.Default.GetHashCode(obj.X), NumberComparer.Default.GetHashCode(obj.Y));
}

internal sealed class RectangleComparer : IEqualityComparer<Rectangle>
{
    public static readonly RectangleComparer Default = new();

    public bool Equals(Rectangle? x, Rectangle? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return PointComparer.Default.Equals(x.LeftTop, y.LeftTop)
               && NumberComparer.Default.Equals(x.Width, y.Width)
               && NumberComparer.Default.Equals(x.Height, y.Height);
    }

    public int GetHashCode(Rectangle obj) => HashCode.Combine(
        PointComparer.Default.GetHashCode(obj.LeftTop),
        NumberComparer.Default.GetHashCode(obj.Width),
        NumberComparer.Default.GetHashCode(obj.Height));
}