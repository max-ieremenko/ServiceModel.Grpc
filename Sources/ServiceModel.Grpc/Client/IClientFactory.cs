using System;
using Grpc.Core;

namespace ServiceModel.Grpc.Client
{
    public interface IClientFactory
    {
        void AddClient<TContract>(Action<ServiceModelGrpcClientOptions> configure = null)
            where TContract : class;

        TContract CreateClient<TContract>(ChannelBase channel)
            where TContract : class;

        TContract CreateClient<TContract>(CallInvoker callInvoker)
            where TContract : class;
    }
}
