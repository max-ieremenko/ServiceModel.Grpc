using System;

namespace WebApplication.Services
{
    internal static class MathTools
    {
        public static double Distance(this Point vertex1, Point vertex2)
        {
            var l1 = (vertex1.X - vertex2.X) * (vertex1.X - vertex2.X);
            var l2 = (vertex1.Y - vertex2.Y) * (vertex1.Y - vertex2.Y);
            return Math.Sqrt(l1 + l2);
        }

        public static double Area(this Triangle triangle)
        {
            var a = triangle.Vertex1.Distance(triangle.Vertex2);
            var b = triangle.Vertex2.Distance(triangle.Vertex3);
            var c = triangle.Vertex3.Distance(triangle.Vertex1);

            var p = (a + b + c) / 2;

            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        public static double Area(this Rectangle rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }
    }
}
