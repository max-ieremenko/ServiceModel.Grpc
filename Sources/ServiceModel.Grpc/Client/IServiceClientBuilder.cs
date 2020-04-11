using System;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Client
{
    internal interface IServiceClientBuilder
    {
        IMarshallerFactory MarshallerFactory { get; set; }

        CallOptions? DefaultCallOptions { get; set; }

        ILogger Logger { get; set; }

        Func<CallInvoker, TContract> Build<TContract>(string factoryId);
    }
}
