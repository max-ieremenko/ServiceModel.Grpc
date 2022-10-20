using System;
using System.ServiceModel;
using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Service;

internal sealed class FaultExceptionServerHandler : ServerErrorHandlerBase
{
    protected override ServerFaultDetail? ProvideFaultOrIgnoreCore(ServerCallInterceptorContext context, Exception error)
    {
        // handle FaultException<ApplicationExceptionFaultDetail>
        if (error is FaultException<ApplicationExceptionFaultDetail> appFault)
        {
            // pass detail to a client call
            return new ServerFaultDetail
            {
                Detail = appFault.Detail
            };
        }

        // handle FaultException<InvalidOperationExceptionFaultDetail>
        if (error is FaultException<InvalidOperationExceptionFaultDetail> opFault)
        {
            // pass detail to a client call
            return new ServerFaultDetail
            {
                Detail = opFault.Detail
            };
        }

        // ignore other error
        return null;
    }
}