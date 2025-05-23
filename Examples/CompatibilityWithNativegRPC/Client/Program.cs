﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;

namespace Client;

public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        // set ProtobufMarshaller as default Marshaller
        MarshallerFactory = ProtobufMarshallerFactory.Default
    });

    public static async Task Main(string[] args)
    {
        var channelCodeFirst = GrpcChannel.ForAddress("http://localhost:5000");
        var channelProto = GrpcChannel.ForAddress("http://localhost:5001");

        Console.WriteLine("Proto->Proto");
        await ProtoCall(channelProto);

        Console.WriteLine();
        Console.WriteLine("CodeFirst->CodeFirst");
        await CodeFirstCall(channelCodeFirst);

        Console.WriteLine();
        Console.WriteLine("Proto->CodeFirst");
        await ProtoCall(channelCodeFirst);

        Console.WriteLine();
        Console.WriteLine("CodeFirst->Proto");
        await CodeFirstCall(channelProto);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task ProtoCall(ChannelBase channel)
    {
        var client = new CalculatorNative.CalculatorNativeClient(channel);

        // SumAsync
        var sumResult = await client.SumAsync(new SumRequest { X = 1, Y = 2, Z = 3 });
        Console.WriteLine("   SumAsync(1 + 2 + 3) = {0}", sumResult.Result);

        // SumValues
        using (var call = client.SumValues())
        {
            foreach (var i in new[] { 1, 2, 3 })
            {
                await call.RequestStream.WriteAsync(new Int32Value { Value = i });
            }

            await call.RequestStream.CompleteAsync();

            var sumValuesResult = await call.ResponseAsync;
            Console.WriteLine("   SumValuesAsync(1 + 2 + 3) = {0}", sumValuesResult.Result);
        }

        // Range
        using (var call = client.Range(new RangeRequest { Start = 0, Count = 3 }))
        {
            var rangeResult = new List<int>();
            while (await call.ResponseStream.MoveNext())
            {
                rangeResult.Add(call.ResponseStream.Current.Value);
            }

            Console.WriteLine("   Range(0, 3) = {0}", string.Join(", ", rangeResult));
        }

        // MultiplyBy2
        using (var call = client.MultiplyBy2())
        {
            foreach (var i in new[] { 1, 2, 3 })
            {
                await call.RequestStream.WriteAsync(new Int32Value { Value = i });
            }

            await call.RequestStream.CompleteAsync();

            var multiplyBy2Result = new List<int>();
            while (await call.ResponseStream.MoveNext())
            {
                multiplyBy2Result.Add(call.ResponseStream.Current.Value);
            }

            Console.WriteLine("   MultiplyBy2(1, 2, 3) = ({0})", string.Join(", ", multiplyBy2Result));
        }
    }

    private static async Task CodeFirstCall(ChannelBase channel)
    {
        var client = DefaultClientFactory.CreateClient<ICalculator>(channel);

        // SumAsync
        var sumResult = await client.SumAsync(1, 2, 3);
        Console.WriteLine("   SumAsync(1 + 2 + 3) = {0}", sumResult);

        // SumValues
        var sumValuesResult = await client.SumValuesAsync(new[] { 1, 2, 3 }.AsAsyncEnumerable());
        Console.WriteLine("   SumValuesAsync(1 + 2 + 3) = {0}", sumValuesResult);

        // Range
        var rangeResult = new List<int>();
        await foreach (var i in client.Range(0, 3))
        {
            rangeResult.Add(i);
        }

        Console.WriteLine("   Range(0, 3) = {0}", string.Join(", ", rangeResult));

        // MultiplyBy2
        var multiplyBy2Result = new List<int>();
        await foreach (var i in client.MultiplyBy2(new[] { 1, 2, 3 }.AsAsyncEnumerable()))
        {
            multiplyBy2Result.Add(i);
        }

        Console.WriteLine("   MultiplyBy2(1, 2, 3) = ({0})", string.Join(", ", multiplyBy2Result));
    }
}