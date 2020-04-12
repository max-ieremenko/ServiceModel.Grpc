namespace ServiceModel.Grpc
{
    public interface ILogger
    {
        void LogError(string message, params object[] args);

        void LogWarning(string message, params object[] args);

        void LogDebug(string message, params object[] args);
    }
}
