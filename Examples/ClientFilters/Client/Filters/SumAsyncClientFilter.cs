using System;
using System.Threading.Tasks;
using Contract;
using ServiceModel.Grpc.Filters;

namespace Client.Filters;

internal class SumAsyncClientFilter : IClientFilter
{
    public void Invoke(IClientFilterContext context, Action next)
    {
        // ignore "non Sum" operations
        if (!IsSumOperation(context))
        {
            next();
            return;
        }

        throw new NotImplementedException();
    }

    public ValueTask InvokeAsync(IClientFilterContext context, Func<ValueTask> next)
    {
        // ignore "non Sum" operations
        if (!IsSumOperation(context))
        {
            return next();
        }

        var x = (int)context.Request["x"]!;
        var y = (int)context.Request["y"]!;

        // do not call Calculator.SumAsync
        // await next().ConfigureAwait(false);

        context.Response["result"] = x + y;

        return ValueTask.CompletedTask;
    }

    private static bool IsSumOperation(IClientFilterContext context)
    {
        return context.ContractMethodInfo.Name == nameof(ICalculator.SumAsync);
    }
}