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

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    [TestFixture]
    public partial class ServiceModelServiceMethodProviderTest
    {
        private Mock<IServiceProvider> _serviceProvider = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        }

        [Test]
        public void GetServiceInstanceTypeAsIs()
        {
            var sut = CreateSut<Service>();

            sut.GetServiceInstanceType().ShouldBe(typeof(Service));
        }

        [Test]
        public void GetServiceInstanceTypeResolve()
        {
            var sut = CreateSut<IService>();

            _serviceProvider
                .Setup(s => s.GetService(typeof(IService)))
                .Returns(new Service());

            sut.GetServiceInstanceType().ShouldBe(typeof(Service));

            _serviceProvider.VerifyAll();
        }

        [Test]
        public void GetServiceInstanceTypeResolveFailed()
        {
            var sut = CreateSut<IService>();

            var fail = new NotSupportedException();

            _serviceProvider
                .Setup(s => s.GetService(typeof(IService)))
                .Throws(fail);

            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetServiceInstanceType());

            Console.WriteLine(ex);

            _serviceProvider.VerifyAll();
            ex.ShouldNotBeNull();
            ex.InnerException.ShouldBe(fail);
        }

        private ServiceModelServiceMethodProvider<TService> CreateSut<TService>()
            where TService : class
        {
            var rootConfiguration = new Mock<IOptions<ServiceModelGrpcServiceOptions>>(MockBehavior.Strict);
            rootConfiguration
                .SetupGet(c => c.Value)
                .Returns(new ServiceModelGrpcServiceOptions());

            var serviceConfiguration = new Mock<IOptions<ServiceModelGrpcServiceOptions<TService>>>(MockBehavior.Strict);
            serviceConfiguration
                .SetupGet(c => c.Value)
                .Returns(new ServiceModelGrpcServiceOptions<TService>());

            var logger = new Mock<ILogger<ServiceModelServiceMethodProvider<TService>>>(MockBehavior.Strict);

            return new ServiceModelServiceMethodProvider<TService>(
                rootConfiguration.Object,
                serviceConfiguration.Object,
                logger.Object,
                _serviceProvider.Object);
        }
    }
}
