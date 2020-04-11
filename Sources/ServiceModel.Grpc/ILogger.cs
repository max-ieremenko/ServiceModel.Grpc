namespace ServiceModel.Grpc
{
    public interface ILogger
    {
        void LogError(string message, params object[] args);
    }
}
