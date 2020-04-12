using System.Collections.Generic;
using Moq;

namespace ServiceModel.Grpc.TestApi
{
    public sealed class LoggerMock
    {
        public LoggerMock()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
            Debug = new List<string>();

            var logger = new Mock<ILogger>(MockBehavior.Strict);
            logger
                .Setup(l => l.LogError(It.IsNotNull<string>(), It.IsNotNull<object[]>()))
                .Callback<string, object[]>((message, args) => Errors.Add(string.Format(message, args)));
            logger
                .Setup(l => l.LogWarning(It.IsNotNull<string>(), It.IsNotNull<object[]>()))
                .Callback<string, object[]>((message, args) => Warnings.Add(string.Format(message, args)));
            logger
                .Setup(l => l.LogDebug(It.IsNotNull<string>(), It.IsNotNull<object[]>()))
                .Callback<string, object[]>((message, args) => Debug.Add(string.Format(message, args)));

            Logger = logger.Object;
        }

        public ILogger Logger { get; }

        public List<string> Errors { get; }

        public List<string> Warnings { get; }

        public List<string> Debug { get; }

        public void ClearAll()
        {
            Errors.Clear();
            Warnings.Clear();
            Debug.Clear();
        }
    }
}
