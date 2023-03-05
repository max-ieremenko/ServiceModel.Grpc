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
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.Filters;

[TestFixture]
public class FilteredClientTest
{
    private Mock<CallInvoker> _callInvoker = null!;
    private IFilteredService _client = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            Filters =
            {
                new FilterRegistration<IClientFilter>(1, _ => new TrackingClientFilter("global"))
            }
        });

        clientFactory.AddClient<IFilteredService>(options =>
        {
            options.Filters.Add(new FilterRegistration<IClientFilter>(2, _ => new TrackingClientFilter("client-options")));
        });

        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);

        _client = clientFactory.CreateClient<IFilteredService>(_callInvoker.Object);
    }

    [Test]
    public void UnarySync()
    {
        _callInvoker.SetupBlockingUnaryCallInOut<IList<string>, IList<string>>(input => new List<string>(input) { "implementation" });

        var track = _client.UnarySync(new[] { nameof(IFilteredService.UnarySync) });

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.UnarySync),
            "global-before",
            "client-options-before",
            "implementation",
            "client-options-after",
            "global-after"
        });
    }

    [Test]
    public async Task UnaryAsync()
    {
        _callInvoker.SetupAsyncUnaryCallInOut<IList<string>, IList<string>>(input => new List<string>(input) { "implementation" });

        var track = await _client.UnaryAsync(new[] { nameof(IFilteredService.UnaryAsync) }).ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.UnaryAsync),
            "global-before",
            "client-options-before",
            "implementation",
            "client-options-after",
            "global-after"
        });
    }

    [Test]
    public async Task ClientStreamAsync()
    {
        var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
        requestStream.Setup(new List<int>());

        var response = new List<string>();

        _callInvoker.SetupAsyncClientStreamingCall(
            requestStream.Object,
            (IList<string>)response,
            options =>
            {
                var values = CompatibilityToolsTestExtensions.DeserializeMethodInput<List<string>>(DataContractMarshallerFactory.Default, options.Headers);
                response.AddRange(values);
                response.Add("implementation");
            });

        var track = await _client.ClientStreamAsync(new[] { 1 }.AsAsyncEnumerable(), new[] { nameof(IFilteredService.ClientStreamAsync) }).ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.ClientStreamAsync),
            "global-before",
            "client-options-before",
            "implementation",
            "client-options-after",
            "global-after"
        });
    }

    [Test]
    public async Task ServerStreamSync()
    {
        var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);

        _callInvoker.SetupAsyncServerStreamingCall<IList<string>, string>(
            (request, _) =>
            {
                var response = new List<string>(request) { "implementation" };
                responseStream.Setup(default, response.ToArray());
                return responseStream.Object;
            });

        var stream = _client.ServerStreamSync(new[] { nameof(IFilteredService.ServerStreamSync) });

        var track = await stream.ToListAsync().ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.ServerStreamSync),
            "global-before",
            "client-options-before",
            "implementation",
            "client-options-after",
            "global-after"
        });
    }

    [Test]
    public async Task ServerStreamAsync()
    {
        var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
        responseStream.Setup(default, 1);

        _callInvoker.SetupAsyncServerStreamingCall<IList<string>, int>(
            (request, metadata) =>
            {
                IList<string> response = new List<string>(request) { "implementation" };
                var headers = CompatibilityToolsTestExtensions.SerializeMethodOutput(DataContractMarshallerFactory.Default, response);
                metadata.Add(headers[0]);
                return responseStream.Object;
            });

        var (stream, track) = await _client.ServerStreamAsync(new[] { nameof(IFilteredService.ServerStreamAsync) }).ConfigureAwait(false);

        await stream.ToListAsync().ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.ServerStreamAsync),
            "global-before",
            "client-options-before",
            "implementation",
            "client-options-after",
            "global-after"
        });
    }

    [Test]
    public async Task DuplexStreamAsync()
    {
        var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
        responseStream.Setup(default, 1);

        var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
        requestStream.Setup(new List<int>());

        _callInvoker.SetupAsyncDuplexStreamingCall(
            requestStream.Object,
            (options, responseHeaders) =>
            {
                var input = CompatibilityToolsTestExtensions.DeserializeMethodInput<List<string>>(DataContractMarshallerFactory.Default, options.Headers);

                IList<string> response = new List<string>(input) { "implementation" };
                var headers = CompatibilityToolsTestExtensions.SerializeMethodOutput(DataContractMarshallerFactory.Default, response);
                responseHeaders.Add(headers[0]);
                return responseStream.Object;
            });

        var (stream, track) = await _client
            .DuplexStreamAsync(new[] { 1 }.AsAsyncEnumerable(), new[] { nameof(IFilteredService.DuplexStreamAsync) })
            .ConfigureAwait(false);

        await stream.ToListAsync().ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.DuplexStreamAsync),
            "global-before",
            "client-options-before",
            "implementation",
            "client-options-after",
            "global-after"
        });
    }

    [Test]
    public async Task DuplexStreamSync()
    {
        var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);

        var requestStream = new Mock<IClientStreamWriter<Message<string>>>(MockBehavior.Strict);
        requestStream.Setup(new List<string>());

        _callInvoker.SetupAsyncDuplexStreamingCall(
            requestStream.Object,
            (options, _) =>
            {
                var input = CompatibilityToolsTestExtensions.DeserializeMethodInput<List<string>>(DataContractMarshallerFactory.Default, options.Headers);
                responseStream.Setup(default, new List<string>(input) { "implementation" }.ToArray());
                return responseStream.Object;
            });

        var stream = _client.DuplexStreamSync(Array.Empty<string>().AsAsyncEnumerable(), new[] { nameof(IFilteredService.DuplexStreamSync) });

        var track = await stream.ToListAsync().ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.DuplexStreamSync),
            "global-before",
            "client-options-before",
            "implementation",
            "client-options-after",
            "global-after"
        });
    }

    private sealed class TrackingClientFilter : IClientFilter
    {
        public TrackingClientFilter(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void Invoke(IClientFilterContext context, Action next)
        {
            OnRequest(context);
            next();
            OnResponse(context);
        }

        public async ValueTask InvokeAsync(IClientFilterContext context, Func<ValueTask> next)
        {
            OnRequest(context);
            await next().ConfigureAwait(false);
            OnResponse(context);
        }

        private void OnRequest(IClientFilterContext context)
        {
            context.ContractMethodInfo.ShouldNotBeNull();

            var message = Name + "-before";

            if (context.Request.Count != 0)
            {
                var input = context.Request[0].ShouldBeAssignableTo<IList<string>>()!;
                context.ContractMethodInfo.Name.ShouldBe(input[0]);

                context.Request["input"] = new List<string>(input) { message };
            }

            if (context.Request.Stream != null)
            {
                context.Request.Stream = ExtendStream(context.Request.Stream, message);
            }
        }

        private void OnResponse(IClientFilterContext context)
        {
            context.Response.IsProvided.ShouldBeTrue();

            var message = Name + "-after";

            if (context.Response.Count != 0)
            {
                var result = context.Response[0].ShouldBeAssignableTo<IList<string>>()!;
                context.Response[0] = new List<string>(result) { message };
            }

            if (context.Response.Stream != null)
            {
                context.Response.Stream = ExtendStream(context.Response.Stream, message);
            }
        }

        private object ExtendStream(object stream, string message)
        {
            if (stream is IAsyncEnumerable<string> list)
            {
                return ExtendStream(list, message);
            }

            return stream;
        }

        private async IAsyncEnumerable<string> ExtendStream(IAsyncEnumerable<string> stream, string message)
        {
            await foreach (var item in stream)
            {
                yield return item;
            }

            yield return message;
        }
    }
}