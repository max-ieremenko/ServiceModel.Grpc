using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;
using Shouldly;

namespace Client;

public static class ClientCalls
{
    public static async Task CallCalculator(Uri calculatorLocation, CancellationToken token)
    {
        var clientFactory = new ClientFactory();
        using (var channel = GrpcChannel.ForAddress(calculatorLocation))
        {
            var calculator = clientFactory.CreateClient<ICalculator>(channel);

            await CallSumAsync(calculator, token).ConfigureAwait(false);
            await CallDivideByAsync(calculator, token).ConfigureAwait(false);
            await CallMultiplyByAsync(calculator, token).ConfigureAwait(false);
        }
    }

    private static async Task CallSumAsync(ICalculator calculator, CancellationToken token)
    {
        var sum = await calculator.SumAsync(1, 2, token).ConfigureAwait(false);
        Console.WriteLine("client: SumAsync(1, 2) = {0}", sum);

        sum.ShouldBe(3);
    }

    private static async Task CallDivideByAsync(ICalculator calculator, CancellationToken token)
    {
        var result = await calculator.DivideByAsync(1, 0, token).ConfigureAwait(false);
        Console.WriteLine("client: DivideByAsync(1, 0) = {0}", result);
        result.IsSuccess.ShouldBeFalse();

        result = await calculator.DivideByAsync(1, 10, token).ConfigureAwait(false);
        Console.WriteLine("client: DivideByAsync(1, 10) = {0}", result);
        result.IsSuccess.ShouldBeFalse();

        result = await calculator.DivideByAsync(30, 15, token).ConfigureAwait(false);
        Console.WriteLine("client: DivideByAsync(1, 0) = {0}", result);
        result.IsSuccess.ShouldBeTrue();
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