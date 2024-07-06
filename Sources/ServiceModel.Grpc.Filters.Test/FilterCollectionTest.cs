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
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Filters;

[TestFixture]
public class FilterCollectionTest
{
    private FilterCollection<IDisposable> _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new FilterCollection<IDisposable>();
    }

    [Test]
    public void AddFactory()
    {
        Func<IServiceProvider, SomeFilter> factory = _ => throw new NotImplementedException();

        _sut.Add(10, factory);

        _sut.Count.ShouldBe(1);
        _sut[0].Factory.ShouldBe(factory);
        _sut[0].Order.ShouldBe(10);
    }

    [Test]
    public void AddInstance()
    {
        var filter = new SomeFilter();
        _sut.Add(10, filter);

        _sut.Count.ShouldBe(1);
        _sut[0].Factory(null!).ShouldBe(filter);
        _sut[0].Order.ShouldBe(10);
    }

    private sealed class SomeFilter : IDisposable
    {
        public void Dispose() => throw new NotImplementedException();
    }
}