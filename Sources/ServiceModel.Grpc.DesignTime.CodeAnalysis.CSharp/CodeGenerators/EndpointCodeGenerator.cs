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

using System.Globalization;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class EndpointCodeGenerator : ICodeGenerator
{
    private readonly IContractDescription _contract;

    public EndpointCodeGenerator(IContractDescription contract)
    {
        _contract = contract;
    }

    public string GetHintName() => Hints.Endpoints(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("internal sealed class ")
            .AppendLine(NamingContract.Endpoint.Class(_contract.BaseClassName));
        output.AppendLine("{");

        using (output.Indent())
        {
            foreach (var interfaceDescription in _contract.Services)
            {
                foreach (var operation in interfaceDescription.Operations)
                {
                    output.AppendLine();
                    ImplementMethod(output, interfaceDescription.InterfaceType, operation);
                }
            }
        }

        output.AppendLine("}");
    }

    private void ImplementMethod(ICodeStringBuilder output, ITypeSymbol interfaceType, IOperationDescription operation)
    {
        switch (operation.OperationType)
        {
            case MethodType.Unary:
                BuildUnary(output, interfaceType, operation);
                break;

            case MethodType.ClientStreaming:
                BuildClientStreaming(output, interfaceType, operation);
                break;

            case MethodType.ServerStreaming:
                BuildServerStreaming(output, interfaceType, operation);
                break;

            case MethodType.DuplexStreaming:
                BuildDuplexStreaming(output, interfaceType, operation);
                break;

            default:
                throw new NotImplementedException($"{operation.OperationType} operation is not implemented.");
        }
    }

    private void BuildUnary(ICodeStringBuilder output, ITypeSymbol interfaceType, IOperationDescription operation)
    {
        // Task<TResponse> Invoke(TService service, TRequest request, ServerCallContext context)
        output.Append("public ");
        if (operation.IsAsync)
        {
            output.Append("async ");
        }

        output
            .Append("Task<").WriteMessage(operation.ResponseType).Append("> ")
            .Append(operation.OperationName)
            .Append("(")
            .WriteType(interfaceType).Append(" service, ")
            .WriteMessage(operation.RequestType).Append(" request, ")
            .WriteType(typeof(ServerCallContext)).AppendLine(" context)")
            .AppendLine("{");

        using (output.Indent())
        {
            if (operation.ResponseType.Properties.Length > 0)
            {
                output.Append("var result = ");
            }

            if (operation.IsAsync)
            {
                output.Append("await ");
            }

            output
                .Append("service.")
                .Append(operation.Method.Name)
                .Append("(");

            var source = operation.Method;
            for (var i = 0; i < source.Parameters.Length; i++)
            {
                output.WriteCommaIf(i > 0);

                var parameter = source.Parameters[i];
                if (operation.ContextInput.Contains(i))
                {
                    PushContext(output, parameter);
                }
                else
                {
                    output
                        .Append("request.Value")
                        .Append((Array.IndexOf(operation.RequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                }
            }

            output.Append(")");
            if (operation.IsAsync)
            {
                output.Append(".ConfigureAwait(false)");
            }

            output
                .AppendLine(";")
                .Append("return ");

            if (!operation.IsAsync)
            {
                output.Append("Task.FromResult(");
            }

            output
                .Append("new ")
                .WriteMessage(operation.ResponseType)
                .Append("(");

            if (operation.ResponseType.Properties.Length > 0)
            {
                output.Append("result");
            }

            output.Append(")");

            if (!operation.IsAsync)
            {
                output.Append(")");
            }

            output.AppendLine(";");
        }

        output.AppendLine("}");
    }

    private void BuildClientStreaming(ICodeStringBuilder output, ITypeSymbol interfaceType, IOperationDescription operation)
    {
        // Task<TResponse> Invoke(TService service, TRequestHeader requestHeader, IAsyncEnumerable<TRequest> request, ServerCallContext context)
        output
            .Append("public async Task<").WriteMessage(operation.ResponseType).Append("> ")
            .Append(operation.OperationName)
            .Append("(")
            .WriteType(interfaceType).Append(" service, ")
            .WriteMessageOrDefault(operation.HeaderRequestType).Append(" requestHeader, ")
            .Append("IAsyncEnumerable<").WriteType(operation.RequestType.Properties[0]).Append(">").Append(" request, ")
            .WriteType(typeof(ServerCallContext)).AppendLine(" context)")
            .AppendLine("{");

        using (output.Indent())
        {
            if (operation.ResponseType.Properties.Length > 0)
            {
                output.Append("var result = ");
            }

            output
                .Append("await service.")
                .Append(operation.Method.Name)
                .Append("(");

            var source = operation.Method;
            for (var i = 0; i < source.Parameters.Length; i++)
            {
                output.WriteCommaIf(i > 0);

                var parameter = source.Parameters[i];
                if (operation.ContextInput.Contains(i))
                {
                    PushContext(output, parameter);
                }
                else if (operation.HeaderRequestTypeInput.Contains(i))
                {
                    output
                        .Append("requestHeader.Value")
                        .Append((Array.IndexOf(operation.HeaderRequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    output.Append("request");
                }
            }

            output
                .AppendLine(").ConfigureAwait(false);")
                .Append("return ");

            output
                .Append("new ")
                .WriteMessage(operation.ResponseType)
                .Append("(");

            if (operation.ResponseType.Properties.Length > 0)
            {
                output.Append("result");
            }

            output.AppendLine(");");
        }

        output.AppendLine("}");
    }

    private void BuildServerStreaming(ICodeStringBuilder output, ITypeSymbol interfaceType, IOperationDescription operation)
    {
        // ValueTask<(TResponseHeader, IAsyncEnumerable<TResponse>)> Invoke(TService service, TRequest request, ServerCallContext context)
        output
            .Append("public")
            .Append(operation.IsAsync ? " async" : string.Empty)
            .Append(" ValueTask<(")
            .WriteMessageOrDefault(operation.HeaderResponseType)
            .Append(", IAsyncEnumerable<")
            .WriteType(operation.ResponseType.Properties[0])
            .Append(">)> ")
            .Append(operation.OperationName)
            .Append("(")
            .WriteType(interfaceType).Append(" service, ")
            .WriteMessage(operation.RequestType).Append(" request, ")
            .WriteType(typeof(ServerCallContext)).AppendLine(" context)")
            .AppendLine("{");

        using (output.Indent())
        {
            output.Append("var result = ");
            if (operation.IsAsync)
            {
                output.Append("await ");
            }

            output
                .Append("service.")
                .Append(operation.Method.Name)
                .Append("(");

            var source = operation.Method;
            for (var i = 0; i < source.Parameters.Length; i++)
            {
                output.WriteCommaIf(i > 0);

                var parameter = source.Parameters[i];
                if (operation.ContextInput.Contains(i))
                {
                    PushContext(output, parameter);
                }
                else
                {
                    output
                        .Append("request.Value")
                        .Append((Array.IndexOf(operation.RequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                }
            }

            output.Append(")");
            if (operation.IsAsync)
            {
                output.Append(".ConfigureAwait(false)");
            }

            output.AppendLine(";");

            BuildWriteServerStreamingResult(output, operation);
        }

        output.AppendLine("}");
    }

    private void BuildDuplexStreaming(ICodeStringBuilder output, ITypeSymbol interfaceType, IOperationDescription operation)
    {
        // ValueTask<(TResponseHeader, IAsyncEnumerable<TResponse>)> Invoke(TService service, TRequestHeader requestHeader, IAsyncEnumerable<TRequest> request, ServerCallContext context)
        output
            .Append("public")
            .Append(operation.IsAsync ? " async" : string.Empty)
            .Append(" ValueTask<(")
            .WriteMessageOrDefault(operation.HeaderResponseType)
            .Append(", IAsyncEnumerable<")
            .WriteType(operation.ResponseType.Properties[0])
            .Append(">)> ")
            .Append(operation.OperationName)
            .Append("(")
            .WriteType(interfaceType).Append(" service, ")
            .WriteMessageOrDefault(operation.HeaderRequestType).Append(" requestHeader, ")
            .Append("IAsyncEnumerable<").WriteType(operation.RequestType.Properties[0]).Append("> request, ")
            .WriteType(typeof(ServerCallContext)).AppendLine(" context)")
            .AppendLine("{");

        using (output.Indent())
        {
            output.Append("var result = ");
            if (operation.IsAsync)
            {
                output.Append("await ");
            }

            output
                .Append("service.")
                .Append(operation.Method.Name)
                .Append("(");

            var source = operation.Method;
            for (var i = 0; i < source.Parameters.Length; i++)
            {
                output.WriteCommaIf(i > 0);

                var parameter = source.Parameters[i];
                if (operation.ContextInput.Contains(i))
                {
                    PushContext(output, parameter);
                }
                else if (operation.HeaderRequestTypeInput.Contains(i))
                {
                    output
                        .Append("requestHeader.Value")
                        .Append((Array.IndexOf(operation.HeaderRequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    output.Append("request");
                }
            }

            output.Append(")");

            if (operation.IsAsync)
            {
                output.Append(".ConfigureAwait(false)");
            }

            output.AppendLine(";");

            BuildWriteServerStreamingResult(output, operation);
        }

        output.AppendLine("}");
    }

    // return ValueTask<(Message<THeader>, IAsyncEnumerable<TResponse>)>
    private void BuildWriteServerStreamingResult(ICodeStringBuilder output, IOperationDescription operation)
    {
        output.Append("return ");
        if (operation.IsAsync)
        {
            output.Append("(");
        }
        else
        {
            output
                .Append("new ValueTask<(")
                .WriteMessageOrDefault(operation.HeaderResponseType)
                .Append(", IAsyncEnumerable<")
                .WriteType(operation.ResponseType.Properties[0])
                .Append(">)>((");
        }

        if (operation.HeaderResponseType == null)
        {
            output.Append("null");
        }
        else
        {
            output
                .Append("new ")
                .WriteMessage(operation.HeaderResponseType)
                .Append("(");

            for (var i = 0; i < operation.HeaderResponseTypeInput.Length; i++)
            {
                output
                    .WriteCommaIf(i > 0)
                    .Append("result.Item").Append((operation.HeaderResponseTypeInput[i] + 1).ToString(CultureInfo.InvariantCulture));
            }

            output.Append(")");
        }

        output.Append(", ");

        if (operation.HeaderResponseType == null)
        {
            output.Append("result");
        }
        else
        {
            output
                .Append("result.Item")
                .Append((operation.ResponseTypeIndex + 1).ToString(CultureInfo.InvariantCulture));
        }

        if (operation.IsAsync)
        {
            output.AppendLine(");");
        }
        else
        {
            output.AppendLine("));");
        }
    }

    private void PushContext(ICodeStringBuilder output, IParameterSymbol parameter)
    {
        if (parameter.Type.IsAssignableFrom(typeof(ServerCallContext))
            || parameter.Type.IsAssignableFrom(typeof(CallContext)))
        {
            output.Append("context");
            return;
        }

        if (parameter.Type.Is(typeof(CancellationToken))
            || parameter.Type.Is(typeof(CancellationToken?)))
        {
            output
                .Append("context.")
                .Append(nameof(ServerCallContext.CancellationToken));
            return;
        }

        if (parameter.Type.Is(typeof(CallOptions))
            || parameter.Type.Is(typeof(CallOptions?)))
        {
            // new CallOptions(context.RequestHeaders, context.Deadline, context.CancellationToken, context.WriteOptions)
            output
                .Append("new ")
                .WriteType(typeof(CallOptions))
                .Append("(")
                .Append("context.").Append(nameof(ServerCallContext.RequestHeaders))
                .Append(", context.").Append(nameof(ServerCallContext.Deadline))
                .Append(", context.").Append(nameof(ServerCallContext.CancellationToken))
                .Append(", context.").Append(nameof(ServerCallContext.WriteOptions))
                .Append(")");
            return;
        }

        throw new NotImplementedException();
    }
}