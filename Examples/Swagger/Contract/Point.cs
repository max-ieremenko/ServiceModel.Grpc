using System;
using System.Runtime.Serialization;

namespace Contract;

/// <summary>
/// Represents a point.
/// </summary>
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

    /// <summary>
    /// Coordinate X.
    /// </summary>
    [DataMember]
    public double X { get; set; }

    /// <summary>
    /// Coordinate Y.
    /// </summary>
    [DataMember]
    public double Y { get; set; }

    public bool Equals(Point? other) => other != null && Math.Abs(other.X - X) < double.Epsilon && Math.Abs(other.Y - Y) <= double.Epsilon;

    public override bool Equals(object? obj) => Equals(obj as Point);

    public override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());

    public override string ToString() => $"{X};{Y}";
}