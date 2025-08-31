using PolyType;

namespace Contract;

[GenerateShape]
public sealed partial record Rectangle
{
    public Rectangle()
        : this((0, 0), 0, 0)
    {
    }

    public Rectangle(Point leftTop, Number width, Number height)
    {
        LeftTop = leftTop;
        Width = width;
        Height = height;
    }

    [PropertyShape(Order = 1)]
    public Point LeftTop { get; set; }

    [PropertyShape(Order = 2)]
    public Number Width { get; set; }

    [PropertyShape(Order = 3)]
    public Number Height { get; set; }
}