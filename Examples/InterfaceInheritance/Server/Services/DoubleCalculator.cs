using System;
using System.Threading.Tasks;
using Contract;

namespace Server.Services;

internal sealed class DoubleCalculator : IDoubleCalculator
{
    // POST /IDoubleCalculator/Touch
    public string Touch()
    {
        return nameof(DoubleCalculator);
    }

    // POST /IDoubleCalculator/Sum
    public Task<double> Sum(double x, double y)
    {
        return Task.FromResult(x + y);
    }

    // POST /IDoubleCalculator/Multiply
    public ValueTask<double> Multiply(double x, double y)
    {
        return new ValueTask<double>(x * y);
    }

    // POST /IDoubleCalculator/GetRandomValue
    public ValueTask<double> GetRandomValue()
    {
        var result = new Random(DateTime.Now.Millisecond).NextDouble();
        return new ValueTask<double>(result);
    }
}