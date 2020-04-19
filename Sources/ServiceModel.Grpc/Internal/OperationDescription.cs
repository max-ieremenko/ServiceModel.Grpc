using System.Diagnostics;

namespace ServiceModel.Grpc.Internal
{
    [DebuggerDisplay("{ServiceName}/{OperationName}")]
    internal sealed class OperationDescription
    {
        public OperationDescription(string serviceName, string operationName, MessageAssembler message)
        {
            ServiceName = serviceName;
            OperationName = operationName;
            Message = message;
        }

        public string ServiceName { get; }
        
        public string OperationName { get; }
        
        public MessageAssembler Message { get; }
    }
}