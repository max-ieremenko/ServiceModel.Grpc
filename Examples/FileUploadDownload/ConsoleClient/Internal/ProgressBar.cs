using System;
using Contract;

namespace ConsoleClient.Internal;

public sealed class ProgressBar
{
    private static bool _isDisabled;
    private readonly string _format;

    public ProgressBar(string name, long total)
    {
        _format = string.Format("{0} {1} of {2}", name, "{0}", StreamExtensions.SizeToString(total));
    }

    // disable for Benchmarks
    public static void Disable()
    {
        _isDisabled = true;
    }

    public void Report(long current)
    {
        if (!_isDisabled)
        {
            Console.WriteLine(_format, StreamExtensions.SizeToString(current));
        }
    }
}