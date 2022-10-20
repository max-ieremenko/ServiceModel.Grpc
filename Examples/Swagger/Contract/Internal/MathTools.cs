using System;

namespace Contract.Internal;

internal static class MathTools
{
    public static double Distance(this Point vertex1, Point vertex2)
    {
        var l1 = (vertex1.X - vertex2.X) * (vertex1.X - vertex2.X);
        var l2 = (vertex1.Y - vertex2.Y) * (vertex1.Y - vertex2.Y);
        return Math.Sqrt(l1 + l2);
    }
}