// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Threading;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public abstract class ExceptionHandlingTestBase
    {
        private CancellationTokenSource _cancellationSource = null!;

        protected ExceptionHandlingTestBase(GrpcChannelType channelType)
        {
            ChannelType = channelType;
        }

        protected GrpcChannelType ChannelType { get; }

        protected IErrorService DomainService { get; set; } = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _cancellationSource = new CancellationTokenSource();
        }

        [TearDown]
        public void AfterEachTest()
        {
            _cancellationSource?.Dispose();
        }

        [Test]
        public void ThrowApplicationException()
        {
            var ex = Assert.Throws<ServerException>(() => DomainService.ThrowApplicationException("some text"));

            ex.ShouldNotBeNull();
            ex.Message.ShouldBe("some text");
            ex.Detail.ErrorType.ShouldBe(typeof(ApplicationException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
            Console.WriteLine(ex);
        }

        [Test]
        public void ThrowApplicationExceptionAsync()
        {
            var ex = Assert.ThrowsAsync<ServerException>(() => DomainService.ThrowApplicationExceptionAsync("some text"));

            ex.ShouldNotBeNull();
            ex.Message.ShouldBe("some text");
            ex.Detail.ErrorType.ShouldBe(typeof(ApplicationException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
            Console.WriteLine(ex);
        }

        [Test]
        public void ThrowOperationCanceledException()
        {
            // handled as regular exception
            var ex = Assert.Throws<ServerException>(() => DomainService.ThrowOperationCanceledException());

            ex.ShouldNotBeNull();
            ex.Detail.ErrorType.ShouldBe(typeof(OperationCanceledException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
        }

        [Test]
        public void CancelOperation()
        {
            _cancellationSource.CancelAfter(300);

            var ex = Assert.Throws<OperationCanceledException>(() => DomainService.CancelOperation(_cancellationSource.Token));

            ex.ShouldNotBeNull();
            ex.CancellationToken.ShouldBe(_cancellationSource.Token);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Cancelled);
        }

        [Test]
        public void CancelOperationAsync()
        {
            _cancellationSource.CancelAfter(300);

            var ex = Assert.ThrowsAsync<OperationCanceledException>(() => DomainService.CancelOperationAsync(_cancellationSource.Token));

            ex.ShouldNotBeNull();
            ex.CancellationToken.ShouldBe(_cancellationSource.Token);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Cancelled);
        }

        [Test]
        public void ThrowApplicationExceptionAfterCancel()
        {
            _cancellationSource.CancelAfter(300);

            var ex = Assert.Throws<OperationCanceledException>(() => DomainService.ThrowApplicationExceptionAfterCancel("some text", _cancellationSource.Token));

            ex.ShouldNotBeNull();
            ex.CancellationToken.ShouldBe(_cancellationSource.Token);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Cancelled);
        }

        [Test]
        public void ThrowApplicationExceptionAfterCancelAsync()
        {
            _cancellationSource.CancelAfter(300);

            var ex = Assert.ThrowsAsync<OperationCanceledException>(() => DomainService.ThrowApplicationExceptionAfterCancelAsync("some text", _cancellationSource.Token));

            ex.ShouldNotBeNull();
            ex.CancellationToken.ShouldBe(_cancellationSource.Token);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Cancelled);
        }

        [Test]
        public void ThrowApplicationExceptionClientStreaming()
        {
            var call = DomainService.ThrowApplicationExceptionClientStreaming(new[] { 1, 2 }.AsAsyncEnumerable(), "some text");
            var ex = Assert.ThrowsAsync<ServerException>(() => call);

            ex.ShouldNotBeNull();
            ex.Detail.ErrorType.ShouldBe(typeof(ApplicationException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
        }

        [Test]
        public void ThrowApplicationExceptionServerStreaming()
        {
            var ex = Assert.ThrowsAsync<ServerException>(() => DomainService.ThrowApplicationExceptionServerStreaming("some text").ToListAsync());

            ex.ShouldNotBeNull();
            ex.Detail.ErrorType.ShouldBe(typeof(ApplicationException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
        }

        [Test]
        public void ThrowApplicationExceptionServerStreamingHeader()
        {
            var call = DomainService.ThrowApplicationExceptionServerStreamingHeader("some text");
            var ex = Assert.ThrowsAsync<ServerException>(async () => await call.ConfigureAwait(false));

            ex.ShouldNotBeNull();
            ex.Detail.ErrorType.ShouldBe(typeof(ApplicationException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
        }

        [Test]
        public void ThrowApplicationExceptionDuplexStreaming()
        {
            var call = DomainService.ThrowApplicationExceptionDuplexStreaming(new[] { 1, 2 }.AsAsyncEnumerable(), "some text").ToListAsync();
            var ex = Assert.ThrowsAsync<ServerException>(() => call);

            ex.ShouldNotBeNull();
            ex.Detail.ErrorType.ShouldBe(typeof(ApplicationException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
        }

        [Test]
        public void ThrowApplicationExceptionDuplexStreamingHeader()
        {
            var call = DomainService.ThrowApplicationExceptionDuplexStreamingHeader(new[] { 1, 2 }.AsAsyncEnumerable(), "some text");
            var ex = Assert.ThrowsAsync<ServerException>(async () => await call.ConfigureAwait(false));

            ex.ShouldNotBeNull();
            ex.Detail.ErrorType.ShouldBe(typeof(ApplicationException).FullName);
            ex.InnerException.ShouldBeOfType<RpcException>().StatusCode.ShouldBe(StatusCode.Internal);
        }

        [Test]
        public void ExceptionOnClientSerialize()
        {
            if (ChannelType == GrpcChannelType.GrpcCore)
            {
                // gRPC core channel: ignores interceptors, exception comes directly to the caller
                var ex = Assert.Throws<ApplicationException>(() => DomainService.PassSerializationFail(new DomainObjectSerializationFail { OnSerializedError = "On serialized error" }));
                Console.WriteLine(ex);

                ex.ShouldNotBeNull();
                ex.Message.ShouldBe("On serialized error");
            }
            else
            {
                // gRPC .net channel: invokes interceptors
                var ex = Assert.Throws<RpcException>(() => DomainService.PassSerializationFail(new DomainObjectSerializationFail { OnSerializedError = "On serialized error" }));
                Console.WriteLine(ex);

                ex.ShouldNotBeNull();
                ex.Status.DebugException.ShouldBeOfType<ApplicationException>();
                ex.Status.DebugException.Message.ShouldBe("On serialized error");
            }
        }

        [Test]
        public void ExceptionOnClientDeserialize()
        {
            var ex = Assert.Throws<RpcException>(() => DomainService.ReturnSerializationFail(onDeserializedError: "On deserialized error"));
            Console.WriteLine(ex);

            ex.ShouldNotBeNull();
            ex.StatusCode.ShouldBe(StatusCode.Internal);

            if (ChannelType == GrpcChannelType.GrpcCore)
            {
                ex.Status.Detail.ShouldBe("Failed to deserialize response message.");
                ex.Status.DebugException.ShouldBeNull();
            }
            else
            {
                ex.Status.Detail.ShouldBe("Error starting gRPC call. ApplicationException: On deserialized error");
                ex.Status.DebugException.ShouldBeOfType<ApplicationException>();
                ex.Status.DebugException.Message.ShouldBe("On deserialized error");
            }
        }

        [Test]
        public void ExceptionOnServerDeserialize()
        {
            var ex = Assert.Throws<RpcException>(() => DomainService.PassSerializationFail(new DomainObjectSerializationFail { OnDeserializedError = "On deserialized error" }));
            Console.WriteLine(ex);

            ex.ShouldNotBeNull();
            ex.StatusCode.ShouldBe(StatusCode.Unknown);
            ex.Status.Detail.ShouldBe("Exception was thrown by handler.");
        }

        [Test]
        public void ExceptionOnServerSerialize()
        {
            var ex = Assert.Throws<RpcException>(() => DomainService.ReturnSerializationFail(onSerializedError: "On serialized error"));

            ex.ShouldNotBeNull();
            Console.WriteLine(ex);

            ex.StatusCode.ShouldBeOneOf(StatusCode.Unknown, StatusCode.Cancelled);
        }
    }
}
