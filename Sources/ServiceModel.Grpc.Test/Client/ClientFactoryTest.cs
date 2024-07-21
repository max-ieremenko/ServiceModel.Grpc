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
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.Client;

[TestFixture]
public partial class ClientFactoryTest
{
    private Mock<IClientBuilder<IDisposable>> _emitClientBuilder = null!;
    private ServiceModelGrpcClientOptions _defaultOptions = null!;
    private Mock<CallInvoker> _callInvoker = null!;
    private IClientErrorHandler _globalErrorHandler = null!;
    private IClientErrorHandler _localErrorHandler = null!;
    private ClientFactory _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _emitClientBuilder = new Mock<IClientBuilder<IDisposable>>(MockBehavior.Strict);

        _defaultOptions = new ServiceModelGrpcClientOptions();

        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);

        _globalErrorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;
        _localErrorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;

        _sut = new ClientFactory(_defaultOptions);
    }

    [Test]
    public void AddClientDefaultOptions()
    {
        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
                ((ClientMethodBinder)binder).DefaultCallOptionsFactory.ShouldBeNull();
                ((ClientMethodBinder)binder).ServiceProvider.ShouldBeNull();
            });

        _sut.AddClient(_emitClientBuilder.Object);

        _emitClientBuilder.VerifyAll();
    }

    [Test]
    public void AddClientCustomOptions()
    {
        var marshaller = new Mock<IMarshallerFactory>(MockBehavior.Strict);
        Func<CallOptions> callOptions = () => throw new NotSupportedException();
        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict).Object;

        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(marshaller.Object);
                ((ClientMethodBinder)binder).DefaultCallOptionsFactory.ShouldBe(callOptions);
                ((ClientMethodBinder)binder).ServiceProvider.ShouldBe(serviceProvider);
            });

        _sut.AddClient(
            _emitClientBuilder.Object,
            options =>
            {
                options.DefaultCallOptionsFactory = callOptions;
                options.MarshallerFactory = marshaller.Object;
                options.ServiceProvider = serviceProvider;
            });

        _emitClientBuilder.VerifyAll();
    }

    [Test]
    public void AddClientWithBuilder()
    {
        var instance = new Mock<ISomeContract>(MockBehavior.Strict);

        var builder = new ManualClientBuilder
        {
            OnBuild = invoker =>
            {
                invoker.ShouldBe(_callInvoker.Object);
                return instance.Object;
            }
        };

        _sut.AddClient(builder);
        _sut.CreateClient<ISomeContract>(_callInvoker.Object).ShouldBe(instance.Object);

        builder.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
        builder.DefaultCallOptionsFactory.ShouldBeNull();
    }

    [Test]
    public void AddClientGenericInterface()
    {
        var clientBuilder = new Mock<IClientBuilder<IComparable<int>>>(MockBehavior.Strict);
        clientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
                ((ClientMethodBinder)binder).DefaultCallOptionsFactory.ShouldBeNull();
            });

        _sut.AddClient(clientBuilder.Object);

        clientBuilder.VerifyAll();
    }

    [Test]
    public void CreateClientWithoutRegistration()
    {
        _sut.CreateClient<IDisposable>(_callInvoker.Object).ShouldNotBeNull();
    }

    [Test]
    public void DoubleClientRegistration()
    {
        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()));

        _sut.AddClient<IDisposable>();

        Assert.Throws<InvalidOperationException>(() => _sut.AddClient<IDisposable>());
    }

    [Test]
    public void AddClientNonInterface()
    {
        var ex = Assert.Throws<NotSupportedException>(() => _sut.AddClient<object>());

        TestOutput.WriteLine(ex);
    }

    [Test]
    public void AddClientInternalInterface()
    {
        var ex = Assert.Throws<NotSupportedException>(() => _sut.AddClient<IInternalContract>());

        TestOutput.WriteLine(ex);
    }

    [Test]
    public void GlobalErrorHandler()
    {
        _defaultOptions.ErrorHandler = _globalErrorHandler;

        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
                ((ClientMethodBinder)binder).DefaultCallOptionsFactory.ShouldBeNull();
            });

        _emitClientBuilder
            .Setup(b => b.Build(It.IsNotNull<CallInvoker>()))
            .Callback<CallInvoker>(i =>
            {
                var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                var callInterceptor = interceptor
                    .GetType()
                    .InstanceProperty("CallInterceptor")
                    .GetValue(interceptor)
                    .ShouldNotBeNull();
                callInterceptor
                    .GetType()
                    .InstanceProperty("ErrorHandler")
                    .GetValue(callInterceptor)
                    .ShouldBe(_globalErrorHandler);

                callInvoker.ShouldBe(_callInvoker.Object);
            })
            .Returns(new Mock<IDisposable>(MockBehavior.Strict).Object);

        _sut.AddClient(_emitClientBuilder.Object);
        _sut.CreateClient<IDisposable>(_callInvoker.Object);

        _emitClientBuilder.VerifyAll();
    }

    [Test]
    public void ServiceErrorHandler()
    {
        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
                ((ClientMethodBinder)binder).DefaultCallOptionsFactory.ShouldBeNull();
            });

        _emitClientBuilder
            .Setup(b => b.Build(It.IsNotNull<CallInvoker>()))
            .Callback<CallInvoker>(i =>
            {
                var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                var callInterceptor = interceptor
                    .GetType()
                    .InstanceProperty("CallInterceptor")
                    .GetValue(interceptor)
                    .ShouldNotBeNull();
                callInterceptor
                    .GetType()
                    .InstanceProperty("ErrorHandler")
                    .GetValue(callInterceptor)
                    .ShouldBe(_localErrorHandler);

                callInvoker.ShouldBe(_callInvoker.Object);
            })
            .Returns(new Mock<IDisposable>(MockBehavior.Strict).Object);

        _sut.AddClient(_emitClientBuilder.Object, options => options.ErrorHandler = _localErrorHandler);
        _sut.CreateClient<IDisposable>(_callInvoker.Object);

        _emitClientBuilder.VerifyAll();
    }

    [Test]
    public void ServiceErrorHandlerOverrideGlobal()
    {
        _defaultOptions.ErrorHandler = _globalErrorHandler;

        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
                ((ClientMethodBinder)binder).DefaultCallOptionsFactory.ShouldBeNull();
            });

        _emitClientBuilder
            .Setup(b => b.Build(It.IsNotNull<CallInvoker>()))
            .Callback<CallInvoker>(i =>
            {
                var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                var callInterceptor = interceptor
                    .GetType()
                    .InstanceProperty("CallInterceptor")
                    .GetValue(interceptor)
                    .ShouldNotBeNull();
                callInterceptor
                    .GetType()
                    .InstanceProperty("ErrorHandler")
                    .GetValue(callInterceptor)
                    .ShouldBe(_localErrorHandler);

                callInvoker.ShouldBe(_callInvoker.Object);
            })
            .Returns(new Mock<IDisposable>(MockBehavior.Strict).Object);

        _sut.AddClient(_emitClientBuilder.Object, options => options.ErrorHandler = _localErrorHandler);
        _sut.CreateClient<IDisposable>(_callInvoker.Object);

        _emitClientBuilder.VerifyAll();
    }

    [Test]
    public void GlobalFilter()
    {
        var filter = new Mock<IClientFilter>(MockBehavior.Strict);

        _defaultOptions.Filters.Add(2, filter.Object);

        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                var filters = ((ClientMethodBinder)binder).GetFilterRegistrations();

                filters.ShouldNotBeNull();
                filters.Count.ShouldBe(1);

                filters[0].Order.ShouldBe(2);
                filters[0].Factory(null!).ShouldBe(filter.Object);
            });

        _sut.AddClient(_emitClientBuilder.Object);

        _emitClientBuilder.VerifyAll();
    }

    [Test]
    public void ServiceFilter()
    {
        var filter = new Mock<IClientFilter>(MockBehavior.Strict);

        _emitClientBuilder
            .Setup(b => b.Initialize(It.IsNotNull<ClientMethodBinder>()))
            .Callback<IClientMethodBinder>(binder =>
            {
                var filters = ((ClientMethodBinder)binder).GetFilterRegistrations();

                filters.ShouldNotBeNull();
                filters.Count.ShouldBe(1);

                filters[0].Order.ShouldBe(1);
                filters[0].Factory(null!).ShouldBe(filter.Object);
            });

        _sut.AddClient<IDisposable>(
            _emitClientBuilder.Object,
            options =>
            {
                options.Filters.Add(1, filter.Object);
            });

        _emitClientBuilder.VerifyAll();
    }
}