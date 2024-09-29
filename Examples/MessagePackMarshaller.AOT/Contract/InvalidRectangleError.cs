using MessagePack;

namespace Contract;

/// <summary>
/// An object to transfer <see cref="InvalidRectangleException"/> details from Server to Client.
/// </summary>
[MessagePackObject]
public sealed class InvalidRectangleError
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

    [Key(1)]
    public string Message { get; set; }

    [Key(2)]
    public Point[] Points { get; set; }
}