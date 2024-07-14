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
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class MessageAccessorExtensionsTest
{
    private Mock<IMessageAccessor> _messageAccessor = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _messageAccessor = new Mock<IMessageAccessor>(MockBehavior.Strict);
        _messageAccessor
            .SetupGet(a => a.Names)
            .Returns(["p1", "p2"]);
    }

    [Test]
    [TestCase(0, "p1", "value 1")]
    [TestCase(1, "p2", 10)]
    public void SetValue(int index, string name, object expected)
    {
        var message = new object();
        _messageAccessor
            .Setup(a => a.SetValue(message, index, expected));

        _messageAccessor.Object.SetValue(message, name, expected);

        _messageAccessor.VerifyAll();
    }

    [Test]
    [TestCase(0, "p1", "value 1")]
    [TestCase(1, "p2", 10)]
    public void GetValue(int index, string name, object expected)
    {
        var message = new object();
        _messageAccessor
            .Setup(a => a.GetValue(message, index))
            .Returns(expected);

        _messageAccessor.Object.GetValue(message, name).ShouldBe(expected);

        _messageAccessor.VerifyAll();
    }
}