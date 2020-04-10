namespace ServiceModel.Grpc.Hosting
{
    internal interface ILog
    {
        void LogError(string message, params object[] args);
    }
}
