using System;
using System.Threading;

namespace Client.Internal;

internal sealed class AppExitTokenSource : IDisposable
{
    private readonly CancellationTokenSource _tokenSource;

    public AppExitTokenSource()
    {
        _tokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += ConsoleCancelKeyPress;
    }

    public CancellationToken Token => _tokenSource.Token;

    public void Dispose()
    {
        Console.CancelKeyPress -= ConsoleCancelKeyPress;
        _tokenSource.Dispose();
    }

    private void ConsoleCancelKeyPress(object? sender, ConsoleCancelEventArgs e) => _tokenSource.Cancel();
}