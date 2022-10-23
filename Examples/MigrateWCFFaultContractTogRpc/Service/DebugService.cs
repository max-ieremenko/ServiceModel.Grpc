using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using Contract;

namespace Service;

public sealed class DebugService : IDebugService
{
    public Task ThrowApplicationException(string message)
    {
        var detail = new ApplicationExceptionFaultDetail { Message = message };
        throw new FaultException<ApplicationExceptionFaultDetail>(detail, new FaultReason("demo"));
    }

    public Task ThrowInvalidOperationException(string message)
    {
        var detail = new InvalidOperationExceptionFaultDetail
        {
            Message = message,
            StackTrace = new StackTrace().ToString()
        };

        throw new FaultException<InvalidOperationExceptionFaultDetail>(detail, new FaultReason("demo"));
    }
}