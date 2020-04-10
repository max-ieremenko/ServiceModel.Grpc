using System;
using Grpc.Core.Logging;
using ServiceModel.Grpc.Configuration;

//// ReSharper disable CheckNamespace
namespace Grpc.Core
//// ReSharper restore CheckNamespace
{
    public sealed class ServiceModelGrpcServiceOptions
    {
        public IMarshallerFactory MarshallerFactory { get; set; }

        public ILogger Logger { get; set; }

        public Func<ServerServiceDefinition, ServerServiceDefinition> ConfigureServiceDefinition { get; set; }
    }
}
