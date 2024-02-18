using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface IFigureService
{
    [OperationContract]
    Task<Triangle> CreateTriangle(Point vertex1, Point vertex2, Point vertex3, CancellationToken token = default);

    [OperationContract]
    Rectangle CreateRectangle(Point vertexLeftTop, int width, int height);

    [OperationContract]
    Task<Point> CreatePoint(int x, int y);

    [OperationContract]
    double CalculateArea(FigureBase figure);

    [OperationContract]
    Task<FigureBase?> FindSmallestFigure(IAsyncEnumerable<FigureBase> figures, CancellationToken token = default);

    [OperationContract]
    IAsyncEnumerable<FigureBase> CreateRandomFigures(int count, CancellationToken token = default);

    [OperationContract]
    IAsyncEnumerable<double> CalculateAreas(IAsyncEnumerable<FigureBase> figures, CancellationToken token = default);
}