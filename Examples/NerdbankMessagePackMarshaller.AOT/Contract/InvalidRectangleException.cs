using System;

namespace Contract;

/// <summary>
/// Client-Server shared exception example, see also <see cref="InvalidRectangleError"/>
/// </summary>
public sealed class InvalidRectangleException : ApplicationException
{
    public InvalidRectangleException(string message, Point[] points)
        : base(message)
    {
        Points = points;
    }

    public InvalidRectangleException(string message, Exception innerException, Point[] points)
        : base(message, innerException)
    {
        Points = points;
    }

    public Point[] Points { get; }
}