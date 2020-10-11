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
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using Shouldly;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    [TestFixture]
    public partial class SelfHostGrpcServiceFactoryTest
    {
        [Test]
        public void GetServiceInstanceTypeAsIs()
        {
            var sut = CreateSut<Service>(() => throw new NotImplementedException());

            sut.GetServiceInstanceType().ShouldBe(typeof(Service));
        }

        [Test]
        public void GetServiceInstanceTypeResolve()
        {
            var sut = CreateSut<IService>(() => new Service());

            sut.GetServiceInstanceType().ShouldBe(typeof(Service));
        }

        [Test]
        public void GetServiceInstanceTypeResolveFailed()
        {
            var fail = new NotSupportedException();

            var sut = CreateSut<IService>(() => throw fail);

            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetServiceInstanceType());

            Console.WriteLine(ex);

            ex.InnerException.ShouldBe(fail);
        }

        private SelfHostGrpcServiceFactory<TService> CreateSut<TService>(Func<TService> serviceFactory)
            where TService : class
        {
            var logger = new Mock<ILogger>(MockBehavior.Strict);

            var builder = new Mock<ServerServiceDefinition.Builder>(MockBehavior.Strict);

            return new SelfHostGrpcServiceFactory<TService>(
                logger.Object,
                DataContractMarshallerFactory.Default,
                serviceFactory,
                builder.Object);
        }
    }
}
