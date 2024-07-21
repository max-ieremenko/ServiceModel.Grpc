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
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions.Reflection;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal sealed class OperationDescription : IOperationDescription
{
    private readonly OperationDescription<ITypeSymbol> _source;

    public OperationDescription(OperationDescription<ITypeSymbol> source)
    {
        _source = source;
        Method = ((CodeAnalysisMethodInfo)source.Method).Source;
        ServiceName = source.ServiceName;
        OperationName = source.OperationName;
        OperationType = source.OperationType;
        IsAsync = source.IsAsync;
        ResponseType = new MessageDescription(source.ResponseType);
        ResponseTypeIndex = source.ResponseTypeIndex;
        HeaderResponseType = source.HeaderResponseType == null ? null : new MessageDescription(source.HeaderResponseType);
        HeaderResponseTypeInput = source.HeaderResponseTypeInput;
        RequestType = new MessageDescription(source.RequestType);
        RequestTypeInput = source.RequestTypeInput;
        HeaderRequestType = source.HeaderRequestType == null ? null : new MessageDescription(source.HeaderRequestType);
        HeaderRequestTypeInput = source.HeaderRequestTypeInput;
        ContextInput = source.ContextInput;
    }

    public string ServiceName { get; }

    public string OperationName { get; }

    public IMethodSymbol Method { get; }

    public MethodType OperationType { get; }

    public bool IsAsync { get; }

    public IMessageDescription ResponseType { get; }

    public int ResponseTypeIndex { get; }

    public IMessageDescription? HeaderResponseType { get; }

    public int[] HeaderResponseTypeInput { get; }

    public IMessageDescription RequestType { get; }

    public int[] RequestTypeInput { get; }

    public IMessageDescription? HeaderRequestType { get; }

    public int[] HeaderRequestTypeInput { get; }

    public int[] ContextInput { get; }

    public string[] GetResponseHeaderNames() => _source.GetResponseHeaderNames();

    public (IMessageDescription Message, string[] Names) GetRequest()
    {
        var request = _source.GetRequest();
        return (new MessageDescription(request.Message), request.Names);
    }

    public (IMessageDescription Message, string[] Names) GetResponse()
    {
        var response = _source.GetResponse();
        return (new MessageDescription(response.Message), response.Names);
    }
}