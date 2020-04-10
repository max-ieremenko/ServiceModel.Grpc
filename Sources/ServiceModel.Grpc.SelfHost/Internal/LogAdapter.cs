using Grpc.Core.Logging;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class LogAdapter : ILog
    {
        private readonly ILogger _logger;

        public LogAdapter(ILogger logger)
        {
            _logger = logger;
        }

        public void LogError(string message, params object[] args)
        {
            _logger?.Error(message, args);
        }
    }
}
