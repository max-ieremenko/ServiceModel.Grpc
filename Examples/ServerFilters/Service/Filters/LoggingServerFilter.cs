using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceModel.Grpc.Filters;

namespace Service.Filters;

public sealed class LoggingServerFilter : IServerFilter
{
    private readonly ILoggerFactory _loggerFactory;

    public LoggingServerFilter(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public async ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        // create logger with a service name
        var logger = _loggerFactory.CreateLogger(context.ServiceInstance.GetType().Name);

        // log input
        LogBegin(logger, context.ContractMethodInfo.Name, context.Request);

        // log client stream in case of Client/Duplex streaming
        if (context.Request.Stream != null)
        {
            context.Request.Stream = LogWrapStream(
                logger,
                context.ContractMethodInfo.Name + " client stream",
                context.Request.Stream,
                context.ServerCallContext.CancellationToken);
        }

        try
        {
            // invoke all other filters in the stack and the service method
            await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // log exception
            logger.LogError("error {0} failed: {1}", context.ContractMethodInfo.Name, ex);
            throw;
        }

        // log server stream in case of Server/Duplex streaming
        if (context.Response.Stream != null)
        {
            context.Response.Stream = LogWrapStream(
                logger,
                context.ContractMethodInfo.Name + " server stream",
                context.Response.Stream,
                context.ServerCallContext.CancellationToken);
        }

        // log output
        LogEnd(logger, context.ContractMethodInfo.Name, context.Response);
    }

    private static void LogBegin(ILogger logger, string methodName, IRequestContext request)
    {
        var message = new StringBuilder()
            .AppendFormat("begin {0}", methodName)
            .Append("(");

        var comma = false;
        foreach (var entry in request)
        {
            if (comma)
            {
                message.Append(", ");
            }

            comma = true;
            message.AppendFormat("{0}={1}", entry.Key, entry.Value);
        }

        message.Append(")");

        logger.LogInformation(message.ToString());
    }

    private static void LogEnd(ILogger logger, string methodName, IResponseContext response)
    {
        var message = new StringBuilder()
            .AppendFormat("end {0}", methodName);

        if (!response.IsProvided)
        {
            message.Append(" (response was created by server filter)");
        }

        message.Append(": ");

        var comma = false;
        foreach (var entry in response)
        {
            if (comma)
            {
                message.Append(", ");
            }

            comma = true;
            message.AppendFormat("{0}={1}", entry.Key, entry.Value);
        }

        if (response.IsProvided)
        {
            logger.LogInformation(message.ToString());
        }
        else
        {
            // warn: the service method was not called
            logger.LogWarning(message.ToString());
        }
    }

    private static object LogWrapStream(ILogger logger, string streamName, object stream, CancellationToken token)
    {
        var itemType = stream
            .GetType()
            .GetInterfaces()
            .First(i => i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            .GenericTypeArguments[0];
            
        var logStream = typeof(LoggingServerFilter)
            .GetMethod(nameof(LogStream), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .MakeGenericMethod(itemType);

        var result = logStream.Invoke(null, new object[] { logger, streamName, stream, token });
        return result;
    }

    private static async IAsyncEnumerable<T> LogStream<T>(ILogger logger, string streamName, IAsyncEnumerable<T> stream, [EnumeratorCancellation] CancellationToken token)
    {
        logger.LogInformation("begin {0}", streamName);
        var count = 0;
        await foreach (var value in stream.WithCancellation(token).ConfigureAwait(false))
        {
            logger.LogInformation("{0} item {1} = {2}", streamName, count, value);
            count++;
            yield return value;
        }

        logger.LogInformation("end {0}, items count {1}", streamName, count);
    }
}