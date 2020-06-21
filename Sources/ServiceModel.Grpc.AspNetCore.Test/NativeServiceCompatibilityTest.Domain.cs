// <copyright>
// Copyright 2020 Max Ieremenko
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
            Task<string> HelloAsync(string name, CallContext? context = default);

            [OperationContract(Name = "HelloAll")]
            IAsyncEnumerable<string> HelloAllAsync(IAsyncEnumerable<string> names, string greet, CallContext? context = default);
        }

        private sealed class DomainGreeterService : IDomainGreeterService
        {
            public async Task<string> HelloAsync(string name, CallContext? context)
            {
                var response = await new GreeterService().Hello(new HelloRequest { Name = name }, context!);
                return response.Message;
            }

            public async IAsyncEnumerable<string> HelloAllAsync(IAsyncEnumerable<string> names, string greet, CallContext? context)
            {
                await foreach (var i in names)
                {
                    yield return greet + " " + i + "!";
                }
            }
        }
    }
}
