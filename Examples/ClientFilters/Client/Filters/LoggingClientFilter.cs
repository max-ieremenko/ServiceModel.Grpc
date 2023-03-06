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

namespace Client.Filters;

internal sealed class LoggingClientFilter : IClientFilter
{
    private readonly ILoggerFactory _loggerFactory;

    public LoggingClientFilter(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public void Invoke(IClientFilterContext context, Action next)
    {
        var logger = CreateLogger(context);

        OnRequest(context, logger);

        try
        {
            // invoke all other filters in the stack and do service call
            next();
        }
        catch (Exception ex)
        {
            OnError(context, logger, ex);
            throw;
        }

        OnResponse(context, logger);
    }

    public async ValueTask InvokeAsync(IClientFilterContext context, Func<ValueTask> next)
    {
        var logger = CreateLogger(context); ;

        OnRequest(context, logger);

        try
        {
            // invoke all other filters in the stack and do service call
            await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            OnError(context, logger, ex);
            throw;
        }

        OnResponse(context, logger);
    }

    private static void LogBegin(ILogger logger, IRequestContext request)
    {
        var message = new StringBuilder()
            .Append("begin (");

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

    private static void LogEnd(ILogger logger, IResponseContext response)
    {
        var message = new StringBuilder()
            .Append("end");

        if (!response.IsProvided)
        {
            message.Append(" (response was created by client filter)");
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
            
        var logStream = typeof(LoggingClientFilter)
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

    private static void OnRequest(IClientFilterContext context, ILogger logger)
    {
        // log input
        LogBegin(logger, context.Request);

        // log client stream in case of Client/Duplex streaming
        if (context.Request.Stream != null)
        {
            context.Request.Stream = LogWrapStream(
                logger,
                "client stream",
                context.Request.Stream,
                context.CallOptions.CancellationToken);
        }
    }

    private static void OnResponse(IClientFilterContext context, ILogger logger)
    {
        // log server stream in case of Server/Duplex streaming
        if (context.Response.Stream != null)
        {
            context.Response.Stream = LogWrapStream(
                logger,
                "server stream",
                context.Response.Stream,
                context.CallOptions.CancellationToken);
        }

        // log output
        LogEnd(logger, context.Response);
    }

    private static void OnError(IClientFilterContext context, ILogger logger, Exception error)
    {
        // log exception
        logger.LogError("error {0} failed: {1}", context.ContractMethodInfo.Name, error);
    }

    private ILogger CreateLogger(IClientFilterContext context)
    {
        // create logger with a contract name
        var categoryName = string.Format("{0}.{1}", context.ContractMethodInfo.DeclaringType?.Name, context.ContractMethodInfo.Name);
        return _loggerFactory.CreateLogger(categoryName);
    }
}