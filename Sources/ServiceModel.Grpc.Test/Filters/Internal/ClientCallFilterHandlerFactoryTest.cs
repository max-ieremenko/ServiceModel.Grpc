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
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class ClientCallFilterHandlerFactoryTest
{
    private IMethod _grpcMethod = null!;
    private CallInvoker _callInvoker = null!;
    private MethodInfo _asyncContractMethod = null!;
    private MethodInfo _syncContractMethod = null!;
    private IClientFilter _asyncMethodFilter = null!;
    private IClientFilter _syncMethodFilter = null!;
    private ClientCallFilterHandlerFactory _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict).Object;

        _sut = new ClientCallFilterHandlerFactory(serviceProvider.Object, new Dictionary<IMethod, ClientMethodMetadata>());

        var grpcMethod = new Mock<IMethod>(MockBehavior.Strict);
        grpcMethod
            .Setup(m => m.GetHashCode())
            .Returns(1);
        grpcMethod
            .Setup(m => m.Equals(It.IsAny<object>()))
            .Returns<object>(other => ReferenceEquals(other, _grpcMethod));
        _grpcMethod = grpcMethod.Object;

        _asyncContractMethod = typeof(IMultipurposeService).InstanceMethod(nameof(IMultipurposeService.BlockingCallAsync));
        _syncContractMethod = typeof(IMultipurposeService).InstanceMethod(nameof(IMultipurposeService.BlockingCall));

        _sut.MethodMetadataByGrpc.Add(
            _grpcMethod,
            new ClientMethodMetadata(() => _asyncContractMethod, () => _syncContractMethod));

        _asyncMethodFilter = new Mock<IClientFilter>(MockBehavior.Strict).Object;
        _sut.MethodMetadataByGrpc[_grpcMethod].Method.FilterFactories = new[]
        {
            new Func<IServiceProvider, IClientFilter>(provider =>
            {
                provider.ShouldBe(_sut.ServiceProvider);
                return _asyncMethodFilter;
            })
        };

        _syncMethodFilter = new Mock<IClientFilter>(MockBehavior.Strict).Object;
        _sut.MethodMetadataByGrpc[_grpcMethod].AlternateMethod!.FilterFactories = new[]
        {
            new Func<IServiceProvider, IClientFilter>(provider =>
            {
                provider.ShouldBe(_sut.ServiceProvider);
                return _syncMethodFilter;
            })
        };
    }

    [Test]
    public void CreateAsyncHandlerMethodIsNotDefined()
    {
        _sut.MethodMetadataByGrpc.Clear();

        _sut.CreateAsyncHandler(_grpcMethod, _callInvoker, default).ShouldBeNull();
    }

    [Test]
    public void CreateBlockingHandlerMethodIsNotDefined()
    {
        _sut.MethodMetadataByGrpc.Clear();

        _sut.CreateBlockingHandler(_grpcMethod, _callInvoker, default).ShouldBeNull();
    }

    [Test]
    public void CreateAsyncHandler()
    {
        var options = new CallOptions(deadline: DateTime.Now);

        var actual = _sut.CreateAsyncHandler(_grpcMethod, _callInvoker, options).ShouldBeOfType<ClientCallFilterHandler>();

        actual.Filters.Length.ShouldBe(1);
        actual.Filters[0].ShouldBe(_asyncMethodFilter);

        var context = actual.Context.ShouldBeOfType<ClientFilterContext>();

        context.ServiceProvider.ShouldBe(_sut.ServiceProvider);
        context.CallOptions.Deadline.ShouldBe(options.Deadline);
        ReferenceEquals(context.ContractMethodInfo, _asyncContractMethod).ShouldBeTrue();
        context.Method.ShouldBe(_grpcMethod);
        context.RequestInternal.ShouldBeOfType<RequestContext>();
        context.ResponseInternal.ShouldBeOfType<ResponseContext>();
        context.CallInvoker.ShouldBe(_callInvoker);
    }

    [Test]
    public void CreateBlockingHandler()
    {
        var options = new CallOptions(deadline: DateTime.Now);

        var actual = _sut.CreateBlockingHandler(_grpcMethod, _callInvoker, options).ShouldBeOfType<ClientCallFilterHandler>();

        actual.Filters.Length.ShouldBe(1);
        actual.Filters[0].ShouldBe(_syncMethodFilter);

        var context = actual.Context.ShouldBeOfType<ClientFilterContext>();

        context.ServiceProvider.ShouldBe(_sut.ServiceProvider);
        context.CallOptions.Deadline.ShouldBe(options.Deadline);
        context.Method.ShouldBe(_grpcMethod);
        ReferenceEquals(context.ContractMethodInfo, _syncContractMethod).ShouldBeTrue();
        context.RequestInternal.ShouldBeOfType<RequestContext>();
        context.ResponseInternal.ShouldBeOfType<ResponseContext>();
        context.CallInvoker.ShouldBe(_callInvoker);
    }
}