// <copyright>
// Copyright 2020-2021 Max Ieremenko
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

namespace ServiceModel.Grpc.SelfHost
{
    [TestFixture]
    public class MultipurposeServiceTest : MultipurposeServiceTestBase
    {
        private ServerHost _host = null!;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            _host = new ServerHost();

            _host.Services.AddServiceModelSingleton<IMultipurposeService>(new MultipurposeService());
            DomainService = new ClientFactory().CreateClient<IMultipurposeService>(_host.Channel);

            _host.Start();
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _host.DisposeAsync().ConfigureAwait(false);
        }
    }
}
