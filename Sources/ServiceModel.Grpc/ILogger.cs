namespace ServiceModel.Grpc
{
    /// <summary>
    /// Represents a type used to perform logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An objects array that contains zero or more objects to format.</param>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An objects array that contains zero or more objects to format.</param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An objects array that contains zero or more objects to format.</param>
        void LogDebug(string message, params object[] args);
    }
}
