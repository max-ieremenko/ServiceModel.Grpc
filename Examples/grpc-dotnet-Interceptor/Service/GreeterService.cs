/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Interceptor/Server/Services/GreeterService.cs
 */

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Microsoft.Extensions.Logging;

namespace Service;

public sealed class GreeterService : IGreeterService
{
    private readonly ILogger _logger;

    public GreeterService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(nameof(GreeterService));
    }

    public string SayHello(string name)
    {
        _logger.LogInformation($"Sending hello to {name}");

        return "Hello " + name;
    }

    public Task<string> SayHelloAsync(string name)
    {
        _logger.LogInformation($"Sending hello to {name}");

        return Task.FromResult("Hello " + name);
        ////throw new InvalidOperationException("test!!!");
    }

    public async IAsyncEnumerable<string> SayHellosAsync(string name, [EnumeratorCancellation] CancellationToken token)
    {
        var i = 0;
        while (!token.IsCancellationRequested)
        {
            var message = $"How are you {name}? {++i}";
            _logger.LogInformation($"Sending greeting {message}.");

            yield return message;

            // Gotta look busy
            await Task.Delay(1000, token);
        }
    }

    public async Task<string> SayHelloToLotsOfBuddiesAsync(IAsyncEnumerable<string> names, CancellationToken token)
    {
        var buffer = new List<string>();
        await foreach (var name in names.WithCancellation(token))
        {
            buffer.Add(name);
        }

        var message = $"Hello {string.Join(", ", names)}";

        _logger.LogInformation($"Sending greeting {message}.");
        return message;
    }

    public async IAsyncEnumerable<string> SayHellosToLotsOfBuddiesAsync(IAsyncEnumerable<string> names, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var name in names.WithCancellation(token))
        {
            yield return "Hello " + name;
        }
    }
}