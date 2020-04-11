using System;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Client
{
    public sealed class ServiceModelGrpcClientOptions
    {
        public IMarshallerFactory MarshallerFactory { get; set; }

        public CallOptions? DefaultCallOptions { get; set; }

        public ILogger Logger { get; set; }

        internal Func<IServiceClientBuilder> ClientBuilder { get; set; }
    }
}
