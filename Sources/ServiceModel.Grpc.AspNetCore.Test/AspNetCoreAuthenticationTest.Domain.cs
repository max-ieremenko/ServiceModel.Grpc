using System.ServiceModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ServiceModel.Grpc.AspNetCore
{
    public partial class AspNetCoreAuthenticationTest
    {
        [ServiceContract]
        public interface IService
        {
            [OperationContract]
            string GetCurrentUserName(CallContext context = default);

            [OperationContract]
            string TryGetCurrentUserName(CallContext context = default);
        }

        [Authorize]
        internal sealed class Service : IService
        {
            private readonly IHttpContextAccessor _contextAccessor;

            public Service(IHttpContextAccessor contextAccessor)
            {
                _contextAccessor = contextAccessor;
            }

            public string GetCurrentUserName(CallContext context)
            {
                return _contextAccessor.HttpContext.User.Identity.Name;
            }

            [AllowAnonymous]
            public string TryGetCurrentUserName(CallContext context)
            {
                return _contextAccessor.HttpContext.User?.Identity?.Name;
            }
        }
    }
}
