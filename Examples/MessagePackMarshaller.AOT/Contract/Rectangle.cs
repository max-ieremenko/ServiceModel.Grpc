using MessagePack;

namespace Contract;

[MessagePackObject]
public sealed class Rectangle
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

    [Key(1)]
    public Point LeftTop { get; set; }

    [Key(2)]
    public Number Width { get; set; }

    [Key(3)]
    public Number Height { get; set; }

    public override string ToString() => $"{LeftTop}; Width: {Width}: Height: {Height}";
}