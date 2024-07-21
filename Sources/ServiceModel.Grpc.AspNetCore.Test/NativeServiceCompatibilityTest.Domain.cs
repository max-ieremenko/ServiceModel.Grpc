// <copyright>
// Copyright Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.ServiceModel;

namespace ServiceModel.Grpc.AspNetCore;

public partial class NativeServiceCompatibilityTest
{
    [ServiceContract(Name = "Greeter")]
    public interface IDomainGreeterService
    {
        [OperationContract(Name = "Unary")]
        Task<string> UnaryAsync(string name, CallContext? context = default);

        [OperationContract]
        Task<(string Greet, IAsyncEnumerable<string> Stream)> DuplexStreaming(IAsyncEnumerable<string> names, string greet, CallContext? context = default);
    }

    private sealed class DomainGreeterService : IDomainGreeterService
    {
        public async Task<string> UnaryAsync(string name, CallContext? context)
        {
            var response = await new GreeterService().Unary(new HelloRequest { Name = name }, context!).ConfigureAwait(false);
            return response.Message;
        }

        public async Task<(string Greet, IAsyncEnumerable<string> Stream)> DuplexStreaming(IAsyncEnumerable<string> names, string greet, CallContext? context)
        {
            await Task.CompletedTask.ConfigureAwait(false);

            var stream = Greet(names, greet);
            return (greet, stream);
        }

        private static async IAsyncEnumerable<string> Greet(IAsyncEnumerable<string> names, string greet)
        {
            await foreach (var i in names.ConfigureAwait(false))
            {
                yield return greet + " " + i + "!";
            }
        }
    }
}