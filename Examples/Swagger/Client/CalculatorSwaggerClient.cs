using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Client;

internal sealed class CalculatorSwaggerClient : ICalculator
{
    private readonly HttpClient _httpClient;

    public CalculatorSwaggerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<int> GetRandomNumber() => _httpClient.PostAsync<int>("ICalculator/GetRandomNumber", null);

    public Task<long> Sum(long x, int y, int z, CancellationToken token)
    {
        var request = new { x, y, z };
        return _httpClient.PostAsync<long>("ICalculator/Sum", request, token);
    }

    public ValueTask<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token)
    {
        throw new NotSupportedException("Streaming is not supported.");
    }
}