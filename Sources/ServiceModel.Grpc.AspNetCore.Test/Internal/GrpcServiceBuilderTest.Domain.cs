using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    public partial class GrpcServiceBuilderTest
    {
        [ServiceContract]
        public interface IContract
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
            void OneParameterContext(CallOptions options, int value);

            [OperationContract]
            Task OneParameterAsync(double value);

            [OperationContract]
            double AddTwoValues(int x, double y);

            [OperationContract]
            Task<string> ConcatThreeValueAsync(int x, string y, CancellationToken token, long z);

            [OperationContract]
            IAsyncEnumerable<int> EmptyServerStreaming();

            [OperationContract]
            IAsyncEnumerable<int> ServerStreamingRepeatValue(int value, int count, CancellationToken token);

            [OperationContract]
            Task ClientStreamingEmpty(IAsyncEnumerable<int> values);

            [OperationContract]
            Task<string> ClientStreamingSumValues(IAsyncEnumerable<int> values, CancellationToken token);

            [OperationContract]
            IAsyncEnumerable<string> DuplexStreamingConvert(IAsyncEnumerable<int> values, CancellationToken token);
        }
    }
}
