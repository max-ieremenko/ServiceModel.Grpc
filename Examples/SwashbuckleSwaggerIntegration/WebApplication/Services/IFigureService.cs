using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication.Services
{
    [ServiceContract]
    public interface IFigureService
    {
        [OperationContract]
        public Task<Triangle> CreateTriangle(Point vertex1, Point vertex2, Point vertex3, CancellationToken token);

        [OperationContract]
        public Rectangle CreateRectangle(Point vertexLeftTop, int width, int height);

        [OperationContract]
        public double CalculateArea(FigureBase figure);

        [OperationContract]
        public Task<FigureBase> FindSmallestFigure(IAsyncEnumerable<FigureBase> figures, CancellationToken token);

        [OperationContract]
        public IAsyncEnumerable<FigureBase> CreateRandomFigures(int count, CancellationToken token);

        [OperationContract]
        public IAsyncEnumerable<double> CalculateAreas(IAsyncEnumerable<FigureBase> figures, CancellationToken token);
    }
}
