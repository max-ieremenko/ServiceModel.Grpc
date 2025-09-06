using System;
using Microsoft.Extensions.Logging;
using IGrpcLogger = Grpc.Core.Logging.ILogger;

namespace Server;

internal sealed class GrpcCoreLogger : IGrpcLogger
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _target;

    public GrpcCoreLogger(ILoggerFactory loggerFactory)
        : this(loggerFactory, loggerFactory.CreateLogger<GrpcCoreLogger>())
    {
    }

    private GrpcCoreLogger(ILoggerFactory loggerFactory, ILogger target)
    {
        _loggerFactory = loggerFactory;
        _target = target;
    }

    public IGrpcLogger ForType<T>() => new GrpcCoreLogger(_loggerFactory, _loggerFactory.CreateLogger<T>());

    public void Debug(string message) => _target.LogDebug(message);

    public void Debug(string format, params object[] formatArgs) => _target.LogDebug(format, formatArgs);

    public void Info(string message) => _target.LogInformation(message);

    public void Info(string format, params object[] formatArgs) => _target.LogInformation(format, formatArgs);

    public void Warning(string message) => _target.LogWarning(message);

    public void Warning(string format, params object[] formatArgs) => _target.LogWarning(format, formatArgs);

    public void Warning(Exception exception, string message) => _target.LogWarning(exception, message);

    public void Error(string message) => _target.LogError(message);

    public void Error(string format, params object[] formatArgs) => _target.LogError(format, formatArgs);

    public void Error(Exception exception, string message) => _target.LogError(exception, message);
}