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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal
{
    [TestFixture]
    public class ResponseContextTest
    {
        [Test]
        public void CreateResponseByUserRequest()
        {
            var sut = new ResponseContext(new MessageProxy(new[] { "p1" }, typeof(Message<int>)), new StreamProxy(typeof(string)));

            sut["p1"].ShouldBe(0);
            sut.Stream.ShouldBeAssignableTo<IAsyncEnumerable<string>>();
        }

        [Test]
        public void CreateResponseByHandlerRequest()
        {
            var sut = new ResponseContext(new MessageProxy(new[] { "p1" }, typeof(Message<int>)), new StreamProxy(typeof(string)));

            var (response, stream) = sut.GetRaw();

            response.ShouldBeOfType<Message<int>>();
            stream.ShouldBeAssignableTo<IAsyncEnumerable<string>>();
        }

        [Test]
        public void ListProperties()
        {
            var sut = new ResponseContext(new MessageProxy(new[] { "p1", "p2" }, typeof(Message<int, string>)), null);

            var actual = sut.ToArray();

            actual.Length.ShouldBe(2);
            actual[0].Key.ShouldBe("p1");
            actual[0].Value.ShouldBe(0);
            actual[1].Key.ShouldBe("p2");
            actual[1].Value.ShouldBeNull();
        }

        [Test]
        public void ResponseIsProvided()
        {
            var sut = new ResponseContext(new MessageProxy(new[] { "p1" }, typeof(Message<int>)), null);

            sut.IsProvided.ShouldBeFalse();

            sut.SetRaw(new Message<int>(), null);

            sut.IsProvided.ShouldBeTrue();
        }
    }
}
