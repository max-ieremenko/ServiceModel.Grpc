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

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

#pragma warning disable SA1414 // Tuple types in signatures should have element names

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer
{
    public partial class ApiDescriptionGeneratorTest
    {
        [ServiceContract]
        private sealed class TestCases
        {
            [OperationContract]
            [RequestMetadata(new int[0], new int[0])]
            [ResponseMetadata(null, new Type[0], new string[0])]
            public void Void() => throw new NotSupportedException();

            [OperationContract]
            [RequestMetadata(new int[0], new int[0])]
            [ResponseMetadata(typeof(IAsyncEnumerable<string>), new[] { typeof(string), typeof(int) }, new[] { "Value1", "Value2" })]
            public Task<(string Value1, IAsyncEnumerable<string> Stream, int Value2)> ServerStreamingWithHeaders() => throw new NotSupportedException();

            [OperationContract]
            [RequestMetadata(new[] { 0 }, new int[0])]
            [ResponseMetadata(typeof(IAsyncEnumerable<string>), new[] { typeof(string), typeof(int) }, new[] { "Value1", "Item2" })]
            public Task<(string Value1, IAsyncEnumerable<string>, int)> ServerStreamingWithMixedHeaderNames(string data) => throw new NotSupportedException();

            [OperationContract]
            [RequestMetadata(new[] { 0 }, new[] { 1 })]
            [ResponseMetadata(typeof(IAsyncEnumerable<string>), new[] { typeof(string), typeof(int) }, new[] { "Item1", "Item2" })]
            public Task<(string, IAsyncEnumerable<string>, int)> DuplexStreamingWithHeaders(IAsyncEnumerable<int> data1, string data2) => throw new NotSupportedException();
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class ResponseMetadataAttribute : Attribute
        {
            public ResponseMetadataAttribute(Type? type, Type[] headerTypes, string[] headerNames)
            {
                Type = type;
                HeaderTypes = headerTypes;
                HeaderNames = headerNames;
            }

            public Type? Type { get; }

            public Type[] HeaderTypes { get; }

            public string[] HeaderNames { get; }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class RequestMetadataAttribute : Attribute
        {
            public RequestMetadataAttribute(int[] parameters, int[] headerParameters)
            {
                Parameters = parameters;
                HeaderParameters = headerParameters;
            }

            public int[] Parameters { get; }

            public int[] HeaderParameters { get; }
        }
    }
}
