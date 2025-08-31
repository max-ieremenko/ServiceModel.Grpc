using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceModel.Grpc.Filters;

namespace Server.Services;

/// <summary>
/// Filters: log all gRPC calls
/// </summary>
internal sealed class LoggingServerFilter : IServerFilter
{
    private readonly ILogger _logger;

    public LoggingServerFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("ServerFilter");
    }

    public async ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        OnRequest(context);

        try
        {
            await next();
        }
        catch (Exception ex)
        {
            OnException(context, ex);
            throw;
        }

        OnResponse(context);
    }

    private void OnRequest(IServerFilterContext context) =>
        _logger.LogInformation("request {signature}", GetSignature(context.ContractMethodInfo, context.Request));

    private void OnResponse(IServerFilterContext context) =>
        _logger.LogInformation("response {signature}", GetSignature(context.ContractMethodInfo, context.Response));

    private void OnException(IServerFilterContext context, Exception ex) =>
        _logger.LogError(ex, $"{context.ContractMethodInfo.DeclaringType?.Name}.{context.ContractMethodInfo.Name}");

    private static string GetSignature(MethodInfo method, IEnumerable<KeyValuePair<string, object?>> parameters)
    {
        var message = new StringBuilder()
            .Append($"{method.DeclaringType?.Name}.{method.Name}")
            .Append(' ');

        var comma = false;
        foreach (var entry in parameters)
        {
            if (comma)
            {
                message.Append(", ");
            }

            comma = true;
            message
                .Append(entry.Key)
                .Append(':')
                .Append(entry.Value);
        }

        return message.ToString();
    }
}