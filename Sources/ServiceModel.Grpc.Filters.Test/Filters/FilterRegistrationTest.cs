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

namespace ServiceModel.Grpc.Filters;

[TestFixture]
public class FilterRegistrationTest
{
    [Test]
    [TestCase(1, 1, 0)]
    [TestCase(2, 1, 1)]
    [TestCase(1, 2, -1)]
    public void CompareTo(int xOrder, int yOrder, int expected)
    {
        var x = new FilterRegistration<IDisposable>(xOrder, _ => throw new NotImplementedException());
        var y = new FilterRegistration<IDisposable>(yOrder, _ => throw new NotImplementedException());

        var actual = x.CompareTo(y);
        if (expected == 0)
        {
            actual.ShouldBe(expected);
        }
        else if (expected > 0)
        {
            actual.ShouldBeGreaterThan(0);
        }
        else
        {
            actual.ShouldBeLessThan(0);
        }
    }
}