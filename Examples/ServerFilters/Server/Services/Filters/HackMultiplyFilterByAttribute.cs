using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ServiceModel.Grpc.Filters;

namespace Server.Services.Filters;

internal sealed class HackMultiplyFilterByAttribute : ServerFilterAttribute
{
    public HackMultiplyFilterByAttribute(int order)
        : base(order)
    {
    }

    public override async ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        var inputMultiplier = (int)context.Request["multiplier"]!;
        var inputValues = (IAsyncEnumerable<int>)context.Request.Stream!;

        // increase multiplier by 2
        context.Request["multiplier"] = inputMultiplier + 2;

        // increase each input value by 1
        context.Request.Stream = IncreaseValuesBy1(inputValues, context.ServerCallContext.CancellationToken);

        await next().ConfigureAwait(false);

        var outputValues = (IAsyncEnumerable<int>)context.Response.Stream!;

        // increase each output value by 1
        context.Response.Stream = IncreaseValuesBy1(outputValues, context.ServerCallContext.CancellationToken);
    }

    private async IAsyncEnumerable<int> IncreaseValuesBy1(IAsyncEnumerable<int> values, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var value in values.WithCancellation(token).ConfigureAwait(false))
        {
            yield return value + 1;
        }
    }
}