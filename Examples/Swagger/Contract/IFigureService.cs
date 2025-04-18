using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface IFigureService
{
    /// <summary>
    /// Creates a triangle.
    /// </summary>
    /// <remarks>
    /// Returns a triangle with <paramref name="vertex1"/>, <paramref name="vertex2"/>, and <paramref name="vertex3"/>.
    /// </remarks>
    [OperationContract]
    Task<Triangle> CreateTriangle(Point vertex1, Point vertex2, Point vertex3, CancellationToken token = default);

    /// <summary>
    /// Creates a rectangle.
    /// </summary>
    /// <remarks>
    /// Returns a rectangle with the top left vertex, <paramref name="width"/>, and <paramref name="height"/>.
    /// </remarks>
    [OperationContract]
    Rectangle CreateRectangle(Point vertexLeftTop, int width, int height);

    /// <summary>
    /// Creates a point.
    /// </summary>
    /// <remarks>
    /// Returns a point with <paramref name="x"/> and <paramref name="y"/> coordinates.
    /// </remarks>
    [OperationContract]
    Task<Point> CreatePoint(int x, int y);

    /// <summary>
    /// Calculates the area of ​​a shape.
    /// </summary>
    /// <remarks>
    /// Returns the area of shape <paramref name="figure"/>.
    /// </remarks>
    [OperationContract]
    double CalculateArea(FigureBase figure);

    /// <summary>
    /// Searches for a shape with the smallest area.
    /// </summary>
    /// <remarks>
    /// Returns the shape with the smallest area from the list <paramref name="figures"/>.
    /// </remarks>
    [OperationContract]
    Task<FigureBase?> FindSmallestFigure(IAsyncEnumerable<FigureBase> figures, CancellationToken token = default);

    /// <summary>
    /// Generates random shapes.
    /// </summary>
    /// <remarks>
    /// Returns <paramref name="count"/> randomly generated shapes.
    /// </remarks>
    [OperationContract]
    IAsyncEnumerable<FigureBase> CreateRandomFigures(int count, CancellationToken token = default);

    /// <summary>
    /// Calculates the area of shapes.
    /// </summary>
    /// <remarks>
    /// Projects each shape of the sequence <paramref name="figures"/> into its area.
    /// </remarks>
    [OperationContract]
    IAsyncEnumerable<double> CalculateAreas(IAsyncEnumerable<FigureBase> figures, CancellationToken token = default);
}