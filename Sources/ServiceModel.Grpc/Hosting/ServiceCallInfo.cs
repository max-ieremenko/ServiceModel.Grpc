using System.Reflection;

namespace ServiceModel.Grpc.Hosting
{
    internal readonly struct ServiceCallInfo
    {
        public ServiceCallInfo(MethodInfo serviceInstanceMethod, MethodInfo channelMethod)
        {
            ServiceInstanceMethod = serviceInstanceMethod;
            ChannelMethod = channelMethod;
        }

        public MethodInfo ServiceInstanceMethod { get; }
        
        public MethodInfo ChannelMethod { get; }
    }
}
