using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal
{
    public partial class GrpcServiceClientBuilderTest
    {
        [ServiceContract]
        public interface IContract : IDisposable
        {
            [OperationContract]
            void Empty();

            [OperationContract]
            Task EmptyAsync();

            [OperationContract]
            void EmptyContext(CallContext context);

            [OperationContract]
            Task EmptyTokenAsync(CancellationToken token);

            [OperationContract]
            string ReturnString();

            [OperationContract]
            Task<string> ReturnStringAsync(ServerCallContext context = default);

            [OperationContract]
            void OneParameterContext(int value, CallOptions options);

            [OperationContract]
            Task OneParameterAsync(double value);

            [OperationContract]
            double AddTwoValues(int x, double y);

            [OperationContract]
            Task<string> ConcatThreeValueAsync(int x, string y, long z, CancellationToken token);

            [OperationContract]
            IAsyncEnumerable<int> ServerStreamingRepeatValue(int value, int count, CancellationToken token);

            [OperationContract]
            IAsyncEnumerable<int> EmptyServerStreaming();

            [OperationContract]
            Task<int> ClientStreamingSumValues(IAsyncEnumerable<int> values, CancellationToken token);

            [OperationContract]
            Task EmptyClientStreaming(IAsyncEnumerable<int> values);

            [OperationContract]
            IAsyncEnumerable<string> DuplexStreamingConvert(IAsyncEnumerable<int> values, CancellationToken token);
        }
    }
}
