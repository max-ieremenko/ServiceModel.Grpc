using System.ServiceModel;
using Contract;
using ServiceModel.Grpc.Interceptors;

namespace gRPCClient
{
    internal sealed class FaultExceptionClientHandler : ClientErrorHandlerBase
    {
        protected override void ThrowOrIgnoreCore(ClientCallInterceptorContext context, ClientFaultDetail detail)
        {
            // handle ApplicationExceptionFaultDetail
            if (detail.Detail is ApplicationExceptionFaultDetail appDetail)
            {
                throw new FaultException<ApplicationExceptionFaultDetail>(appDetail);
            }

            // handle InvalidOperationExceptionFaultDetail
            if (detail.Detail is InvalidOperationExceptionFaultDetail opDetail)
            {
                throw new FaultException<InvalidOperationExceptionFaultDetail>(opDetail);
            }

            // ignore other errors
        }
    }
}
