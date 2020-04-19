using System.ServiceModel;

namespace ServiceModel.Grpc.Internal.Emit
{
    [ServiceContract]
    public interface IGenericContract<in T1, T2>
    {
        [OperationContract]
        T2 Invoke(T1 value, T2 value2);
    }
}
