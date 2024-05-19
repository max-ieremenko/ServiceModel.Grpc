// <copyright>
// Copyright 2024 Max Ieremenko
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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Grpc.Core;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal readonly ref struct OperationDescriptionBuilder
{
    private readonly IMethodSymbol _method;
    private readonly string _serviceName;
    private readonly string _operationName;

    public OperationDescriptionBuilder(IMethodSymbol method, string serviceName, string operationName)
    {
        _method = method;
        _serviceName = serviceName;
        _operationName = operationName;
    }

    public bool TryBuild([NotNullWhen(true)] out OperationDescription? operation, [NotNullWhen(false)] out string? error)
    {
        operation = null;
        if (!ValidateSignature())
        {
            error = BuildError();
            return false;
        }

        if (!TryCreateResponseType(
                _method.ReturnType,
                out var responseType,
                out var responseTypeIndex,
                out var headerResponseType,
                out var headerResponseTypeInput,
                out var errorDetails))
        {
            error = BuildError(errorDetails);
            return false;
        }

        if (!TryCreateRequestType(
                _method.Parameters,
                out var requestType,
                out var requestTypeInput,
                out var headerRequestType,
                out var headerRequestTypeInput))
        {
            error = BuildError();
            return false;
        }

        var operationType = GetOperationType(_method.Parameters, _method.ReturnType);
        var contextInput = GetContextInput(_method.Parameters);

        error = null;
        operation = new OperationDescription(
            _method,
            _serviceName,
            _operationName,
            responseType,
            responseTypeIndex,
            headerResponseType,
            headerResponseTypeInput,
            requestType,
            requestTypeInput,
            headerRequestType,
            headerRequestTypeInput,
            contextInput,
            operationType);
        return true;
    }

    private static bool IsContextParameter(ITypeSymbol type) =>
        type.IsAssignableFrom(typeof(ServerCallContext))
        || type.Is(typeof(CancellationToken))
        || type.Is(typeof(CancellationToken?))
        || type.Is(typeof(CallContext))
        || type.Is(typeof(CallOptions))
        || type.Is(typeof(CallOptions?));

    private static bool IsDataParameter(ITypeSymbol type) =>
        !SyntaxTools.IsTask(type)
        && !IsContextParameter(type)
        && !SyntaxTools.IsStream(type);

    private static MessageDescription CreateMessage(params ITypeSymbol[] properties) => new(properties);

    private static bool TryCreateResponseType(
        ITypeSymbol returnType,
        out MessageDescription responseType,
        out int responseTypeIndex,
        out MessageDescription? headerType,
        out int[] headerIndexes,
        out string? errorDetails)
    {
        responseType = MessageDescription.Empty;
        responseTypeIndex = 0;
        headerType = null;
        headerIndexes = [];
        errorDetails = null;

        if (SyntaxTools.IsVoid(returnType))
        {
            return true;
        }

        var actualReturnType = returnType;
        if (SyntaxTools.IsTask(returnType))
        {
            var genericArguments = returnType.GenericTypeArguments();
            if (genericArguments.IsEmpty)
            {
                return true;
            }

            actualReturnType = genericArguments[0];
        }

        var actualReturnTypeArguments = actualReturnType.GenericTypeArguments();
        if (SyntaxTools.IsValueTuple(actualReturnType) && actualReturnTypeArguments.Any(SyntaxTools.IsAsyncEnumerable))
        {
            if (!SyntaxTools.IsTask(returnType))
            {
                errorDetails = "Wrap return type with Task<> or ValueTask<>.";
                return false;
            }

            if (actualReturnTypeArguments.Length == 1)
            {
                errorDetails = "Unwrap return type from ValueTuple<>.";
                return false;
            }

            var streamIndex = -1;
            var headerIndexList = new List<int>();
            var headerTypes = new List<ITypeSymbol>();
            for (var i = 0; i < actualReturnTypeArguments.Length; i++)
            {
                var genericArgument = actualReturnTypeArguments[i];
                if (SyntaxTools.IsAsyncEnumerable(genericArgument))
                {
                    actualReturnType = genericArgument.GenericTypeArguments()[0];
                    if (streamIndex >= 0 || IsContextParameter(actualReturnType) || !IsDataParameter(actualReturnType))
                    {
                        return false;
                    }

                    streamIndex = i;
                }
                else if (IsContextParameter(genericArgument) || !IsDataParameter(genericArgument))
                {
                    return false;
                }
                else
                {
                    headerIndexList.Add(i);
                    headerTypes.Add(genericArgument);
                }
            }

            responseType = CreateMessage(actualReturnType);
            responseTypeIndex = streamIndex;
            headerType = CreateMessage(headerTypes.ToArray());
            headerIndexes = headerIndexList.ToArray();
            return true;
        }

        if (SyntaxTools.IsAsyncEnumerable(actualReturnType))
        {
            actualReturnType = actualReturnTypeArguments[0];
        }

        if (IsContextParameter(actualReturnType) || !IsDataParameter(actualReturnType))
        {
            return false;
        }

        responseType = CreateMessage(actualReturnType);
        return true;
    }

    private bool TryCreateRequestType(
        ImmutableArray<IParameterSymbol> parameters,
        out MessageDescription requestType,
        out int[] dataIndexes,
        out MessageDescription? headerType,
        out int[] headerIndexes)
    {
        requestType = MessageDescription.Empty;
        dataIndexes = [];
        headerType = null;
        headerIndexes = [];

        if (parameters.Length == 0)
        {
            return true;
        }

        var dataParameters = new List<ITypeSymbol>();
        var dataParameterIndexes = new List<int>();
        var streamingIndex = -1;

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (IsDataParameter(parameter.Type))
            {
                if (SyntaxTools.IsAsyncEnumerable(parameter.Type))
                {
                    streamingIndex = i;
                }
                else
                {
                    dataParameters.Add(parameter.Type);
                    dataParameterIndexes.Add(i);
                }
            }
        }

        if (streamingIndex >= 0)
        {
            requestType = CreateMessage(parameters[streamingIndex].Type.GenericTypeArguments()[0]);
            if (dataParameters.Count > 0)
            {
                headerType = CreateMessage(dataParameters.ToArray());
            }

            dataIndexes = [streamingIndex];
            headerIndexes = dataParameterIndexes.ToArray();
            return true;
        }

        requestType = CreateMessage(dataParameters.ToArray());
        dataIndexes = dataParameterIndexes.ToArray();
        return true;
    }

    private MethodType GetOperationType(ImmutableArray<IParameterSymbol> parameters, ITypeSymbol returnType)
    {
        if (SyntaxTools.IsTask(returnType))
        {
            var args = returnType.GenericTypeArguments();
            returnType = args.IsEmpty ? returnType : args[0];
        }

        var responseIsStreaming = SyntaxTools.IsAsyncEnumerable(returnType)
                                  || (SyntaxTools.IsValueTuple(returnType) && returnType.GenericTypeArguments().Any(SyntaxTools.IsAsyncEnumerable));

        var requestIsStreaming = parameters.Select(i => i.Type).Any(SyntaxTools.IsAsyncEnumerable);
        if (responseIsStreaming)
        {
            return requestIsStreaming ? MethodType.DuplexStreaming : MethodType.ServerStreaming;
        }

        return requestIsStreaming ? MethodType.ClientStreaming : MethodType.Unary;
    }

    private int[] GetContextInput(ImmutableArray<IParameterSymbol> parameters)
    {
        if (parameters.Length == 0)
        {
            return [];
        }

        var indexes = new List<int>(1);

        for (var i = 0; i < parameters.Length; i++)
        {
            if (IsContextParameter(parameters[i].Type))
            {
                indexes.Add(i);
            }
        }

        return indexes.Count == 0 ? [] : indexes.ToArray();
    }

    private bool ValidateSignature()
    {
        if (_method.TypeArguments.Length != 0)
        {
            return false;
        }

        var hasInputStreaming = false;

        for (var i = 0; i < _method.Parameters.Length; i++)
        {
            var parameter = _method.Parameters[i];

            if (parameter.IsOut() || parameter.IsRef())
            {
                return false;
            }

            if (IsDataParameter(parameter.Type))
            {
                if (SyntaxTools.IsAsyncEnumerable(parameter.Type))
                {
                    if (hasInputStreaming)
                    {
                        return false;
                    }

                    hasInputStreaming = true;
                }
            }
            else if (!IsContextParameter(parameter.Type))
            {
                return false;
            }
        }

        return true;
    }

    private string BuildError(string? additionalInfo = null)
    {
        var message = new StringBuilder()
            .AppendFormat("Method signature [{0}] is not supported.", SyntaxTools.GetSignature(_method));

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            message.Append(' ').Append(additionalInfo);
        }

        return message.ToString();
    }
}