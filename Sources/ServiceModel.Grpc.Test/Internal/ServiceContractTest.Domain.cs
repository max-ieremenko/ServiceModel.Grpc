using System.ServiceModel;

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
    }
}
