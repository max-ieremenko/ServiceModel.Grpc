using System;
using System.ServiceModel;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal
{
    public partial class ServiceContractTest
    {
        [ServiceContract]
        public interface IServiceContract
        {
            [OperationContract]
            void Empty();

            void Ignore();
        }

        [ServiceContract]
        public interface IGenericServiceContract<T>
        {
            [OperationContract]
            void Invoke(T value);
        }

        [BindServiceMethod(typeof(NativeGrpcService), nameof(BindService))]
        public abstract class NativeGrpcService
        {
            public static void BindService() => throw new NotImplementedException();
        }
    }
}
