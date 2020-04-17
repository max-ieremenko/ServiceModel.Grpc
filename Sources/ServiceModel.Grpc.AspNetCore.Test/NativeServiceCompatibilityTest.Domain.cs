using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.AspNetCore
{
    public partial class NativeServiceCompatibilityTest
    {
        [ServiceContract(Name = "Greeter")]
        public interface IDomainGreeterService
        {
            [OperationContract(Name = "Hello")]
            Task<string> HelloAsync(string name, CallContext context = default);

            [OperationContract(Name = "HelloAll")]
            IAsyncEnumerable<string> HelloAllAsync(IAsyncEnumerable<string> names, string greet, CallContext context = default);
        }

        private sealed class DomainGreeterService : IDomainGreeterService
        {
            public async Task<string> HelloAsync(string name, CallContext context)
            {
                var response = await new GreeterService().Hello(new HelloRequest { Name = name }, context);
                return response.Message;
            }

            public async IAsyncEnumerable<string> HelloAllAsync(IAsyncEnumerable<string> names, string greet, CallContext context = default)
            {
                await foreach (var i in names)
                {
                    yield return greet + " " + i + "!";
                }
            }
        }
    }
}
