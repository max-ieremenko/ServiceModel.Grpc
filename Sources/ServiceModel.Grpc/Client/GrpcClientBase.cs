using Grpc.Core;

namespace ServiceModel.Grpc.Client
{
    internal abstract class GrpcClientBase
    {
        protected GrpcClientBase(CallInvoker callInvoker)
        {
            CallInvoker = callInvoker;
        }

        public CallInvoker CallInvoker { get; }
    }
}
