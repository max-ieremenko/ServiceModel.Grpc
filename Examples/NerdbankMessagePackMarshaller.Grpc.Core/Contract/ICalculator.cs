using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Threading;

namespace Contract;

[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    Task<Rectangle> CreateRectangleAsync(Point leftTop, Point rightTop, Point rightBottom, Point leftBottom, CancellationToken cancellationToken = default);

    [OperationContract]
    Task<Number> GetAreaAsync(Rectangle rectangle, CancellationToken cancellationToken = default);

    [OperationContract]
    Task<Point[]> ShiftAsync(IAsyncEnumerable<Point> points, Number deltaX, Number deltaY, CancellationToken cancellationToken = default);

    [OperationContract]
    Task<(IAsyncEnumerable<Point> Points, Number Count)> GetVerticesAsync(Rectangle rectangle, CancellationToken cancellationToken = default);

    [OperationContract]
    IAsyncEnumerable<Number> GetNumbersAsync(IAsyncEnumerable<Point> points, CancellationToken cancellationToken = default);
}