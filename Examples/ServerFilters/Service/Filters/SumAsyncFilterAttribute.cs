using System;
using System.Threading.Tasks;
using ServiceModel.Grpc.Filters;

namespace Service.Filters;

internal sealed class SumAsyncFilterAttribute : ServerFilterAttribute
{
    public SumAsyncFilterAttribute(int order)
        : base(order)
    {
    }

    public override ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        var x = (int)context.Request["x"]!;
        var y = (int)context.Request["y"]!;

        // do not call Calculator.SumAsync
        // await next().ConfigureAwait(false);

        context.Response["result"] = x + y;

        return new ValueTask(Task.CompletedTask);
    }
}