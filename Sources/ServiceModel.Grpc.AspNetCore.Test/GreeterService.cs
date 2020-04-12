using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.AspNetCore
{
    internal sealed class GreeterService : Greeter.GreeterBase
    {
        public override Task<HelloResult> Hello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloResult
            {
                Message = "Hello " + request.Name + "!"
            });
        }
    }
}
