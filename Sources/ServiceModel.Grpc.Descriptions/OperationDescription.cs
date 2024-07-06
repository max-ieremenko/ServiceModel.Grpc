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

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Grpc.Core;
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Descriptions;

[DebuggerDisplay("{OperationType} {OperationName}")]
internal sealed class OperationDescription<TType>
{
    public OperationDescription(
        IMethodInfo<TType> method,
        string serviceName,
        string operationName,
        MessageDescription<TType> responseType,
        int responseTypeIndex,
        MessageDescription<TType>? headerResponseType,
        int[] headerResponseTypeInput,
        MessageDescription<TType> requestType,
        int[] requestTypeInput,
        MessageDescription<TType>? headerRequestType,
        int[] headerRequestTypeInput,
        int[] contextInput,
        MethodType operationType,
        bool isAsync)
    {
        Method = method;
        ServiceName = serviceName;
        OperationName = operationName;
        ResponseType = responseType;
        ResponseTypeIndex = responseTypeIndex;
        HeaderResponseType = headerResponseType;
        HeaderResponseTypeInput = headerResponseTypeInput;
        RequestType = requestType;
        RequestTypeInput = requestTypeInput;
        HeaderRequestType = headerRequestType;
        HeaderRequestTypeInput = headerRequestTypeInput;
        ContextInput = contextInput;
        OperationType = operationType;
        IsAsync = isAsync;
    }

    public IMethodInfo<TType> Method { get; }

    public string ServiceName { get; }

    public string OperationName { get; }

    public MessageDescription<TType> ResponseType { get; }

    public int ResponseTypeIndex { get; }

    public MessageDescription<TType>? HeaderResponseType { get; }

    public int[] HeaderResponseTypeInput { get; }

    public MessageDescription<TType> RequestType { get; }

    public int[] RequestTypeInput { get; }

    public MessageDescription<TType>? HeaderRequestType { get; }

    public int[] HeaderRequestTypeInput { get; }

    public int[] ContextInput { get; }

    public MethodType OperationType { get; }

    public bool IsAsync { get; }

    public string[] GetResponseHeaderNames()
    {
        if (HeaderResponseTypeInput.Length == 0)
        {
            return [];
        }

        IList<string>? transformNames = null;
        if (Method.TryGetReturnParameterCustomAttribute($"System.Runtime.CompilerServices.{nameof(TupleElementNamesAttribute)}", out var attribute))
        {
            transformNames = (IList<string>?)attribute.GetPropertyValue(nameof(TupleElementNamesAttribute.TransformNames));
        }

        var result = new string[HeaderResponseTypeInput.Length];

        for (var i = 0; i < result.Length; i++)
        {
            var index = HeaderResponseTypeInput[i];

            string? name = null;
            if (transformNames != null)
            {
                name = transformNames[index];
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"Item{(i + 1).ToString(CultureInfo.InvariantCulture)}";
            }

            result[i] = name!;
        }

        return result;
    }
}