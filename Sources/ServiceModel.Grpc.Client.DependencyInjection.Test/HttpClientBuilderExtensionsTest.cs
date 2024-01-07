// <copyright>
// Copyright 2023 Max Ieremenko
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

using System;
using System.Net.Http;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Client.DependencyInjection.Internal;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.Client.DependencyInjection;

[TestFixture]
public class HttpClientBuilderExtensionsTest
{
    private Mock<IClientFactory> _clientFactory = null!;
    private IContract _proxy = null!;
    private ServiceCollection _services = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _clientFactory = new Mock<IClientFactory>(MockBehavior.Strict);
        _proxy = new Mock<IContract>(MockBehavior.Strict).Object;
        _services = new ServiceCollection();
    }

    [Test]
    public void RegisterNetOnly()
    {
        AddGrpcClient()
            .ConfigureServiceModelGrpcClientCreator<IContract>();

        Validate();
    }

    [Test]
    public void RegisterNetWithClientBuilder()
    {
        var clientBuilder = new Mock<IClientBuilder<IContract>>(MockBehavior.Strict).Object;

        HttpClientBuilderExtensions.ConfigureServiceModelGrpcClientBuilder(
                AddGrpcClient(),
                clientBuilder,
                null);

        Validate();
    }

    [Test]
    public void RegisterNetThenServiceModel()
    {
        AddGrpcClient()
            .ConfigureServiceModelGrpcClientCreator<IContract>();

        _services
            .AddServiceModelGrpcClient<IContract>();

        Validate();
    }

    [Test]
    public void RegisterServiceModelThenNet()
    {
        _services
            .AddServiceModelGrpcClient<IContract>();

        AddGrpcClient()
            .ConfigureServiceModelGrpcClientCreator<IContract>();

        Validate();
    }

    [Test]
    public void TwoDifferentCalInvokersServiceModelThenNet()
    {
        _services
            .AddServiceModelGrpcClient<IContract>(channel: ChannelProviderFactory.Default());

        AddGrpcClient()
            .ConfigureServiceModelGrpcClientCreator<IContract>();

        var provider = _services.BuildServiceProvider();

        var ex = Should.Throw<NotSupportedException>(provider.GetRequiredService<IContract>);
        TestOutput.WriteLine(ex);
    }

    [Test]
    public void TwoDifferentCalInvokersNetThenServiceModel()
    {
        AddGrpcClient()
            .ConfigureServiceModelGrpcClientCreator<IContract>();

        _services
            .AddServiceModelGrpcClient<IContract>(channel: ChannelProviderFactory.Default());

        var provider = _services.BuildServiceProvider();

        var ex = Should.Throw<NotSupportedException>(provider.GetRequiredService<IContract>);
        TestOutput.WriteLine(ex);
    }

    private IHttpClientBuilder AddGrpcClient()
    {
        var result = _services
            .AddGrpcClient<IContract>(options => options.Address = new Uri("https://localhost1:5001"));

#if NET462
        // https://learn.microsoft.com/en-us/aspnet/core/grpc/netstandard?view=aspnetcore-8.0#httphandler-configuration
        result = result.ConfigurePrimaryHttpMessageHandler(() => new WinHttpHandler());
#endif
        return result;
    }

    private void Validate()
    {
        // skip client factory chain
        _services.AddSingleton(_clientFactory.Object);

        _clientFactory
            .Setup(f => f.CreateClient<IContract>(It.IsNotNull<CallInvoker>()))
            .Callback<CallInvoker>(invoker =>
            {
                invoker.ShouldNotBeNull();
                invoker.GetType().FullName.ShouldBe("Grpc.Net.Client.Internal.HttpClientCallInvoker");
            })
            .Returns(_proxy);

        var provider = _services.BuildServiceProvider();

        var actual = provider.GetRequiredService<IContract>();

        actual.ShouldBe(_proxy);
        _clientFactory.VerifyAll();
    }
}