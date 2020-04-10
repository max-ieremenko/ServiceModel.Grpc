using System;

namespace ServiceModel.Grpc
{
    public static class GrpcChannelExtensions
    {
        private const string SwitchHttp2UnencryptedSupport = "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport";

        public static bool Http2UnencryptedSupport
        {
            get => AppContext.TryGetSwitch(SwitchHttp2UnencryptedSupport, out var value) && value;
            set => AppContext.SetSwitch(SwitchHttp2UnencryptedSupport, value);
        }
    }
}
