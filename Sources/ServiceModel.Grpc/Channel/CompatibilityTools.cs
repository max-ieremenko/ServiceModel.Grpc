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

using System.Runtime.CompilerServices;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Channel;

internal static class CompatibilityTools
{
    internal const string HeaderNameMethodInput = $"smgrpc-method-input{Metadata.BinaryHeaderSuffix}";
    internal const string HeaderNameMethodOutput = $"smgrpc-method-output{Metadata.BinaryHeaderSuffix}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Metadata SerializeMethodInputHeader<T>(Marshaller<T> marshaller, T value) =>
        new()
        {
            { HeaderNameMethodInput, MarshallerExtensions.Serialize(marshaller, value) }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DeserializeMethodInputHeader<T>(Marshaller<T> marshaller, Metadata? requestHeaders) =>
        DeserializeHeader(marshaller, requestHeaders, HeaderNameMethodInput);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Metadata SerializeMethodOutputHeader<T>(Marshaller<T> marshaller, T value) =>
        new()
        {
            { HeaderNameMethodOutput, MarshallerExtensions.Serialize(marshaller, value) }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DeserializeMethodOutputHeader<T>(Marshaller<T> marshaller, Metadata? responseHeaders) =>
        DeserializeHeader(marshaller, responseHeaders, HeaderNameMethodOutput);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsMethodOutputHeader(Metadata? responseHeaders) =>
        MetadataExtensions.TryFindHeader(responseHeaders, HeaderNameMethodOutput, true, out _);

    private static T DeserializeHeader<T>(Marshaller<T> marshaller, Metadata? headers, string headerName)
    {
        if (!MetadataExtensions.TryFindHeader(headers, headerName, true, out var header))
        {
            throw new InvalidOperationException($"Fail to resolve header parameters, {headerName} header not found.");
        }

        return MarshallerExtensions.Deserialize(marshaller, header.ValueBytes);
    }
}