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

using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using GrpcChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.SelfHost
{
    [TestFixture]
    public class ExceptionHandlingTest : ExceptionHandlingTestBase
    {
        private const int Port = 8080;
        private GrpcChannel _channel;
        private Server _server;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            _server = new Server
            {
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };

            _channel = new GrpcChannel("localhost", Port, ChannelCredentials.Insecure);
            _server.Services.AddServiceModelSingleton(
                new ErrorService(),
                options =>
                {
                    options.ErrorHandler = new ServerErrorHandler();
                });

            DomainService = new ClientFactory(new ServiceModelGrpcClientOptions { ErrorHandler = new ClientErrorHandler() })
                .CreateClient<IErrorService>(_channel.CreateCallInvoker());

            _server.Start();
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _channel.ShutdownAsync();
            await _server.ShutdownAsync();
        }
    }
}
