using System.Runtime.Serialization;

namespace Contract;

/// <summary>
/// Represents the shape of a rectangle.
/// </summary>
[DataContract]
public class Rectangle : FigureBase
{
    /// <summary>
    /// The left top vertex.
    /// </summary>
    [DataMember]
    public Point VertexLeftTop { get; set; } = new Point(0, 0);

    /// <summary>
    /// The width of the shape.
    /// </summary>
    [DataMember]
    public int Width { get; set; }

    /// <summary>
    /// The height of the shape.
    /// </summary>
    [DataMember]
    public int Height { get; set; }

    public override double GetArea()
    {
        return Width * Height;
    }
}