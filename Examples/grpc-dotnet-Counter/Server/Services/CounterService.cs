/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Counter/Server/Services/CounterService.cs
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;
using Microsoft.Extensions.Logging;

namespace Server.Services
{
    internal sealed class CounterService : ICounterService
    {
        private readonly IncrementingCounter _counter;
        private readonly ILogger _logger;

        public CounterService(IncrementingCounter counter, ILoggerFactory loggerFactory)
        {
            _counter = counter;
            _logger = loggerFactory.CreateLogger(nameof(CounterService));
        }

        public ValueTask<long> IncrementCountAsync()
        {
            _logger.LogInformation("Incrementing count by 1");
            var result = _counter.Increment(1);

            return new ValueTask<long>(result);
        }

        public async ValueTask<long> AccumulateCountAsync(IAsyncEnumerable<int> amounts)
        {
            await foreach (var amount in amounts)
            {
                _logger.LogInformation($"Incrementing count by {amount}");
                _counter.Increment(amount);
            }

            return _counter.Count;
        }

        public async IAsyncEnumerable<long> CountdownAsync()
        {
            for (var i = _counter.Count; i >= 0; i--)
            {
                yield return i;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
