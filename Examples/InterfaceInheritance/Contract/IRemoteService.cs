// ReSharper disable OperationContractWithoutServiceContract

using System.ServiceModel;

namespace Contract;

// remove [ServiceContract]
public interface IRemoteService
{
    [OperationContract]
    string Touch();
}