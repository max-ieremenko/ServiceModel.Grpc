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
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Descriptions;

internal readonly ref struct OperationDescriptionBuilder<TType>
{
    private readonly IMethodInfo<TType> _method;
    private readonly string _serviceName;
    private readonly string _operationName;
    private readonly IReflect<TType> _reflect;

    public OperationDescriptionBuilder(IMethodInfo<TType> method, string serviceName, string operationName, IReflect<TType> reflect)
    {
        _method = method;
        _serviceName = serviceName;
        _operationName = operationName;
        _reflect = reflect;
    }

    public bool TryBuild([NotNullWhen(true)] out OperationDescription<TType>? operation, [NotNullWhen(false)] out string? error)
    {
        operation = null;
        if (!ValidateSignature())
        {
            error = BuildError();
            return false;
        }

        if (!TryCreateResponseType(
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
        operation = new OperationDescription<TType>(
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
            operationType,
            _reflect.IsTaskOrValueTask(_method.ReturnType));
        return true;
    }

    private static MessageDescription<TType> CreateMessage(params TType[] properties) => new(properties);

    private bool IsContextParameter(TType type) =>
        _reflect.IsAssignableFrom(type, typeof(ServerCallContext))
        || _reflect.IsAssignableFrom(type, typeof(CancellationToken))
        || _reflect.IsAssignableFrom(type, typeof(CancellationToken?))
        || _reflect.IsAssignableFrom(type, typeof(CallContext))
        || _reflect.IsAssignableFrom(type, typeof(CallOptions))
        || _reflect.IsAssignableFrom(type, typeof(CallOptions?));

    private bool IsDataParameter(TType type) =>
        !_reflect.IsTaskOrValueTask(type)
        && !IsContextParameter(type)
        && !_reflect.IsAssignableFrom(type, typeof(Stream));

    private bool TryCreateResponseType(
        out MessageDescription<TType> responseType,
        out int responseTypeIndex,
        out MessageDescription<TType>? headerType,
        out int[] headerIndexes,
        out string? errorDetails)
    {
        responseType = MessageDescription<TType>.Empty;
        responseTypeIndex = 0;
        headerType = null;
        headerIndexes = [];
        errorDetails = null;

        if (_reflect.IsAssignableFrom(_method.ReturnType, typeof(void)))
        {
            return true;
        }

        var actualReturnType = _method.ReturnType;
        if (_reflect.IsTaskOrValueTask(actualReturnType))
        {
            var genericArguments = _reflect.GenericTypeArguments(actualReturnType);
            if (genericArguments.Length == 0)
            {
                return true;
            }

            actualReturnType = genericArguments[0];
        }

        var actualReturnTypeArguments = _reflect.GenericTypeArguments(actualReturnType);
        if (_reflect.IsValueTuple(actualReturnType) && _reflect.ContainsAsyncEnumerable(actualReturnTypeArguments))
        {
            if (!_reflect.IsTaskOrValueTask(_method.ReturnType))
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
            var headerTypes = new List<TType>();
            for (var i = 0; i < actualReturnTypeArguments.Length; i++)
            {
                var genericArgument = actualReturnTypeArguments[i];
                if (_reflect.IsAsyncEnumerable(genericArgument))
                {
                    actualReturnType = _reflect.GenericTypeArguments(genericArgument)[0];
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

        if (_reflect.IsAsyncEnumerable(actualReturnType))
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
        IParameterInfo<TType>[] parameters,
        out MessageDescription<TType> requestType,
        out int[] dataIndexes,
        out MessageDescription<TType>? headerType,
        out int[] headerIndexes)
    {
        requestType = MessageDescription<TType>.Empty;
        dataIndexes = [];
        headerType = null;
        headerIndexes = [];

        if (parameters.Length == 0)
        {
            return true;
        }

        var dataParameters = new List<TType>();
        var dataParameterIndexes = new List<int>();
        var streamingIndex = -1;

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (IsDataParameter(parameter.Type))
            {
                if (_reflect.IsAsyncEnumerable(parameter.Type))
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
            requestType = CreateMessage(_reflect.GenericTypeArguments(parameters[streamingIndex].Type)[0]);
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

    private MethodType GetOperationType(IParameterInfo<TType>[] parameters, TType returnType)
    {
        if (_reflect.IsTaskOrValueTask(returnType))
        {
            var args = _reflect.GenericTypeArguments(returnType);
            returnType = args.Length == 0 ? returnType : args[0];
        }

        var responseIsStreaming = _reflect.IsAsyncEnumerable(returnType)
                                  || (_reflect.IsValueTuple(returnType) && _reflect.ContainsAsyncEnumerable(_reflect.GenericTypeArguments(returnType)));

        var requestIsStreaming = _reflect.ContainsAsyncEnumerable(parameters);
        if (responseIsStreaming)
        {
            return requestIsStreaming ? MethodType.DuplexStreaming : MethodType.ServerStreaming;
        }

        return requestIsStreaming ? MethodType.ClientStreaming : MethodType.Unary;
    }

    private int[] GetContextInput(IParameterInfo<TType>[] parameters)
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
        if (_method.HasGenericArguments())
        {
            return false;
        }

        var hasInputStreaming = false;

        for (var i = 0; i < _method.Parameters.Length; i++)
        {
            var parameter = _method.Parameters[i];

            if (parameter.IsRefOrOut())
            {
                return false;
            }

            if (IsDataParameter(parameter.Type))
            {
                if (_reflect.IsAsyncEnumerable(parameter.Type))
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
            .AppendFormat("Method signature [{0}] is not supported.", _reflect.GetSignature(_method));

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            message.Append(' ').Append(additionalInfo);
        }

        return message.ToString();
    }
}