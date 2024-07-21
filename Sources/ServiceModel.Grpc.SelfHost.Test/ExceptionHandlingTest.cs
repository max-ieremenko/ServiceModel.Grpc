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

using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.SelfHost;

[TestFixture(GrpcChannelType.GrpcCore)]
#if NET6_0_OR_GREATER
[TestFixture(GrpcChannelType.GrpcDotNet)]
#endif
public class ExceptionHandlingTest : ExceptionHandlingTestBase
{
    private ServerHost _host = null!;

    public ExceptionHandlingTest(GrpcChannelType channelType)
        : base(channelType)
    {
    }

    [OneTimeSetUp]
    public void BeforeAll()
    {
        _host = new ServerHost(ChannelType);

        _host.Services.AddServiceModelSingleton(
            new ErrorService(),
            options =>
            {
                options.ErrorHandler = new ServerErrorHandler();
            });

        _host.Start();

        DomainService = new ClientFactory(new ServiceModelGrpcClientOptions { ErrorHandler = new ClientErrorHandler() })
            .CreateClient<IErrorService>(_host.Channel.CreateCallInvoker());
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }
}