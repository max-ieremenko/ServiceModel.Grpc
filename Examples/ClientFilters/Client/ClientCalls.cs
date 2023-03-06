using System;
using System.Threading;
using System.Threading.Tasks;
using Client.Filters;
using Contract;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceModel.Grpc.Client;
using Shouldly;

namespace Client;

public static class ClientCalls
{
    public static async Task CallCalculator(Uri calculatorLocation, CancellationToken token)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(options =>
            {
                options.AddConsole();
            })
            .AddTransient<LoggingClientFilter>()
            .BuildServiceProvider();

        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            ServiceProvider = serviceProvider,
            Filters =
            {
                // attach LoggingServerFilter globally
                { 1, provider => provider.GetRequiredService<LoggingClientFilter>() }
            }
        });

        clientFactory.AddClient<ICalculator>(options =>
        {
            // attach filters only for ICalculator client
            options.Filters.Add(100, new SumAsyncClientFilter());
            options.Filters.Add(100, new HackMultiplyClientFilter());
        });

        using (var channel = GrpcChannel.ForAddress(calculatorLocation))
        {
            var calculator = clientFactory.CreateClient<ICalculator>(channel);

            await CallSumAsync(calculator, token).ConfigureAwait(false);
            CallDivideBy(calculator);
            await CallMultiplyByAsync(calculator, token).ConfigureAwait(false);
        }
    }

    private static async Task CallSumAsync(ICalculator calculator, CancellationToken token)
    {
        var sum = await calculator.SumAsync(1, 2, token).ConfigureAwait(false);
        Console.WriteLine("client: SumAsync(1, 2) = {0}", sum);

        sum.ShouldBe(3);
    }

    private static void CallDivideBy(ICalculator calculator)
    {
        var result = calculator.DivideBy(4, 2);
        Console.WriteLine("client: DivideBy(4, 2) = {0}", result);

        result.ShouldBe(2);
    }

    private static async Task CallMultiplyByAsync(ICalculator calculator, CancellationToken token)
    {
        var inputMultiplier = 3;
        var inputValues = new[] { 1, 2 };

        var (outputStream, outputMultiplier) = await calculator
            .MultiplyByAsync(inputValues.AsAsyncEnumerable(token), inputMultiplier, token)
            .ConfigureAwait(false);

        var outputValues = await outputStream.ToListAsync(token).ConfigureAwait(false);

        Console.WriteLine("client: MultiplyByAsync([ 1, 2 ], 3) = [{0}]", string.Join(", ", outputValues));

        // see HackMultiplyByServerFilter
        outputMultiplier.ShouldBe(inputMultiplier + 2);
        outputValues.ShouldBe(new[]
        {
            (inputValues[0] + 1) * outputMultiplier + 1,
            (inputValues[1] + 1) * outputMultiplier + 1
        });
    }
}