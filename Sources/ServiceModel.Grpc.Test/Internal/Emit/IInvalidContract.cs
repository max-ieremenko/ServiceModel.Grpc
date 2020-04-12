using System;
using System.ServiceModel;
using System.Threading;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.Emit
{
    [ServiceContract]
    public interface IInvalidContract : IDisposable
    {
        [OperationContract]
        void InvalidSignature(ref int value1, out int value2);

        [OperationContract]
        T2 Generic<T1, T2>(T1 value);

        [OperationContract]
        void InvalidContextOptions(CancellationToken token = default, CallOptions options = default);
    }
}
