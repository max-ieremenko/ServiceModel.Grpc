// <copyright>
// Copyright 2020-2024 Max Ieremenko
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

using System.Diagnostics;
using Grpc.Core;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

[DebuggerDisplay("{OperationType} {OperationName}")]
internal sealed class OperationDescription : IOperationDescription
{
    public OperationDescription(
        IMethodSymbol method,
        string serviceName,
        string operationName,
        IMessageDescription responseType,
        int responseTypeIndex,
        IMessageDescription? headerResponseType,
        int[] headerResponseTypeInput,
        IMessageDescription requestType,
        int[] requestTypeInput,
        IMessageDescription? headerRequestType,
        int[] headerRequestTypeInput,
        int[] contextInput,
        MethodType operationType)
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
        IsAsync = SyntaxTools.IsTask(method.ReturnType);

        GrpcMethodName = "Method" + OperationName;
        GrpcMethodInputHeaderName = "MethodInputHeader" + OperationName;
        GrpcMethodOutputHeaderName = "MethodOutputHeader" + OperationName;
        ClrDefinitionMethodName = "Get" + OperationName + "Definition";
    }

    public IMethodSymbol Method { get; }

    public string ServiceName { get; }

    public string OperationName { get; }

    public IMessageDescription ResponseType { get; }

    public int ResponseTypeIndex { get; }

    public IMessageDescription? HeaderResponseType { get; }

    public int[] HeaderResponseTypeInput { get; }

    public IMessageDescription RequestType { get; }

    public int[] RequestTypeInput { get; }

    public IMessageDescription? HeaderRequestType { get; }

    public int[] HeaderRequestTypeInput { get; }

    public int[] ContextInput { get; }

    public MethodType OperationType { get; }

    public bool IsAsync { get; }

    public string GrpcMethodName { get; }

    public string GrpcMethodInputHeaderName { get; }

    public string GrpcMethodOutputHeaderName { get; }

    public string ClrDefinitionMethodName { get; set; }
}