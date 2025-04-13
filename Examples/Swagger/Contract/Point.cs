using System;
using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class Point : IEquatable<Point>
{
    public Point()
    {
    }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    [DataMember]
    public double X { get; set; }

    [DataMember]
    public double Y { get; set; }

    public bool Equals(Point? other) => other != null && Math.Abs(other.X - X) < double.Epsilon && Math.Abs(other.Y - Y) <= double.Epsilon;

    public override bool Equals(object? obj) => Equals(obj as Point);

    public override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());

    public override string ToString() => $"{X};{Y}";
}