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

using System;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Internal;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class ClientCallFilterHandlerFactoryTest
{
    private IMethod _grpcMethod = null!;
    private CallInvoker _callInvoker = null!;
    private Mock<IOperationDescriptor> _asyncOperation = null!;
    private Mock<IOperationDescriptor> _syncOperation = null!;
    private MethodInfo _asyncMethod = null!;
    private MethodInfo _syncMethod = null!;
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

        _asyncMethod = new Mock<MethodInfo>().Object;
        _syncMethod = new Mock<MethodInfo>().Object;

        _asyncOperation = new Mock<IOperationDescriptor>(MockBehavior.Strict);
        _asyncOperation
            .Setup(d => d.GetContractMethod())
            .Returns(_asyncMethod);
        _asyncOperation
            .Setup(d => d.GetRequestAccessor())
            .Returns(new Mock<IMessageAccessor>(MockBehavior.Strict).Object);
        _asyncOperation
            .Setup(d => d.GetRequestStreamAccessor())
            .Returns(new Mock<IStreamAccessor>(MockBehavior.Strict).Object);
        _asyncOperation
            .Setup(d => d.GetResponseAccessor())
            .Returns(new Mock<IMessageAccessor>(MockBehavior.Strict).Object);
        _asyncOperation
            .Setup(d => d.GetResponseStreamAccessor())
            .Returns(new Mock<IStreamAccessor>(MockBehavior.Strict).Object);

        _syncOperation = new Mock<IOperationDescriptor>(MockBehavior.Strict);
        _syncOperation
            .Setup(d => d.GetContractMethod())
            .Returns(_syncMethod);
        _syncOperation
            .Setup(d => d.GetRequestAccessor())
            .Returns(new Mock<IMessageAccessor>(MockBehavior.Strict).Object);
        _syncOperation
            .Setup(d => d.GetRequestStreamAccessor())
            .Returns(new Mock<IStreamAccessor>(MockBehavior.Strict).Object);
        _syncOperation
            .Setup(d => d.GetResponseAccessor())
            .Returns(new Mock<IMessageAccessor>(MockBehavior.Strict).Object);
        _syncOperation
            .Setup(d => d.GetResponseStreamAccessor())
            .Returns(new Mock<IStreamAccessor>(MockBehavior.Strict).Object);

        _sut.MethodMetadataByGrpc.Add(
            _grpcMethod,
            new ClientMethodMetadata(_asyncOperation.Object, _syncOperation.Object));

        _asyncMethodFilter = new Mock<IClientFilter>(MockBehavior.Strict).Object;
        _sut.MethodMetadataByGrpc[_grpcMethod].Operation.FilterFactories =
        [
            provider =>
            {
                provider.ShouldBe(_sut.ServiceProvider);
                return _asyncMethodFilter;
            }
        ];

        _syncMethodFilter = new Mock<IClientFilter>(MockBehavior.Strict).Object;
        _sut.MethodMetadataByGrpc[_grpcMethod].AlternateOperation!.FilterFactories =
        [
            provider =>
            {
                provider.ShouldBe(_sut.ServiceProvider);
                return _syncMethodFilter;
            }
        ];
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
        context.ContractMethodInfo.ShouldBe(_asyncMethod);
        context.Method.ShouldBe(_grpcMethod);
        context.RequestInternal.ShouldBeOfType<RequestContext>();
        context.ResponseInternal.ShouldBeOfType<ResponseContext>();
        context.CallInvoker.ShouldBe(_callInvoker);
        context.UserState.ShouldNotBeNull();
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
        context.ContractMethodInfo.ShouldBe(_syncMethod);
        context.RequestInternal.ShouldBeOfType<RequestContext>();
        context.ResponseInternal.ShouldBeOfType<ResponseContext>();
        context.CallInvoker.ShouldBe(_callInvoker);
        context.UserState.ShouldNotBeNull();
    }
}