using System;
using System.Runtime.Serialization;
using Contract.Internal;

namespace Contract;

/// <summary>
/// Represents the shape of a triangle.
/// </summary>
[DataContract]
public class Triangle : FigureBase
{
    /// <summary>
    /// First vertex.
    /// </summary>
    [DataMember]
    public Point Vertex1 { get; set; } = new Point(0, 0);

    /// <summary>
    /// Second vertex.
    /// </summary>
    [DataMember]
    public Point Vertex2 { get; set; } = new Point(0, 0);

    /// <summary>
    /// Third vertex.
    /// </summary>
    [DataMember]
    public Point Vertex3 { get; set; } = new Point(0, 0);

    public override double GetArea()
    {
        var a = Vertex1.Distance(Vertex2);
        var b = Vertex2.Distance(Vertex3);
        var c = Vertex3.Distance(Vertex1);

        var p = (a + b + c) / 2;

        return Math.Sqrt(p * (p - a) * (p - b) * (p - c));

    }
}