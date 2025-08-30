using PolyType;

namespace Contract;

/// <summary>
/// An object to transfer <see cref="InvalidRectangleException"/> details from Server to Client.
/// </summary>
[GenerateShape]
public sealed partial record InvalidRectangleError
{
    public InvalidRectangleError()
        : this(string.Empty, [])
    {
    }

    public InvalidRectangleError(string message, Point[] points)
    {
        Message = message;
        Points = points;
    }

    [PropertyShape(Order = 1)]
    public string Message { get; set; }

    [PropertyShape(Order = 2)]
    public Point[] Points { get; set; }
}