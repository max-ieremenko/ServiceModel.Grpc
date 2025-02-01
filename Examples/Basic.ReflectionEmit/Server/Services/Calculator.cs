using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Server.Services;

internal sealed class Calculator : ICalculator
{
    public Number Sum(int x, int y, int z, CancellationToken cancellationToken) => new(x + y + z);

    public Task<Number> SumAsync(int x, int y, CancellationToken cancellationToken) => Task.FromResult(new Number(x + y));

    public async Task<int?> Max(IAsyncEnumerable<Number> numbers, CancellationToken cancellationToken)
    {
        int? result = null;

        await foreach (var number in numbers.WithCancellation(cancellationToken))
        {
            if (result.HasValue)
            {
                result = Math.Max(number.Value, result.Value);
            }
            else
            {
                result = number.Value;
            }
        }

        return result;
    }

    public async Task<int?> FirstGreaterThan(IAsyncEnumerable<int> numbers, int value, CancellationToken cancellationToken)
    {
        await foreach (var number in numbers.WithCancellation(cancellationToken))
        {
            if (number > value)
            {
                return number;
            }
        }

        return null;
    }

    public async IAsyncEnumerable<Number> GenerateRandom(int count, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var generator = new Random();
        for (var i = 0; i < count; i++)
        {
            await Task.Delay(1, cancellationToken);

            var number = generator.Next();
            yield return new Number(number);
        }
    }

    public async Task<(int MinValue, int MaxValue, IAsyncEnumerable<Number> Numbers)> GenerateRandomWithinRange(int minValue, int maxValue, int count, CancellationToken cancellationToken)
    {
        static async IAsyncEnumerable<Number> Generate(int minValue, int maxValue, int count, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var generator = new Random();
            for (var i = 0; i < count; i++)
            {
                await Task.Delay(1, cancellationToken);

                var number = generator.Next(minValue, maxValue);
                yield return new Number(number);
            }
        }

        await Task.Delay(1, cancellationToken);

        var numbers = Generate(minValue, maxValue, count, cancellationToken);
        return (minValue, maxValue, numbers);
    }

    public async IAsyncEnumerable<Number> MultiplyBy2(IAsyncEnumerable<int> numbers, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var number in numbers.WithCancellation(cancellationToken))
        {
            yield return new Number(number * 2);
        }
    }

    public async Task<(int Multiplier, IAsyncEnumerable<Number> Numbers)> MultiplyBy(IAsyncEnumerable<int> numbers, Number multiplier, CancellationToken cancellationToken)
    {
        static async IAsyncEnumerable<Number> Multiply(IAsyncEnumerable<int> numbers, Number multiplier, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var number in numbers.WithCancellation(cancellationToken))
            {
                yield return new Number(number * multiplier.Value);
            }
        }

        await Task.Delay(1, cancellationToken);

        var values = Multiply(numbers, multiplier, cancellationToken);
        return (multiplier.Value, values);
    }
}