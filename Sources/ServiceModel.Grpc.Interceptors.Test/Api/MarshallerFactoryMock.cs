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
using System.Text;
using Grpc.Core;
using Moq;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Interceptors.Api;

internal sealed class MarshallerFactoryMock
{
    private readonly Mock<IMarshallerFactory> _factory = new(MockBehavior.Strict);

    public IMarshallerFactory Factory => _factory.Object;

    public void SetupString()
    {
        _factory
            .Setup(f => f.CreateMarshaller<string>())
            .Returns(new Marshaller<string>(Serialize, Deserialize));
    }

    public void Throws<TValue, TException>()
        where TException : Exception, new()
    {
        _factory
            .Setup(f => f.CreateMarshaller<TValue>())
            .Throws<TException>();
    }

    private static void Serialize(string value, SerializationContext context) => context.Complete(Encoding.UTF8.GetBytes(value));

    private static string Deserialize(DeserializationContext context) => Encoding.UTF8.GetString(context.PayloadAsNewBuffer());
}