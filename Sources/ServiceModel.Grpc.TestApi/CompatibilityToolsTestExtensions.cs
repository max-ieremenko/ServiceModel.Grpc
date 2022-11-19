// <copyright>
// Copyright 2020 Max Ieremenko
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
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.TestApi;

public static class CompatibilityToolsTestExtensions
{
    public static Metadata SerializeMethodInput<T>(IMarshallerFactory marshallerFactory, T value)
    {
        return CompatibilityTools.SerializeMethodInputHeader(marshallerFactory.CreateMarshaller<Message<T>>(), new Message<T>(value));
    }

    public static Metadata SerializeMethodInput<T1, T2>(IMarshallerFactory marshallerFactory, T1 value1, T2 value2)
    {
        return CompatibilityTools.SerializeMethodInputHeader(marshallerFactory.CreateMarshaller<Message<T1, T2>>(), new Message<T1, T2>(value1, value2));
    }

    public static T DeserializeMethodInput<T>(IMarshallerFactory marshallerFactory, Metadata? requestHeaders)
    {
        var message = CompatibilityTools.DeserializeMethodInputHeader(marshallerFactory.CreateMarshaller<Message<T>>(), requestHeaders);
        return message.Value1;
    }

    public static (T1 Value1, T2 Value2) DeserializeMethodInput<T1, T2>(IMarshallerFactory marshallerFactory, Metadata? requestHeaders)
    {
        var message = CompatibilityTools.DeserializeMethodInputHeader(marshallerFactory.CreateMarshaller<Message<T1, T2>>(), requestHeaders);
        return (message.Value1, message.Value2);
    }

    public static Metadata SerializeMethodOutput<T>(IMarshallerFactory marshallerFactory, T value)
    {
        return CompatibilityTools.SerializeMethodOutputHeader(marshallerFactory.CreateMarshaller<Message<T>>(), new Message<T>(value));
    }

    public static Metadata SerializeMethodOutput<T1, T2>(IMarshallerFactory marshallerFactory, T1 value1, T2 value2)
    {
        return CompatibilityTools.SerializeMethodOutputHeader(marshallerFactory.CreateMarshaller<Message<T1, T2>>(), new Message<T1, T2>(value1, value2));
    }

    public static T DeserializeMethodOutput<T>(IMarshallerFactory marshallerFactory, Metadata requestHeaders)
    {
        var message = CompatibilityTools.DeserializeMethodOutputHeader(marshallerFactory.CreateMarshaller<Message<T>>(), requestHeaders);
        return message.Value1;
    }

    public static (T1 Value1, T2 Value2) DeserializeMethodOutput<T1, T2>(IMarshallerFactory marshallerFactory, Metadata requestHeaders)
    {
        var message = CompatibilityTools.DeserializeMethodOutputHeader(marshallerFactory.CreateMarshaller<Message<T1, T2>>(), requestHeaders);
        return (message.Value1, message.Value2);
    }
}