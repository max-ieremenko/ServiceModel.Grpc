using System.ServiceModel;

namespace ServiceModel.Grpc.Internal
{
    public partial class ContractDescriptionTest
    {
        [ServiceContract]
        public interface IDuplicateOperationName
        {
            [OperationContract]
            void Ping();

            [OperationContract]
            void Ping(int x);
        }

        [ServiceContract(Name = "Service")]
        public interface IServiceBase
        {
            [OperationContract]
            void Ping();
        }

        [ServiceContract(Name = "Service")]
        public interface IService : IServiceBase
        {
            [OperationContract]
            void Ping(int x);
        }
    }
}
