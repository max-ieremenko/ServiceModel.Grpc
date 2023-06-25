using System;
using ServiceModel.Grpc;

namespace ClientDesignTime;

internal sealed class SimpleConsoleLogger : ILogger
{
    public void LogError(string message, params object[] args) => Log("error", message, args);

    public void LogWarning(string message, params object[] args) => Log("warn", message, args);

    public void LogDebug(string message, params object[] args) => Log("info", message, args);

    private static void Log(string category, string message, params object[] args)
    {
        Console.WriteLine("{0} {1}", category, string.Format(message, args));
    }
}