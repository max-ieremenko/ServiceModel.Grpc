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

using Microsoft.Extensions.Options;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.AspNetCore;

[TestFixture]
public class ServiceCollectionExtensionsTest
{
    private ServiceCollection _services = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _services = new ServiceCollection();
    }

    [Test]
    public void WithDefaultOptions()
    {
        _services.AddServiceModelGrpc();

        _services
            .BuildServiceProvider()
            .GetService<IOptions<ServiceModelGrpcServiceOptions>>()
            .ShouldNotBeNull();
    }

    [Test]
    public void WithUserOptions()
    {
        var marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict).Object;
        var errorHandler = new Mock<IServerErrorHandler>(MockBehavior.Strict).Object;
        var stack = new List<string>();

        _services.AddServiceModelGrpc(options =>
        {
            options.ShouldNotBeNull();

            options.DefaultMarshallerFactory = marshallerFactory;

            stack.Add("1");
        });

        _services.AddServiceModelGrpc((options, provider) =>
        {
            options.ShouldNotBeNull();
            provider.ShouldNotBeNull();

            options.DefaultMarshallerFactory.ShouldBe(marshallerFactory);
            options.DefaultErrorHandlerFactory = _ => errorHandler;

            stack.Add("2");
        });

        var options = _services
            .BuildServiceProvider()
            .GetService<IOptions<ServiceModelGrpcServiceOptions>>()
            .ShouldNotBeNull();

        stack.ShouldBeEmpty();

        options.Value.ShouldNotBeNull();
        stack.ShouldBe(new[] { "1", "2" });

        options.Value.DefaultMarshallerFactory.ShouldBe(marshallerFactory);
        options.Value.DefaultErrorHandlerFactory.ShouldNotBeNull();
        options.Value.DefaultErrorHandlerFactory(null!).ShouldBe(errorHandler);
    }

    [Test]
    public void ServiceWithUserOptions()
    {
        var marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict).Object;
        var errorHandler = new Mock<IServerErrorHandler>(MockBehavior.Strict).Object;
        var stack = new List<string>();

        _services.AddServiceModelGrpcServiceOptions<IContract>(options =>
        {
            options.ShouldNotBeNull();

            options.MarshallerFactory = marshallerFactory;

            stack.Add("1");
        });

        _services.AddServiceModelGrpcServiceOptions<IContract>((options, provider) =>
        {
            options.ShouldNotBeNull();
            provider.ShouldNotBeNull();

            options.ErrorHandlerFactory = _ => errorHandler;

            stack.Add("2");
        });

        var options = _services
            .BuildServiceProvider()
            .GetService<IOptions<ServiceModelGrpcServiceOptions<IContract>>>()
            .ShouldNotBeNull();

        stack.ShouldBeEmpty();

        options.Value.ShouldNotBeNull();
        stack.ShouldBe(new[] { "1", "2" });

        options.Value.MarshallerFactory.ShouldBe(marshallerFactory);
        options.Value.ErrorHandlerFactory.ShouldNotBeNull();
        options.Value.ErrorHandlerFactory(null!).ShouldBe(errorHandler);
    }
}