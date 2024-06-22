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
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal;

[TestFixture]
public class FiltersReflectStreamAccessorTest
{
    private FiltersReflectStreamAccessor _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new FiltersReflectStreamAccessor(typeof(int));
    }

    [Test]
    public async Task CreateEmpty()
    {
        var actual = _sut.CreateEmpty();

        var stream = actual.ShouldBeAssignableTo<IAsyncEnumerable<int>>()!;
        var items = await stream.ToListAsync().ConfigureAwait(false);
        items.ShouldBeEmpty();
    }

    [Test]
    public void Validate()
    {
        var value = new[] { 1 }.AsAsyncEnumerable();

        _sut.Validate(value);
    }

    [Test]
    public void ValidateInvalid()
    {
        var value = new[] { 1.0 }.AsAsyncEnumerable();

        Assert.Throws<InvalidCastException>(() => _sut.Validate(value));
    }
}