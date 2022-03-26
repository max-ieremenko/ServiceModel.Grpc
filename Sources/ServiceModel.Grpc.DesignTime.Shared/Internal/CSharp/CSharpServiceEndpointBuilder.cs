// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.Globalization;
using System.Linq;
using System.Threading;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpServiceEndpointBuilder : CodeGeneratorBase
    {
        private readonly ContractDescription _contract;

        public CSharpServiceEndpointBuilder(ContractDescription contract)
        {
            _contract = contract;
        }

        public override string GetGeneratedMemberName() => _contract.EndpointClassName;

        protected override void Generate()
        {
            WriteMetadata();
            Output
                .Append($"internal sealed class {_contract.EndpointClassName}")
                .AppendLine();
            Output.AppendLine("{");

            using (Output.Indent())
            {
                foreach (var interfaceDescription in _contract.Services)
                {
                    foreach (var method in interfaceDescription.Operations)
                    {
                        Output.AppendLine();
                        ImplementMethod(interfaceDescription.InterfaceTypeName, method);
                    }
                }
            }

            Output.AppendLine("}");
        }

        private void ImplementMethod(string interfaceType, OperationDescription operation)
        {
            switch (operation.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(interfaceType, operation);
                    break;

                case MethodType.ClientStreaming:
                    BuildClientStreaming(interfaceType, operation);
                    break;

                case MethodType.ServerStreaming:
                    BuildServerStreaming(interfaceType, operation);
                    break;

                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(interfaceType, operation);
                    break;

                default:
                    throw new NotImplementedException("{0} operation is not implemented.".FormatWith(operation.OperationType));
            }
        }

        private void BuildUnary(string interfaceType, OperationDescription operation)
        {
            // Task<TResponse> Invoke(TService service, TRequest request, ServerCallContext context)
            Output.Append("public ");
            if (operation.IsAsync)
            {
                Output.Append("async ");
            }

            Output
                .Append("Task<").Append(operation.ResponseType.ClassName).Append("> ")
                .Append(operation.OperationName)
                .Append("(")
                .Append(interfaceType).Append(" service, ")
                .Append(operation.RequestType.ClassName).Append(" request, ")
                .Append(nameof(ServerCallContext)).AppendLine(" context)")
                .AppendLine("{");

            using (Output.Indent())
            {
                if (operation.ResponseType.Properties.Length > 0)
                {
                    Output.Append("var result = ");
                }

                if (operation.IsAsync)
                {
                    Output.Append("await ");
                }

                Output
                    .Append("service.")
                    .Append(operation.Method.Name)
                    .Append("(");

                for (var i = 0; i < operation.Method.Parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        Output.Append(", ");
                    }

                    var parameter = operation.Method.Parameters[i];
                    if (operation.ContextInput.Contains(i))
                    {
                        PushContext(parameter);
                    }
                    else
                    {
                        Output
                            .Append("request.Value")
                            .Append((Array.IndexOf(operation.RequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                    }
                }

                Output.Append(")");
                if (operation.IsAsync)
                {
                    Output.Append(".ConfigureAwait(false)");
                }

                Output
                    .AppendLine(";")
                    .Append("return ");

                if (!operation.IsAsync)
                {
                    Output.Append("Task.FromResult(");
                }

                Output
                    .Append("new ")
                    .Append(operation.ResponseType.ClassName)
                    .Append("(");

                if (operation.ResponseType.Properties.Length > 0)
                {
                    Output.Append("result");
                }

                Output.Append(")");

                if (!operation.IsAsync)
                {
                    Output.Append(")");
                }

                Output.AppendLine(";");
            }

            Output.AppendLine("}");
        }

        private void BuildClientStreaming(string interfaceType, OperationDescription operation)
        {
            // Task<TResponse> Invoke(TService service, TRequestHeader requestHeader, IAsyncEnumerable<TRequest> request, ServerCallContext context)
            Output
                .Append("public async Task<").Append(operation.ResponseType.ClassName).Append("> ")
                .Append(operation.OperationName)
                .Append("(")
                .Append(interfaceType).Append(" service, ")
                .Append(operation.HeaderRequestType?.ClassName ?? nameof(Message)).Append(" requestHeader, ")
                .Append("IAsyncEnumerable<").Append(operation.RequestType.Properties[0]).Append(">").Append(" request, ")
                .Append(nameof(ServerCallContext)).AppendLine(" context)")
                .AppendLine("{");

            using (Output.Indent())
            {
                if (operation.ResponseType.Properties.Length > 0)
                {
                    Output.Append("var result = ");
                }

                Output
                    .Append("await service.")
                    .Append(operation.Method.Name)
                    .Append("(");

                for (var i = 0; i < operation.Method.Parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        Output.Append(", ");
                    }

                    var parameter = operation.Method.Parameters[i];
                    if (operation.ContextInput.Contains(i))
                    {
                        PushContext(parameter);
                    }
                    else if (operation.HeaderRequestTypeInput.Contains(i))
                    {
                        Output
                            .Append("requestHeader.Value")
                            .Append((Array.IndexOf(operation.HeaderRequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        Output.Append("request");
                    }
                }

                Output
                    .AppendLine(").ConfigureAwait(false);")
                    .Append("return ");

                Output
                    .Append("new ")
                    .Append(operation.ResponseType.ClassName)
                    .Append("(");

                if (operation.ResponseType.Properties.Length > 0)
                {
                    Output.Append("result");
                }

                Output.AppendLine(");");
            }

            Output.AppendLine("}");
        }

        private void BuildServerStreaming(string interfaceType, OperationDescription operation)
        {
            // ValueTask<(TResponseHeader, IAsyncEnumerable<TResponse>)> Invoke(TService service, TRequest request, ServerCallContext context)
            Output
                .Append("public")
                .Append(operation.IsAsync ? " async" : string.Empty)
                .Append(" ValueTask<(")
                .Append(operation.HeaderResponseType?.ClassName ?? nameof(Message))
                .Append(", IAsyncEnumerable<")
                .Append(operation.ResponseType.Properties[0])
                .Append(">)> ")
                .Append(operation.OperationName)
                .Append("(")
                .Append(interfaceType).Append(" service, ")
                .Append(operation.RequestType.ClassName).Append(" request, ")
                .Append(nameof(ServerCallContext)).AppendLine(" context)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output.Append("var result = ");
                if (operation.IsAsync)
                {
                    Output.Append("await ");
                }

                Output
                    .Append("service.")
                    .Append(operation.Method.Name)
                    .Append("(");

                for (var i = 0; i < operation.Method.Parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        Output.Append(", ");
                    }

                    var parameter = operation.Method.Parameters[i];
                    if (operation.ContextInput.Contains(i))
                    {
                        PushContext(parameter);
                    }
                    else
                    {
                        Output
                            .Append("request.Value")
                            .Append((Array.IndexOf(operation.RequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                    }
                }

                Output.Append(")");
                if (operation.IsAsync)
                {
                    Output.Append(".ConfigureAwait(false)");
                }

                Output.AppendLine(";");

                BuildWriteServerStreamingResult(operation);
            }

            Output.AppendLine("}");
        }

        private void BuildDuplexStreaming(string interfaceType, OperationDescription operation)
        {
            // ValueTask<(TResponseHeader, IAsyncEnumerable<TResponse>)> Invoke(TService service, TRequestHeader requestHeader, IAsyncEnumerable<TRequest> request, ServerCallContext context)
            Output
                .Append("public")
                .Append(operation.IsAsync ? " async" : string.Empty)
                .Append(" ValueTask<(")
                .Append(operation.HeaderResponseType?.ClassName ?? nameof(Message))
                .Append(", IAsyncEnumerable<")
                .Append(operation.ResponseType.Properties[0])
                .Append(">)> ")
                .Append(operation.OperationName)
                .Append("(")
                .Append(interfaceType).Append(" service, ")
                .Append(operation.HeaderRequestType?.ClassName ?? nameof(Message)).Append(" requestHeader, ")
                .Append("IAsyncEnumerable<").Append(operation.RequestType.Properties[0]).Append("> request, ")
                .Append(nameof(ServerCallContext)).AppendLine(" context)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output.Append("var result = ");
                if (operation.IsAsync)
                {
                    Output.Append("await ");
                }

                Output
                    .Append("service.")
                    .Append(operation.Method.Name)
                    .Append("(");

                for (var i = 0; i < operation.Method.Parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        Output.Append(", ");
                    }

                    var parameter = operation.Method.Parameters[i];
                    if (operation.ContextInput.Contains(i))
                    {
                        PushContext(parameter);
                    }
                    else if (operation.HeaderRequestTypeInput.Contains(i))
                    {
                        Output
                            .Append("requestHeader.Value")
                            .Append((Array.IndexOf(operation.HeaderRequestTypeInput, i) + 1).ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        Output.Append("request");
                    }
                }

                Output.Append(")");

                if (operation.IsAsync)
                {
                    Output.Append(".ConfigureAwait(false)");
                }

                Output.AppendLine(";");

                BuildWriteServerStreamingResult(operation);
            }

            Output.AppendLine("}");
        }

        // return ValueTask<(Message<THeader>, IAsyncEnumerable<TResponse>)>
        private void BuildWriteServerStreamingResult(OperationDescription operation)
        {
            Output.Append("return ");
            if (operation.IsAsync)
            {
                Output.Append("(");
            }
            else
            {
                Output
                    .Append("new ValueTask<(")
                    .Append(operation.HeaderResponseType?.ClassName ?? nameof(Message))
                    .Append(", IAsyncEnumerable<")
                    .Append(operation.ResponseType.Properties[0])
                    .Append(">)>((");
            }

            if (operation.HeaderResponseType == null)
            {
                Output.Append("null");
            }
            else
            {
                Output
                    .Append("new ")
                    .Append(operation.HeaderResponseType.ClassName)
                    .Append("(");

                for (var i = 0; i < operation.HeaderResponseTypeInput.Length; i++)
                {
                    if (i > 0)
                    {
                        Output.Append(", ");
                    }

                    Output.Append("result.Item").Append((operation.HeaderResponseTypeInput[i] + 1).ToString(CultureInfo.InvariantCulture));
                }

                Output.Append(")");
            }

            Output.Append(", ");

            if (operation.HeaderResponseType == null)
            {
                Output.Append("result");
            }
            else
            {
                Output
                    .Append("result.Item")
                    .Append((operation.ResponseTypeIndex + 1).ToString(CultureInfo.InvariantCulture));
            }

            if (operation.IsAsync)
            {
                Output.AppendLine(");");
            }
            else
            {
                Output.AppendLine("));");
            }
        }

        private void PushContext(ParameterDescription parameter)
        {
            if (parameter.TypeSymbol.IsAssignableFrom(typeof(ServerCallContext))
                || parameter.TypeSymbol.IsAssignableFrom(typeof(CallContext)))
            {
                Output.Append("context");
                return;
            }

            if (parameter.TypeSymbol.Is(typeof(CancellationToken))
                || parameter.TypeSymbol.Is(typeof(CancellationToken?)))
            {
                Output
                    .Append("context.")
                    .Append(nameof(ServerCallContext.CancellationToken));
                return;
            }

            if (parameter.TypeSymbol.Is(typeof(CallOptions))
                || parameter.TypeSymbol.Is(typeof(CallOptions?)))
            {
                // new CallOptions(context.RequestHeaders, context.Deadline, context.CancellationToken, context.WriteOptions)
                Output
                    .Append("new ")
                    .Append(nameof(CallOptions))
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
}
