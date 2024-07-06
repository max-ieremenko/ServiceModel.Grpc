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

using NUnit.Framework;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[TestFixture]
public class ReflectOperationDescriptorTest
{
    [Test]
    public void CreateCase1Proxy()
    {
        var sut = new ReflectOperationDescriptor(() => typeof(TestContract).InstanceMethod(nameof(TestContract.Case1)));

        sut.GetRequestAccessor().ShouldNotBeNull();
        sut.GetRequestAccessor().Names.ShouldBeEmpty();
        sut.GetRequestAccessor().CreateNew().ShouldBeOfType<Message>();

        sut.GetResponseAccessor().ShouldNotBeNull();
        sut.GetResponseAccessor().Names.ShouldBeEmpty();
        sut.GetResponseAccessor().CreateNew().ShouldBeOfType<Message>();

        sut.GetRequestStreamAccessor().ShouldBeNull();
        sut.GetResponseStreamAccessor().ShouldBeNull();

        sut.GetRequestHeaderParameters().ShouldBeEmpty();
        sut.GetRequestParameters().ShouldBeEmpty();
    }

    [Test]
    public void CreateCase2Proxy()
    {
        var sut = new ReflectOperationDescriptor(() => typeof(TestContract).InstanceMethod(nameof(TestContract.Case2)));

        sut.GetRequestAccessor().ShouldNotBeNull();
        sut.GetRequestAccessor().Names.ShouldBe(["x", "y"]);
        sut.GetRequestAccessor().CreateNew().ShouldBeOfType<Message<string, int>>();

        sut.GetResponseAccessor().ShouldNotBeNull();
        sut.GetResponseAccessor().Names.ShouldBe(ReflectOperationDescriptor.UnaryResultNames);
        sut.GetResponseAccessor().CreateNew().ShouldBeOfType<Message<double>>();

        sut.GetRequestStreamAccessor().ShouldBeNull();
        sut.GetResponseStreamAccessor().ShouldBeNull();

        sut.GetRequestHeaderParameters().ShouldBeEmpty();
        sut.GetRequestParameters().ShouldBe([0, 2]);
    }

    [Test]
    public void CreateCase3Proxy()
    {
        var sut = new ReflectOperationDescriptor(() => typeof(TestContract).InstanceMethod(nameof(TestContract.Case3)));

        sut.GetRequestAccessor().ShouldNotBeNull();
        sut.GetRequestAccessor().Names.ShouldBeEmpty();
        sut.GetRequestAccessor().CreateNew().ShouldBeOfType<Message>();

        sut.GetResponseAccessor().ShouldNotBeNull();
        sut.GetResponseAccessor().Names.ShouldBeEmpty();
        sut.GetResponseAccessor().CreateNew().ShouldBeOfType<Message>();

        sut
            .GetRequestStreamAccessor()
            .ShouldNotBeNull()
            .CreateEmpty()
            .ShouldBeAssignableTo<IAsyncEnumerable<int>>();

        sut
            .GetResponseStreamAccessor()
            .ShouldNotBeNull()
            .CreateEmpty()
            .ShouldBeAssignableTo<IAsyncEnumerable<double>>();

        sut.GetRequestHeaderParameters().ShouldBeEmpty();
        sut.GetRequestParameters().ShouldBe([0]);
    }

    [Test]
    public void CreateCase4Proxy()
    {
        var sut = new ReflectOperationDescriptor(() => typeof(TestContract).InstanceMethod(nameof(TestContract.Case4)));

        sut.GetRequestAccessor().ShouldNotBeNull();
        sut.GetRequestAccessor().Names.ShouldBe(["x", "y"]);
        sut.GetRequestAccessor().CreateNew().ShouldBeOfType<Message<double, decimal>>();

        sut.GetResponseAccessor().ShouldNotBeNull();
        sut.GetResponseAccessor().Names.ShouldBe(["R1", "R2"]);
        sut.GetResponseAccessor().CreateNew().ShouldBeOfType<Message<int, string>>();

        sut
            .GetRequestStreamAccessor()
            .ShouldNotBeNull()
            .CreateEmpty()
            .ShouldBeAssignableTo<IAsyncEnumerable<int>>();

        sut
            .GetResponseStreamAccessor()
            .ShouldNotBeNull()
            .CreateEmpty()
            .ShouldBeAssignableTo<IAsyncEnumerable<double>>();

        sut.GetRequestHeaderParameters().ShouldBe([1, 2]);
        sut.GetRequestParameters().ShouldBe([0]);
    }

    private sealed class TestContract
    {
        public void Case1() => throw new NotImplementedException();

        public double Case2(string x, CancellationToken token, int y) => throw new NotImplementedException();

        public IAsyncEnumerable<double> Case3(IAsyncEnumerable<int> stream) => throw new NotImplementedException();

        public ValueTask<(IAsyncEnumerable<double> Stream, int R1, string R2)> Case4(IAsyncEnumerable<int> stream, double x, decimal y) => throw new NotImplementedException();
    }
}