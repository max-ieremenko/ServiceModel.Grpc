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

namespace ServiceModel.Grpc.Client.Internal;

[TestFixture]
public class GrpcMethodEqualityComparerTest
{
    [Test]
    public void EqualsByReference()
    {
        var method = new Mock<IMethod>(MockBehavior.Strict);

        GrpcMethodEqualityComparer.Instance.Equals(method.Object, method.Object).ShouldBeTrue();
    }

    [Test]
    public void EqualsByValue()
    {
        var method1 = new Mock<IMethod>(MockBehavior.Strict);
        method1
            .SetupGet(m => m.FullName)
            .Returns("the-method");

        var method2 = new Mock<IMethod>(MockBehavior.Strict);
        method2
            .SetupGet(m => m.FullName)
            .Returns("the-method");

        GrpcMethodEqualityComparer.Instance.Equals(method1.Object, method2.Object).ShouldBeTrue();
        GrpcMethodEqualityComparer.Instance.Equals(method2.Object, method1.Object).ShouldBeTrue();

        GrpcMethodEqualityComparer.Instance.GetHashCode(method1.Object).ShouldBe(GrpcMethodEqualityComparer.Instance.GetHashCode(method2.Object));
    }

    [Test]
    public void NotEquals()
    {
        var method1 = new Mock<IMethod>(MockBehavior.Strict);
        method1
            .SetupGet(m => m.FullName)
            .Returns("method1");

        var method2 = new Mock<IMethod>(MockBehavior.Strict);
        method2
            .SetupGet(m => m.FullName)
            .Returns("method2");

        GrpcMethodEqualityComparer.Instance.Equals(method1.Object, method2.Object).ShouldBeFalse();
        GrpcMethodEqualityComparer.Instance.Equals(method2.Object, method1.Object).ShouldBeFalse();
    }
}