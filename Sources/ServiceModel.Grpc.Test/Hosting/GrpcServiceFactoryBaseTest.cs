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
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Hosting
{
    [TestFixture]
    public class GrpcServiceFactoryBaseTest
    {
        private LoggerMock _logger = null!;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            _logger = new LoggerMock();

            var service = new Mock<IInvalidContract>(MockBehavior.Strict);

            var sutType = typeof(GrpcServiceFactoryBase<>).MakeGenericType(service.Object.GetType());
            var sutMockType = typeof(Mock<>).MakeGenericType(sutType);

            var sutMock = (Mock)Activator.CreateInstance(sutMockType, _logger.Logger, DataContractMarshallerFactory.Default, nameof(GrpcServiceFactoryBaseTest))!;

            sutType.InstanceMethod("Bind").Invoke(sutMock!.Object, Array.Empty<object>());
        }

        [Test]
        public void InvalidSignature()
        {
            var log = _logger.Errors.Find(i => i.Contains(nameof(IInvalidContract.InvalidSignature)));
            log.ShouldNotBeNull();
            Console.WriteLine(log);
        }

        [Test]
        public void DisposableIsNotServiceContract()
        {
            var log = _logger.Debug.Find(i => i.Contains(typeof(IDisposable).FullName!));
            log.ShouldNotBeNull();
        }
    }
}
