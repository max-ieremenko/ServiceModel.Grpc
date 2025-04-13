using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Client;

internal sealed class FigureServiceSwaggerClient : IFigureService
{
    private readonly HttpClient _httpClient;

    public FigureServiceSwaggerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<Triangle> CreateTriangle(Point vertex1, Point vertex2, Point vertex3, CancellationToken token)
    {
        var request = new { vertex1, vertex2, vertex3 };
        return _httpClient.PostAsync<Triangle>("IFigureService/CreateTriangle", request, token);
    }

    public Rectangle CreateRectangle(Point vertexLeftTop, int width, int height)
    {
        var request = new { vertexLeftTop, width, height };
        return _httpClient.PostAsync<Rectangle>("IFigureService/CreateRectangle", request).Result;
    }

    public Task<Point> CreatePoint(int x, int y)
    {
        var request = new { x, y };
        return _httpClient.PostAsync<Point>("IFigureService/CreatePoint", request);
    }

    public double CalculateArea(FigureBase figure)
    {
        var request = new { figure };
        return _httpClient.PostAsync<double>("IFigureService/CalculateArea", request).Result;
    }

    public Task<FigureBase?> FindSmallestFigure(IAsyncEnumerable<FigureBase> figures, CancellationToken token)
    {
        throw new NotSupportedException("Streaming is not supported.");
    }

    public IAsyncEnumerable<FigureBase> CreateRandomFigures(int count, CancellationToken token)
    {
        throw new NotSupportedException("Streaming is not supported.");
    }

    public IAsyncEnumerable<double> CalculateAreas(IAsyncEnumerable<FigureBase> figures, CancellationToken token)
    {
        throw new NotSupportedException("Streaming is not supported.");
    }
}