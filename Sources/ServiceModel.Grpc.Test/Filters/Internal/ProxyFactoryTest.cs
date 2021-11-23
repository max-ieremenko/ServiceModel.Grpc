// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal
{
    [TestFixture]
    public class ProxyFactoryTest
    {
        [Test]
        public void CreateCase1Proxy()
        {
            var contractMethodDefinition = typeof(TestContract).InstanceMethod(nameof(TestContract.Case1));

            var actual = new ProxyFactory(contractMethodDefinition);

            actual.RequestProxy.Names.ShouldBeEmpty();
            actual.RequestProxy.CreateDefault().ShouldBeOfType<Message>();

            actual.ResponseProxy.Names.ShouldBeEmpty();
            actual.ResponseProxy.CreateDefault().ShouldBeOfType<Message>();

            actual.RequestStreamProxy.ShouldBeNull();

            actual.ResponseStreamProxy.ShouldBeNull();
        }

        [Test]
        public void CreateCase2Proxy()
        {
            var contractMethodDefinition = typeof(TestContract).InstanceMethod(nameof(TestContract.Case2));

            var actual = new ProxyFactory(contractMethodDefinition);

            actual.RequestProxy.Names.ShouldBe(new[] { "x", "y" });
            actual.RequestProxy.CreateDefault().ShouldBeOfType<Message<string, int>>();

            actual.ResponseProxy.Names.ShouldBe(MessageProxy.UnaryResultNames);
            actual.ResponseProxy.CreateDefault().ShouldBeOfType<Message<double>>();

            actual.RequestStreamProxy.ShouldBeNull();

            actual.ResponseStreamProxy.ShouldBeNull();
        }

        [Test]
        public void CreateCase3Proxy()
        {
            var contractMethodDefinition = typeof(TestContract).InstanceMethod(nameof(TestContract.Case3));

            var actual = new ProxyFactory(contractMethodDefinition);

            actual.RequestProxy.Names.ShouldBeEmpty();
            actual.RequestProxy.CreateDefault().ShouldBeOfType<Message>();

            actual.ResponseProxy.Names.ShouldBeEmpty();
            actual.ResponseProxy.CreateDefault().ShouldBeOfType<Message>();

            actual.RequestStreamProxy.ShouldNotBeNull();
            actual.RequestStreamProxy.CreateDefault().ShouldBeAssignableTo<IAsyncEnumerable<int>>();

            actual.ResponseStreamProxy.ShouldNotBeNull();
            actual.ResponseStreamProxy.CreateDefault().ShouldBeAssignableTo<IAsyncEnumerable<double>>();
        }

        [Test]
        public void CreateCase4Proxy()
        {
            var contractMethodDefinition = typeof(TestContract).InstanceMethod(nameof(TestContract.Case4));

            var actual = new ProxyFactory(contractMethodDefinition);

            actual.RequestProxy.Names.ShouldBe(new[] { "x", "y" });
            actual.RequestProxy.CreateDefault().ShouldBeOfType<Message<double, decimal>>();

            actual.ResponseProxy.Names.ShouldBe(new[] { "R1", "R2" });
            actual.ResponseProxy.CreateDefault().ShouldBeOfType<Message<int, string>>();

            actual.RequestStreamProxy.ShouldNotBeNull();
            actual.RequestStreamProxy.CreateDefault().ShouldBeAssignableTo<IAsyncEnumerable<int>>();

            actual.ResponseStreamProxy.ShouldNotBeNull();
            actual.ResponseStreamProxy.CreateDefault().ShouldBeAssignableTo<IAsyncEnumerable<double>>();
        }

        private sealed class TestContract
        {
            public void Case1() => throw new NotImplementedException();

            public double Case2(string x, CancellationToken token, int y) => throw new NotImplementedException();

            public IAsyncEnumerable<double> Case3(IAsyncEnumerable<int> stream) => throw new NotImplementedException();

            public ValueTask<(IAsyncEnumerable<double> Stream, int R1, string R2)> Case4(IAsyncEnumerable<int> stream, double x, decimal y) => throw new NotImplementedException();
        }
    }
}
