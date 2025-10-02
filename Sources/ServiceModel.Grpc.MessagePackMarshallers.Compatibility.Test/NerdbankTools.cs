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

using Nerdbank.MessagePack;
using PolyType;
using PolyType.SourceGenerator;

// Use an overload that does not take an ITypeShape<T> or ITypeShapeProvider
#pragma warning disable NBMsgPack051

namespace ServiceModel.Grpc;

internal static class NerdbankTools
{
    public static readonly MessagePackSerializer Serializer = new() { PerfOverSchemaStability = true };

    public static ITypeShapeProvider TypeShapeProvider => TypeShapeProvider_ServiceModel_Grpc_MessagePackMarshallers_Compatibility_Test.Default;

    public static byte[] Serialize<T>(T? value) => Serializer.Serialize(value, TypeShapeProvider);

    public static T? Deserialize<T>(byte[] payload) => Serializer.Deserialize<T>(payload, TypeShapeProvider);
}