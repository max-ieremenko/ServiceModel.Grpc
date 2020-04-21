using System;

namespace ServiceModel.Grpc
{
    /// <summary>
    /// Provides set of helpers.
    /// </summary>
    public static class GrpcChannelExtensions
    {
        private const string SwitchHttp2UnencryptedSupport = "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport";

        /// <summary>
        /// Gets or sets a value indicating whether the switch is enabled to be able to use HTTP/2 without TLS with HttpClient.
        /// https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.1#call-insecure-grpc-services-with-net-core-client.
        /// </summary>
        public static bool Http2UnencryptedSupport
        {
            get => AppContext.TryGetSwitch(SwitchHttp2UnencryptedSupport, out var value) && value;
            set => AppContext.SetSwitch(SwitchHttp2UnencryptedSupport, value);
        }
    }
}
