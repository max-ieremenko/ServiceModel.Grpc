using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.AspNetCore
{
    public partial class HeadersHandlingTest
    {
        [ServiceContract]
        public interface IHeadersService
        {
            [OperationContract]
            string GetRequestHeader(string headerName, CallContext context = default);

            [OperationContract]
            Task WriteResponseHeader(string headerName, string headerValue, CallContext context = default);

            [OperationContract]
            IAsyncEnumerable<int> ServerStreamingWriteResponseHeader(string headerName, string headerValue, CallContext context = default);

            [OperationContract]
            Task<string> ClientStreaming(IAsyncEnumerable<int> values, CallContext context = default);

            [OperationContract]
            IAsyncEnumerable<string> DuplexStreaming(IAsyncEnumerable<int> values, CallContext context = default);
        }

        private sealed class HeadersService : IHeadersService
        {
            public string GetRequestHeader(string headerName, CallContext context)
            {
                var header = context.ServerCallContext.RequestHeaders.FirstOrDefault(i => i.Key == headerName);
                return header?.Value;
            }

            public async Task WriteResponseHeader(string headerName, string headerValue, CallContext context)
            {
                await context.ServerCallContext.WriteResponseHeadersAsync(new Metadata
                {
                    { headerName, headerValue }
                });
            }

            public async IAsyncEnumerable<int> ServerStreamingWriteResponseHeader(string headerName, string headerValue, CallContext context = default)
            {
                ServerCallContext serverContext = context;
                await serverContext.WriteResponseHeadersAsync(new Metadata
                {
                    { headerName, headerValue }
                });

                foreach (var i in Enumerable.Range(1, 10))
                {
                    await Task.Delay(0);
                    yield return i;
                }
            }

            public async Task<string> ClientStreaming(IAsyncEnumerable<int> values, CallContext context = default)
            {
                var header = context.ServerCallContext.RequestHeaders.FirstOrDefault(i => i.Key == "h1");

                var list = await values.ToListAsync();

                await context.ServerCallContext.WriteResponseHeadersAsync(new Metadata
                {
                    { "h1", header.Value + list.Count }
                });

                return header.Value;
            }

            public async IAsyncEnumerable<string> DuplexStreaming(IAsyncEnumerable<int> values, CallContext context = default)
            {
                var header = context.ServerCallContext.RequestHeaders.FirstOrDefault(i => i.Key == "h1");

                var list = await values.ToListAsync();

                await context.ServerCallContext.WriteResponseHeadersAsync(new Metadata
                {
                    { "h1", header.Value + list.Count }
                });

                yield return header.Value;
            }
        }
    }
}
