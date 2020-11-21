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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public class HostRegistrationsTest
    {
        private KestrelHost _host = null!;
        private List<Metadata.Entry> _clientMetadata = null!;
        private IServerErrorHandler _globalErrorHandler = null!;
        private IServerErrorHandler _localErrorHandler = null!;
        private IErrorService _domainService = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _clientMetadata = new List<Metadata.Entry>();

            var clientErrorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict);
            clientErrorHandler
                .Setup(h => h.ThrowOrIgnore(It.IsAny<ClientCallInterceptorContext>(), It.IsAny<ClientFaultDetail>()))
                .Callback<ClientCallInterceptorContext, ClientFaultDetail>((_, detail) =>
                {
                    _clientMetadata.AddRange(detail
                        .OriginalError
                        .Trailers
                        .Where(i => "GlobalErrorHandler".Equals(i.Key, StringComparison.OrdinalIgnoreCase) || "LocalErrorHandler".Equals(i.Key, StringComparison.OrdinalIgnoreCase)));
                });

            _host = new KestrelHost()
                .ConfigureClientFactory(options => options.ErrorHandler = clientErrorHandler.Object);

            var globalErrorHandler = new Mock<IServerErrorHandler>(MockBehavior.Strict);
            globalErrorHandler
                .Setup(h => h.ProvideFaultOrIgnore(It.IsAny<ServerCallInterceptorContext>(), It.IsNotNull<Exception>()))
                .Returns<ServerCallInterceptorContext, Exception>((context, ex) =>
                {
                    ex.ShouldBeOfType<ApplicationException>();
                    context.ServerCallContext.ResponseTrailers.Add(new Metadata.Entry("GlobalErrorHandler", "dummy"));
                    return null;
                });
            _globalErrorHandler = globalErrorHandler.Object;

            var localErrorHandler = new Mock<IServerErrorHandler>(MockBehavior.Strict);
            localErrorHandler
                .Setup(h => h.ProvideFaultOrIgnore(It.IsAny<ServerCallInterceptorContext>(), It.IsNotNull<Exception>()))
                .Returns<ServerCallInterceptorContext, Exception>((context, ex) =>
                {
                    ex.ShouldBeOfType<ApplicationException>();
                    context.ServerCallContext.ResponseTrailers.Add(new Metadata.Entry("LocalErrorHandler", "dummy"));
                    return null;
                });
            _localErrorHandler = localErrorHandler.Object;
        }

        [TearDown]
        public async Task AfterEachAll()
        {
            await _host.DisposeAsync();
        }

        [Test]
        public async Task GlobalErrorHandler()
        {
            await _host
                .ConfigureServices(services =>
                {
                    services.AddServiceModelGrpc(options => options.DefaultErrorHandlerFactory = _ => _globalErrorHandler);
                })
                .ConfigureEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<ErrorService>();
                })
                .StartAsync();
            _domainService = _host.ClientFactory.CreateClient<IErrorService>(_host.Channel);

            Assert.Throws<RpcException>(() => _domainService.ThrowApplicationException("some message"));

            _clientMetadata.Count.ShouldBe(1);
            _clientMetadata[0].Key.ShouldBe("GlobalErrorHandler", StringCompareShould.IgnoreCase);
        }

        [Test]
        public async Task ServiceErrorHandler()
        {
            await _host
                .ConfigureServices(services =>
                {
                    services.AddServiceModelGrpcServiceOptions<ErrorService>(options => options.ErrorHandlerFactory = _ => _localErrorHandler);
                })
                .ConfigureEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<ErrorService>();
                })
                .StartAsync();
            _domainService = _host.ClientFactory.CreateClient<IErrorService>(_host.Channel);

            Assert.Throws<RpcException>(() => _domainService.ThrowApplicationException("some message"));

            _clientMetadata.Count.ShouldBe(1);
            _clientMetadata[0].Key.ShouldBe("LocalErrorHandler", StringCompareShould.IgnoreCase);
        }

        [Test]
        public async Task ServiceErrorHandlerViaInterface()
        {
            await _host
                .ConfigureServices(services =>
                {
                    services.AddTransient<IErrorService, ErrorService>();
                    services.AddServiceModelGrpcServiceOptions<IErrorService>(options => options.ErrorHandlerFactory = _ => _localErrorHandler);
                })
                .ConfigureEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<IErrorService>();
                })
                .StartAsync();
            _domainService = _host.ClientFactory.CreateClient<IErrorService>(_host.Channel);

            Assert.Throws<RpcException>(() => _domainService.ThrowApplicationException("some message"));

            _clientMetadata.Count.ShouldBe(1);
            _clientMetadata[0].Key.ShouldBe("LocalErrorHandler", StringCompareShould.IgnoreCase);
        }

        [Test]
        public async Task ServiceErrorHandlerOverrideGlobal()
        {
            await _host
                .ConfigureServices(services =>
                {
                    services.AddServiceModelGrpc(options => options.DefaultErrorHandlerFactory = _ => _globalErrorHandler);
                    services.AddServiceModelGrpcServiceOptions<ErrorService>(options => options.ErrorHandlerFactory = _ => _localErrorHandler);
                })
                .ConfigureEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<ErrorService>();
                })
                .StartAsync();
            _domainService = _host.ClientFactory.CreateClient<IErrorService>(_host.Channel);

            Assert.Throws<RpcException>(() => _domainService.ThrowApplicationException("some message"));

            _clientMetadata.Count.ShouldBe(1);
            _clientMetadata[0].Key.ShouldBe("LocalErrorHandler", StringCompareShould.IgnoreCase);
        }
    }
}
