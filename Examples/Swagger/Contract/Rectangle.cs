using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class Rectangle : FigureBase
{
    [DataMember]
    public Point VertexLeftTop { get; set; } = new Point(0, 0);

    [DataMember]
    public int Width { get; set; }

    [DataMember]
    public int Height { get; set; }

    public override double GetArea()
    {
        return Width * Height;
    }
}