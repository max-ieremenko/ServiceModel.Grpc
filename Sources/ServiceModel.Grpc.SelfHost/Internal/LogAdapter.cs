namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class LogAdapter : ILogger
    {
        private readonly global::Grpc.Core.Logging.ILogger _logger;

        public LogAdapter(global::Grpc.Core.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public void LogError(string message, params object[] args)
        {
            _logger?.Error(message, args);
        }
    }
}
