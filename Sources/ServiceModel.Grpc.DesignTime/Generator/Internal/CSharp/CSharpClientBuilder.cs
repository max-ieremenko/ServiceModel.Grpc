// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Globalization;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpClientBuilder : CodeGeneratorBase
    {
        private const string VarCallOptions = "__callOptions";
        private const string VarRequest = "__request";

        private readonly ContractDescription _contract;
        private readonly HashSet<string> _uniqueMemberNames;

        public CSharpClientBuilder(ContractDescription contract)
        {
            _contract = contract;
            _uniqueMemberNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public override string GetGeneratedMemberName() => _contract.ClientClassName;

        protected override void Generate()
        {
            Output
                .Append($"internal sealed class {_contract.ClientClassName} : ")
                .AppendFormat("ClientBase<{0}>, ", _contract.ClientClassName)
                .AppendLine(_contract.ContractInterfaceName);
            Output.AppendLine("{");

            using (Output.Indent())
            {
                BuildCtorCallInvoker();
                BuildCtorConfiguration();
                BuildProperties();

                foreach (var interfaceDescription in _contract.Interfaces)
                {
                    foreach (var method in interfaceDescription.Methods)
                    {
                        ImplementNotSupportedMethod(interfaceDescription.InterfaceTypeName, method);
                    }
                }

                foreach (var interfaceDescription in _contract.Services)
                {
                    foreach (var method in interfaceDescription.Methods)
                    {
                        ImplementNotSupportedMethod(interfaceDescription.InterfaceTypeName, method);
                    }

                    foreach (var method in interfaceDescription.NotSupportedOperations)
                    {
                        ImplementNotSupportedMethod(interfaceDescription.InterfaceTypeName, method);
                    }

                    foreach (var method in interfaceDescription.Operations)
                    {
                        ImplementMethod(interfaceDescription.InterfaceTypeName, method);
                    }
                }

                BuildMethodNewInstance();
            }

            Output.AppendLine("}");
        }

        private void BuildCtorCallInvoker()
        {
            Output
                .Append("public ")
                .Append(_contract.ClientClassName)
                .Append("(")
                .Append(nameof(CallInvoker)).Append(" callInvoker, ")
                .Append(_contract.ContractClassName).Append(" contract, ")
                .Append("Func<CallOptions> defaultCallOptionsFactory")
                .AppendLine(")");

            using (Output.Indent())
            {
                Output.AppendLine(": base(callInvoker)");
            }

            Output.Append("{");
            using (Output.Indent())
            {
                Output.AppendLine("if (contract == null) throw new ArgumentNullException(\"contract\");");

                Output.AppendLine("Contract = contract;");
                Output.AppendLine("DefaultCallOptionsFactory = defaultCallOptionsFactory;");
            }

            Output.Append("}");
        }

        private void BuildCtorConfiguration()
        {
            Output
                .Append("private ")
                .Append(_contract.ClientClassName)
                .Append("(")
                .Append("ClientBaseConfiguration configuration, ")
                .Append(_contract.ContractClassName).Append(" contract, ")
                .Append("Func<CallOptions> defaultCallOptionsFactory")
                .AppendLine(")");

            using (Output.Indent())
            {
                Output.AppendLine(": base(configuration)");
            }

            Output.Append("{");
            using (Output.Indent())
            {
                Output.AppendLine("Contract = contract;");
                Output.AppendLine("DefaultCallOptionsFactory = defaultCallOptionsFactory;");
            }

            Output.Append("}");
        }

        private void BuildMethodNewInstance()
        {
            Output
                .Append("protected override ")
                .Append(_contract.ClientClassName)
                .AppendLine(" NewInstance(ClientBaseConfiguration configuration)");

            Output.Append("{");
            using (Output.Indent())
            {
                Output
                    .Append("return new ")
                    .Append(_contract.ClientClassName)
                    .AppendLine("(configuration, Contract, DefaultCallOptionsFactory);");
            }

            Output.Append("}");
        }

        private void BuildProperties()
        {
            Output
                .Append("public ")
                .Append(_contract.ContractClassName)
                .AppendLine(" Contract { get; }");

            Output.AppendLine("public Func<CallOptions> DefaultCallOptionsFactory  { get; }");
        }

        private void ImplementMethod(string interfaceType, OperationDescription operation)
        {
            CreateMethodWithSignature(interfaceType, operation.Method);
            Output.AppendLine("{");

            Action? adapterBuilder = null;
            using (Output.Indent())
            {
                switch (operation.OperationType)
                {
                    case MethodType.Unary:
                        BuildUnary(operation);
                        break;
                    case MethodType.ClientStreaming:
                        BuildClientStreaming(operation);
                        break;
                    case MethodType.ServerStreaming:
                        adapterBuilder = BuildServerStreaming(operation);
                        break;
                    case MethodType.DuplexStreaming:
                        adapterBuilder = BuildDuplexStreaming(operation);
                        break;
                    default:
                        throw new NotImplementedException("{0} operation is not implemented.".FormatWith(operation.OperationType));
                }
            }

            Output.AppendLine("}");

            adapterBuilder?.Invoke();
        }

        private void BuildUnary(OperationDescription operation)
        {
            InitializeCallOptionsVariable(operation);
            InitializeRequestVariable(operation);

            if (operation.IsAsync)
            {
                var callContextValue = GetContextVariableValue(operation);

                Output
                    .Append("var __call = CallInvoker.AsyncUnaryCall(Contract.")
                    .Append(operation.GrpcMethodName)
                    .Append(", null, ")
                    .Append(VarCallOptions)
                    .Append(", ")
                    .Append(VarRequest)
                    .AppendLine(");");

                Output.Append("var __response = ");
                if (operation.ResponseType.Properties.Length == 0)
                {
                    Output
                        .Append(nameof(ClientChannelAdapter))
                        .Append(".")
                        .Append(nameof(ClientChannelAdapter.AsyncUnaryCallWait))
                        .Append("(__call, ")
                        .Append(callContextValue)
                        .AppendLine(", __callOptions.CancellationToken);");

                    if (operation.Method.ReturnTypeSymbol.IsValueTask())
                    {
                        Output.AppendLine("return new ValueTask(__response);");
                    }
                    else
                    {
                        Output.AppendLine("return __response;");
                    }
                }
                else
                {
                    Output
                        .Append(nameof(ClientChannelAdapter))
                        .Append(".")
                        .Append(nameof(ClientChannelAdapter.GetAsyncUnaryCallResult))
                        .Append("(__call, ")
                        .Append(callContextValue)
                        .AppendLine(", __callOptions.CancellationToken);");

                    if (operation.Method.ReturnTypeSymbol.IsValueTask())
                    {
                        Output
                            .Append("return new ")
                            .Append(operation.Method.ReturnType)
                            .AppendLine("(__response);");
                    }
                    else
                    {
                        Output.AppendLine("return __response;");
                    }
                }
            }
            else
            {
                if (operation.ResponseType.Properties.Length != 0)
                {
                    Output.Append("var __response = ");
                }

                Output
                    .Append("CallInvoker.BlockingUnaryCall(Contract.")
                    .Append(operation.GrpcMethodName)
                    .Append(", null, ")
                    .Append(VarCallOptions)
                    .Append(", ")
                    .Append(VarRequest)
                    .AppendLine(");");

                if (operation.ResponseType.Properties.Length != 0)
                {
                    Output.AppendLine("return __response.Value1;");
                }
            }
        }

        private void BuildClientStreaming(OperationDescription operation)
        {
            InitializeCallOptionsVariable(operation);
            var callContextValue = GetContextVariableValue(operation);

            Output
                .Append("var __call = CallInvoker.AsyncClientStreamingCall(Contract.")
                .Append(operation.GrpcMethodName)
                .Append(", null, ")
                .Append(VarCallOptions)
                .AppendLine(");");

            Output
                .Append("var __response = ")
                .Append(nameof(ClientChannelAdapter))
                .Append(".");

            if (operation.ResponseType.Properties.Length == 0)
            {
                Output.Append(nameof(ClientChannelAdapter.WriteClientStreamingRequestWait));
            }
            else
            {
                Output.Append(nameof(ClientChannelAdapter.WriteClientStreamingRequest));
            }

            Output
                .Append("(__call, ")
                .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
                .Append(", ")
                .Append(callContextValue)
                .Append(", ")
                .Append(VarCallOptions)
                .AppendLine(".CancellationToken);");

            Output.Append("return ");
            if (operation.Method.ReturnTypeSymbol.IsValueTask())
            {
                Output
                    .Append("new ")
                    .Append(operation.Method.ReturnType)
                    .Append("(__response)");
            }
            else
            {
                Output.Append("__response");
            }

            Output.AppendLine(";");
        }

        private Action? BuildServerStreaming(OperationDescription operation)
        {
            InitializeCallOptionsVariable(operation);
            InitializeRequestVariable(operation);
            var callContextValue = GetContextVariableValue(operation);

            Output
                .Append("var __call = CallInvoker.AsyncServerStreamingCall(Contract.")
                .Append(operation.GrpcMethodName)
                .Append(", null, ")
                .Append(VarCallOptions)
                .Append(", ")
                .Append(VarRequest)
                .AppendLine(");");

            Output
                .Append("var __response = ")
                .Append(nameof(ClientChannelAdapter))
                .Append(".");

            if (operation.IsAsync)
            {
                Output.Append(nameof(ClientChannelAdapter.GetServerStreamingCallResultAsync));
            }
            else
            {
                Output.Append(nameof(ClientChannelAdapter.GetServerStreamingCallResult));
            }

            string resultName;
            Action? adapterBuilder = null;
            if (operation.HeaderResponseType == null)
            {
                Output
                    .Append("(__call, ")
                    .Append(callContextValue)
                    .Append(", ")
                    .Append(VarCallOptions)
                    .AppendLine(".CancellationToken);");
                resultName = "__response";
            }
            else
            {
                Output
                    .Append("<")
                    .Append(operation.HeaderResponseType.ClassName)
                    .Append(", ")
                    .Append(operation.ResponseType.Properties[0])
                    .Append(">(__call, ")
                    .Append(callContextValue)
                    .Append(", ")
                    .Append(VarCallOptions)
                    .Append(".CancellationToken, Contract.")
                    .Append(operation.GrpcMethodOutputHeaderName)
                    .AppendLine(");");

                var adapterFunctionName = GetUniqueMemberName("Adapt" + operation.Method.Name + "Response");
                Output
                    .Append("var __result = ")
                    .Append(nameof(ClientChannelAdapter))
                    .Append(".")
                    .Append(nameof(ClientChannelAdapter.ContinueWith))
                    .Append("(__response, ")
                    .Append(adapterFunctionName)
                    .AppendLine(");");

                resultName = "__result";
                adapterBuilder = () => BuildServerStreamingResultAdapter(operation, adapterFunctionName);
            }

            Output.Append("return ");
            if (operation.IsAsync && operation.Method.ReturnTypeSymbol.IsValueTask())
            {
                Output
                    .Append("new ")
                    .Append(operation.Method.ReturnType)
                    .Append("(")
                    .Append(resultName)
                    .Append(")");
            }
            else
            {
                Output.Append(resultName);
            }

            Output.AppendLine(";");
            return adapterBuilder;
        }

        private void BuildServerStreamingResultAdapter(OperationDescription operation, string functionName)
        {
            var returnType = SyntaxTools.GetFullName(operation.Method.ReturnTypeSymbol.GenericTypeArguments()[0]);
            Output
                .Append("private static ")
                .Append(returnType)
                .Append(" ")
                .Append(functionName)
                .Append("(")
                .Append(nameof(ValueTuple))
                .Append("<")
                .Append(operation.HeaderResponseType!.ClassName)
                .Append(", IAsyncEnumerable<")
                .Append(operation.ResponseType.Properties[0])
                .Append(">>")
                .AppendLine("response)");

            Output.AppendLine("{");
            using (Output.Indent())
            {
                Output.AppendLine("var message = response.Item1;");

                Output
                    .Append("return new ")
                    .Append(returnType)
                    .Append("(");

                // operation.ResponseTypeIndex;
                var propertiesCount = operation.HeaderResponseTypeInput.Length + 1;
                for (var i = 0; i < propertiesCount; i++)
                {
                    string value;
                    if (i == operation.ResponseTypeIndex)
                    {
                        value = "response.Item2";
                    }
                    else
                    {
                        var index = Array.IndexOf(operation.HeaderResponseTypeInput, i) + 1;
                        value = "message.Value" + index.ToString(CultureInfo.InvariantCulture);
                    }

                    if (i > 0)
                    {
                        Output.Append(", ");
                    }

                    Output.Append(value);
                }

                Output.AppendLine(");");
            }

            Output.AppendLine("}");
        }

        private Action? BuildDuplexStreaming(OperationDescription operation)
        {
            InitializeCallOptionsVariable(operation);
            var callContextValue = GetContextVariableValue(operation);

            Output
                .Append("var __call = CallInvoker.AsyncDuplexStreamingCall(Contract.")
                .Append(operation.GrpcMethodName)
                .Append(", null, ")
                .Append(VarCallOptions)
                .AppendLine(");");

            Output
                .Append("var __response = ")
                .Append(nameof(ClientChannelAdapter))
                .Append(".");

            if (operation.IsAsync)
            {
                Output.Append(nameof(ClientChannelAdapter.GetDuplexCallResultAsync));
            }
            else
            {
                Output.Append(nameof(ClientChannelAdapter.GetDuplexCallResult));
            }

            string resultName;
            Action? adapterBuilder = null;
            if (operation.HeaderResponseType == null)
            {
                Output
                    .Append("(__call, ")
                    .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
                    .Append(", ")
                    .Append(callContextValue)
                    .Append(", ")
                    .Append(VarCallOptions)
                    .AppendLine(".CancellationToken);");
                resultName = "__response";
            }
            else
            {
                Output
                    .Append("<")
                    .Append(operation.HeaderResponseType.ClassName)
                    .Append(", ")
                    .Append(operation.RequestType.Properties[0])
                    .Append(", ")
                    .Append(operation.ResponseType.Properties[0])
                    .Append(">(__call, ")
                    .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
                    .Append(", ")
                    .Append(callContextValue)
                    .Append(", ")
                    .Append(VarCallOptions)
                    .Append(".CancellationToken, Contract.")
                    .Append(operation.GrpcMethodOutputHeaderName)
                    .AppendLine(");");

                var adapterFunctionName = GetUniqueMemberName("Adapt" + operation.Method.Name + "Response");
                Output
                    .Append("var __result = ")
                    .Append(nameof(ClientChannelAdapter))
                    .Append(".")
                    .Append(nameof(ClientChannelAdapter.ContinueWith))
                    .Append("(__response, ")
                    .Append(adapterFunctionName)
                    .AppendLine(");");

                resultName = "__result";
                adapterBuilder = () => BuildServerStreamingResultAdapter(operation, adapterFunctionName);
            }

            Output.Append("return ");
            if (operation.IsAsync && operation.Method.ReturnTypeSymbol.IsValueTask())
            {
                Output
                    .Append("new ")
                    .Append(operation.Method.ReturnType)
                    .Append("(")
                    .Append(resultName)
                    .Append(")");
            }
            else
            {
                Output.Append(resultName);
            }

            Output.AppendLine(";");
            return adapterBuilder;
        }

        private void ImplementNotSupportedMethod(string interfaceType, NotSupportedMethodDescription method)
        {
            CreateMethodWithSignature(interfaceType, method.Method);

            Output.AppendLine("{");
            using (Output.Indent())
            {
                Output
                    .Append("throw new NotSupportedException(\"")
                    .Append(method.Error)
                    .AppendLine("\");");
            }

            Output.AppendLine("}");
        }

        private void CreateMethodWithSignature(string interfaceType, MethodDescription method)
        {
            Output
                .Append(method.ReturnType)
                .Append(" ")
                .Append(interfaceType)
                .Append(".")
                .Append(method.Name);

            if (method.Source.TypeArguments.Length != 0)
            {
                Output.Append("<");
                for (var i = 0; i < method.TypeArguments.Length; i++)
                {
                    if (i > 0)
                    {
                        Output.Append(", ");
                    }

                    Output.Append(method.TypeArguments[i]);
                }

                Output.Append(">");
            }

            Output.Append("(");
            for (var i = 0; i < method.Parameters.Length; i++)
            {
                if (i > 0)
                {
                    Output.Append(", ");
                }

                var p = method.Parameters[i];
                if (p.IsOut)
                {
                    Output.Append("out ");
                }
                else if (p.IsRef)
                {
                    Output.Append("ref ");
                }

                Output
                    .Append(p.Type)
                    .Append(" ")
                    .Append(p.Name);
            }

            Output.AppendLine(")");
        }

        private void InitializeCallOptionsVariable(OperationDescription operation)
        {
            Output
                .Append("var ")
                .Append(VarCallOptions)
                .Append(" = new ")
                .Append(nameof(CallOptionsBuilder))
                .Append("(DefaultCallOptionsFactory)");

            using (Output.Indent())
            {
                for (var i = 0; i < operation.ContextInput.Length; i++)
                {
                    var parameter = operation.Method.Parameters[operation.ContextInput[i]];
                    Output.AppendLine(".");
                    Output.AppendFormat("With{0}({1})", parameter.Type, parameter.Name);
                }

                if (operation.HeaderRequestType != null)
                {
                    Output
                        .AppendLine(".")
                        .Append(nameof(CallOptionsBuilder.WithMethodInputHeader))
                        .Append("(Contract.")
                        .Append(operation.GrpcMethodInputHeaderName)
                        .Append(", new ")
                        .Append(operation.HeaderRequestType.ClassName)
                        .Append("(");

                    for (var i = 0; i < operation.HeaderRequestTypeInput.Length; i++)
                    {
                        var parameter = operation.Method.Parameters[operation.HeaderRequestTypeInput[i]];
                        Output
                            .AppendCommaIf(i != 0)
                            .Append(parameter.Name);
                    }

                    Output.Append("))");
                }

                Output
                    .AppendLine(".")
                    .AppendLine("Build();");
            }
        }

        private void InitializeRequestVariable(OperationDescription operation)
        {
            Output
                .Append("var ")
                .Append(VarRequest)
                .Append(" = new ")
                .Append(operation.RequestType.ClassName)
                .Append("(");

            for (var i = 0; i < operation.RequestTypeInput.Length; i++)
            {
                var parameter = operation.Method.Parameters[operation.RequestTypeInput[i]];
                Output
                    .AppendCommaIf(i != 0)
                    .Append(parameter.Name);
            }

            Output.AppendLine(");");
        }

        private string GetContextVariableValue(OperationDescription operation)
        {
            foreach (var i in operation.ContextInput)
            {
                var p = operation.Method.Parameters[i];
                if (p.Type.Equals(nameof(CallContext), StringComparison.Ordinal))
                {
                    return p.Name;
                }
            }

            return "null";
        }

        private string GetUniqueMemberName(string suggestedName)
        {
            var index = 1;
            var result = suggestedName;

            while (!_uniqueMemberNames.Add(result))
            {
                result = suggestedName + index.ToString(CultureInfo.InvariantCulture);
                index++;
            }

            return result;
        }
    }
}
