using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    Task<Rectangle> CreateRectangleAsync(Point leftTop, Point rightTop, Point rightBottom, Point leftBottom, CancellationToken cancellationToken = default);

    [OperationContract]
    Task<Number> GetAreaAsync(Rectangle rectangle, CancellationToken cancellationToken = default);

    [OperationContract]
    Task<Point[]> GetVerticesAsync(Rectangle rectangle, CancellationToken cancellationToken = default);
}