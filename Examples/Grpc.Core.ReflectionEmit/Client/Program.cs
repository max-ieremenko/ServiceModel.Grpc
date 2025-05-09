﻿using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Client;

public static class Program
{
    private const int Port = 8082;

    public static async Task Main()
    {
        IClientFactory clientFactory = new ClientFactory();

        // optionally, configure client, see https://max-ieremenko.github.io/ServiceModel.Grpc/ClientConfiguration.html
        //clientFactory.AddClient<ICalculator>(options =>
        //{
        //    // the DataContractMarshallerFactory is default one
        //    //options.MarshallerFactory = DataContractMarshallerFactory.Default;

        //    // optionally, add client filters, see https://max-ieremenko.github.io/ServiceModel.Grpc/client-filters.html
        //    //options.Filters.Add(...);

        //    // configure error handling, see https://max-ieremenko.github.io/ServiceModel.Grpc/error-handling-general.html
        //    //options.ErrorHandler = ...
        //});

        var channel = new Channel("localhost", Port, ChannelCredentials.Insecure);

        // a proxy will be generated at runtime by ServiceModel.Grpc
        var calculator = clientFactory.CreateClient<ICalculator>(channel);

        Console.WriteLine($"Invoke {nameof(Sum)}");
        Sum(calculator);

        Console.WriteLine($"Invoke {nameof(SumAsync)}");
        await SumAsync(calculator);

        Console.WriteLine($"Invoke {nameof(Max)}");
        await Max(calculator);

        Console.WriteLine($"Invoke {nameof(FirstGreaterThan)}");
        await FirstGreaterThan(calculator);

        Console.WriteLine($"Invoke {nameof(GenerateRandom)}");
        await GenerateRandom(calculator);

        Console.WriteLine($"Invoke {nameof(GenerateRandomWithinRange)}");
        await GenerateRandomWithinRange(calculator);

        Console.WriteLine($"Invoke {nameof(MultiplyBy2)}");
        await MultiplyBy2(calculator);

        Console.WriteLine($"Invoke {nameof(MultiplyBy)}");
        await MultiplyBy(calculator);
    }

    private static void Sum(ICalculator calculator)
    {
        calculator.Sum(1, 2, 3).ShouldBe(new Number(6));
    }

    private static async Task SumAsync(ICalculator calculator)
    {
        var result = await calculator.SumAsync(1, 2);
        result.ShouldBe(new Number(3));
    }

    private static async Task Max(ICalculator calculator)
    {
        var numbers = Enumerable.Range(1, 5).Select(i => new Number(i)).AsAsyncEnumerable();
        var result = await calculator.Max(numbers);
        result.ShouldBe(5);
    }

    private static async Task FirstGreaterThan(ICalculator calculator)
    {
        var numbers = Enumerable.Range(1, 5).AsAsyncEnumerable();
        var result = await calculator.FirstGreaterThan(numbers, 3);
        result.ShouldBe(4);
    }

    private static async Task GenerateRandom(ICalculator calculator)
    {
        var result = await calculator.GenerateRandom(3).ToListAsync();
        result.Count.ShouldBe(3);
    }

    private static async Task GenerateRandomWithinRange(ICalculator calculator)
    {
        var response = await calculator.GenerateRandomWithinRange(3, 10, 5);

        response.MinValue.ShouldBe(3);
        response.MaxValue.ShouldBe(10);

        var numbers = await response.Numbers.ToListAsync();

        numbers.Count.ShouldBe(5);
        foreach (var number in numbers)
        {
            number.Value.ShouldBeGreaterThanOrEqualTo(3);
            number.Value.ShouldBeLessThan(10);
        }
    }

    private static async Task MultiplyBy2(ICalculator calculator)
    {
        var numbers = Enumerable.Range(1, 5).ToArray();
        var values = await calculator.MultiplyBy2(numbers.AsAsyncEnumerable()).ToListAsync();

        values.Count.ShouldBe(numbers.Length);
        for (var i = 0; i < values.Count; i++)
        {
            values[i].Value.ShouldBe(numbers[i] * 2);
        }
    }

    private static async Task MultiplyBy(ICalculator calculator)
    {
        var numbers = Enumerable.Range(1, 5).ToArray();
        var response = await calculator.MultiplyBy(numbers.AsAsyncEnumerable(), new Number(3));

        response.Multiplier.ShouldBe(3);

        var values = await response.Numbers.ToListAsync();

        values.Count.ShouldBe(numbers.Length);
        for (var i = 0; i < values.Count; i++)
        {
            values[i].Value.ShouldBe(numbers[i] * 3);
        }
    }
}