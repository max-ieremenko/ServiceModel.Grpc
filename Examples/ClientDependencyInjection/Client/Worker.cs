using System.Threading.Tasks;
using Contract;
using Microsoft.Extensions.Logging;

namespace Client;

internal sealed class Worker
{
    private readonly IRandomNumberGenerator _generator;
    private readonly ICalculator _calculator;

    public Worker(IRandomNumberGenerator generator, ICalculator calculator, ILoggerFactory loggerFactory)
    {
        _generator = generator;
        _calculator = calculator;
        Logger = loggerFactory.CreateLogger(nameof(Worker));
    }

    public ILogger Logger { get; }

    public async Task Run()
    {
        var x = await _generator.NextInt32();
        var y = await _generator.NextInt32();
        var sum = await _calculator.Sum(x, y);

        Logger.LogInformation($"{x} + {y} = {sum}");
    }
}