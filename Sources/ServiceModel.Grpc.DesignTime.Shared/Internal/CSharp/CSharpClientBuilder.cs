// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

internal sealed class CSharpClientBuilder : CodeGeneratorBase
{
    private const string VarCallOptionsBuilder = "__callOptionsBuilder";

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
        WriteMetadata();
        Output
            .Append($"internal sealed class {_contract.ClientClassName} : ")
            .AppendType(typeof(ClientBase<>))
            .AppendFormat("{0}>, ", _contract.ClientClassName)
            .AppendLine(_contract.ContractInterfaceName);
        Output.AppendLine("{");

        using (Output.Indent())
        {
            BuildCtorCallInvoker();
            Output.AppendLine();

            BuildCtorConfiguration();
            Output.AppendLine();

            BuildProperties();

            foreach (var interfaceDescription in _contract.Interfaces)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(interfaceDescription.InterfaceTypeName, method);
                    Output.AppendLine();
                }
            }

            foreach (var interfaceDescription in _contract.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(interfaceDescription.InterfaceTypeName, method);
                    Output.AppendLine();
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    ImplementNotSupportedMethod(interfaceDescription.InterfaceTypeName, method);
                    Output.AppendLine();
                }

                foreach (var method in interfaceDescription.Operations)
                {
                    ImplementMethod(interfaceDescription.InterfaceTypeName, method, null);
                    Output.AppendLine();
                }

                foreach (var entry in interfaceDescription.SyncOverAsync)
                {
                    ImplementMethod(interfaceDescription.InterfaceTypeName, entry.Sync, entry.Async.GrpcMethodName);
                    Output.AppendLine();
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
            .AppendType(typeof(CallInvoker)).Append(" callInvoker, ")
            .Append(_contract.ContractClassName).Append(" contract, Func<")
            .AppendType(typeof(CallOptions))
            .Append("> defaultCallOptionsFactory")
            .AppendLine(")");

        using (Output.Indent())
        {
            Output.AppendLine(": base(callInvoker)");
        }

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output.AppendArgumentNullException("contract");

            Output.AppendLine("Contract = contract;");
            Output.AppendLine("DefaultCallOptionsFactory = defaultCallOptionsFactory;");
        }

        Output.AppendLine("}");
    }

    private void BuildCtorConfiguration()
    {
        Output
            .Append("private ")
            .Append(_contract.ClientClassName)
            .Append("(")
            .Append("ClientBaseConfiguration configuration, ")
            .Append(_contract.ContractClassName).Append(" contract, Func<")
            .AppendType(typeof(CallOptions))
            .Append("> defaultCallOptionsFactory")
            .AppendLine(")");

        using (Output.Indent())
        {
            Output.AppendLine(": base(configuration)");
        }

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output.AppendLine("Contract = contract;");
            Output.AppendLine("DefaultCallOptionsFactory = defaultCallOptionsFactory;");
        }

        Output.AppendLine("}");
    }

    private void BuildMethodNewInstance()
    {
        Output
            .Append("protected override ")
            .Append(_contract.ClientClassName)
            .AppendLine(" NewInstance(ClientBaseConfiguration configuration)");

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output
                .Append("return new ")
                .Append(_contract.ClientClassName)
                .AppendLine("(configuration, Contract, DefaultCallOptionsFactory);");
        }

        Output.AppendLine("}");
    }

    private void BuildProperties()
    {
        Output
            .Append("public ")
            .Append(_contract.ContractClassName)
            .AppendLine(" Contract { get; }")
            .AppendLine();

        Output
            .Append("public Func<")
            .AppendType(typeof(CallOptions))
            .AppendLine("> DefaultCallOptionsFactory  { get; }")
            .AppendLine();
    }

    private void ImplementMethod(string interfaceType, OperationDescription operation, string? grpcMethodName)
    {
        CreateMethodWithSignature(interfaceType, operation.Method);
        Output.AppendLine("{");

        Action? adapterBuilder = null;
        using (Output.Indent())
        {
            switch (operation.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(operation, grpcMethodName);
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

    private void BuildUnary(OperationDescription operation, string? grpcMethodName)
    {
        InitializeCallOptionsBuilderVariable(operation);

        var hasReturn = operation.IsAsync || operation.ResponseType.Properties.Length > 0;

        // var __response = new UnaryCall<TRequest, TResponse>(method, CallInvoker, __callOptionsBuilder)
        Output
            .Append(hasReturn ? "var __response = " : string.Empty)
            .Append("new ")
            .AppendType(typeof(UnaryCall<,>))
            .AppendMessage(operation.RequestType)
            .Append(", ")
            .AppendMessage(operation.ResponseType)
            .Append(">(Contract.")
            .Append(grpcMethodName ?? operation.GrpcMethodName)
            .Append(", CallInvoker, ")
            .Append(VarCallOptionsBuilder)
            .AppendLine(")");

        using (Output.Indent())
        {
            Output.Append(".Invoke");

            if (operation.IsAsync)
            {
                Output.Append("Async");
            }

            if (operation.ResponseType.Properties.Length > 0)
            {
                Output
                    .Append("<")
                    .Append(operation.ResponseType.Properties[0])
                    .Append(">");
            }

            Output.Append("(");
            CreateRequestMessage(operation);
            Output.AppendLine(");");
        }

        if (hasReturn)
        {
            Output.Append("return ");

            if (operation.Method.ReturnTypeSymbol.IsValueTask())
            {
                Output
                    .Append("new ")
                    .Append(operation.Method.ReturnType)
                    .AppendLine("(__response);");
            }
            else
            {
                Output.AppendLine("__response;");
            }
        }
    }

    private void BuildClientStreaming(OperationDescription operation)
    {
        InitializeCallOptionsBuilderVariable(operation);

        // var __response = new ClientStreamingCall<TRequestHeader, TRequest, TResponse>(method, CallInvoker, __callOptionsBuilder)
        Output
            .Append("var __response = new ")
            .AppendType(typeof(ClientStreamingCall<,,>))
            .AppendMessageOrDefault(operation.HeaderRequestType)
            .Append(", ")
            .Append(operation.RequestType.Properties[0])
            .Append(", ")
            .AppendMessage(operation.ResponseType)
            .Append(">(Contract.")
            .Append(operation.GrpcMethodName)
            .Append(", CallInvoker, ")
            .Append(VarCallOptionsBuilder)
            .AppendLine(")");

        using (Output.Indent())
        {
            if (operation.HeaderRequestType != null)
            {
                WithRequestHeader(operation);
            }

            if (operation.ResponseType.Properties.Length > 0)
            {
                Output
                    .Append(".InvokeAsync<")
                    .Append(operation.ResponseType.Properties[0])
                    .Append(">(")
                    .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
                    .AppendLine(");");
            }
            else
            {
                Output
                    .Append(".InvokeAsync(")
                    .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
                    .AppendLine(");");
            }
        }

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
        InitializeCallOptionsBuilderVariable(operation);

        // var __response = new ServerStreamingCall<TRequest, TResponseHeader, TResponse>(method, CallInvoker, __callOptionsBuilder)
        Output
            .Append("var __response = new ")
            .AppendType(typeof(ServerStreamingCall<,,>))
            .AppendMessage(operation.RequestType)
            .Append(", ")
            .AppendMessageOrDefault(operation.HeaderResponseType)
            .Append(", ")
            .Append(operation.ResponseType.Properties[0])
            .Append(">(Contract.")
            .Append(operation.GrpcMethodName)
            .Append(", CallInvoker, ")
            .Append(VarCallOptionsBuilder)
            .AppendLine(")");

        Action? adapterBuilder = null;
        using (Output.Indent())
        {
            if (operation.HeaderResponseType != null)
            {
                WithResponseHeader(operation);
            }

            if (operation.IsAsync)
            {
                Output.Append(".InvokeAsync(");
            }
            else
            {
                Output.Append(".Invoke(");
            }

            CreateRequestMessage(operation);

            if (operation.HeaderResponseType != null)
            {
                var adapterFunctionName = GetUniqueMemberName("Adapt" + operation.Method.Name + "Response");
                adapterBuilder = () => BuildServerStreamingResultAdapter(operation, adapterFunctionName);
                Output
                    .Append(", ")
                    .Append(adapterFunctionName);
            }

            Output.AppendLine(");");
        }

        Output.Append("return ");

        if (operation.IsAsync && operation.Method.ReturnTypeSymbol.IsValueTask())
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
            .AppendMessage(operation.HeaderResponseType!)
            .Append(" header, IAsyncEnumerable<")
            .Append(operation.ResponseType.Properties[0])
            .Append(">")
            .AppendLine(" response)");

        Output.AppendLine("{");
        using (Output.Indent())
        {
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
                    value = "response";
                }
                else
                {
                    var index = Array.IndexOf(operation.HeaderResponseTypeInput, i) + 1;
                    value = "header.Value" + index.ToString(CultureInfo.InvariantCulture);
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
        InitializeCallOptionsBuilderVariable(operation);

        // var __response = new DuplexStreamingCall<TRequestHeader, TRequest, TResponseHeader, TResponse>(method, CallInvoker, __callOptionsBuilder)
        Output
            .Append("var __response = new ")
            .AppendType(typeof(DuplexStreamingCall<,,,>))
            .AppendMessageOrDefault(operation.HeaderRequestType)
            .Append(", ")
            .Append(operation.RequestType.Properties[0])
            .Append(", ")
            .AppendMessageOrDefault(operation.HeaderResponseType)
            .Append(", ")
            .Append(operation.ResponseType.Properties[0])
            .Append(">(Contract.")
            .Append(operation.GrpcMethodName)
            .Append(", CallInvoker, ")
            .Append(VarCallOptionsBuilder)
            .AppendLine(")");

        Action? adapterBuilder = null;
        using (Output.Indent())
        {
            if (operation.HeaderRequestType != null)
            {
                WithRequestHeader(operation);
            }

            if (operation.HeaderResponseType != null)
            {
                WithResponseHeader(operation);
            }

            if (operation.HeaderResponseType == null)
            {
                Output
                    .Append(operation.IsAsync ? ".InvokeAsync(" : ".Invoke(")
                    .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
                    .AppendLine(");");
            }
            else
            {
                var adapterFunctionName = GetUniqueMemberName("Adapt" + operation.Method.Name + "Response");
                adapterBuilder = () => BuildServerStreamingResultAdapter(operation, adapterFunctionName);
                Output
                    .Append(".InvokeAsync(")
                    .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
                    .Append(", ")
                    .Append(adapterFunctionName)
                    .AppendLine(");");
            }
        }

        Output.Append("return ");

        if (operation.IsAsync && operation.Method.ReturnTypeSymbol.IsValueTask())
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

    private void InitializeCallOptionsBuilderVariable(OperationDescription operation)
    {
        Output
            .Append("var ")
            .Append(VarCallOptionsBuilder)
            .Append(" = new ")
            .AppendType(typeof(CallOptionsBuilder))
            .Append("(DefaultCallOptionsFactory)");

        using (Output.Indent())
        {
            for (var i = 0; i < operation.ContextInput.Length; i++)
            {
                var parameter = operation.Method.Parameters[operation.ContextInput[i]];
                var type = SyntaxTools.IsNullable(parameter.TypeSymbol) ? parameter.TypeSymbol.GenericTypeArguments()[0] : parameter.TypeSymbol;
                Output
                    .AppendLine()
                    .Append(".With").Append(type.Name).Append("(").Append(parameter.Name).Append(")");
            }

            Output.AppendLine(";");
        }
    }

    private void CreateRequestMessage(OperationDescription operation)
    {
        Output
            .Append("new ")
            .AppendMessage(operation.RequestType)
            .Append("(");

        for (var i = 0; i < operation.RequestTypeInput.Length; i++)
        {
            var parameter = operation.Method.Parameters[operation.RequestTypeInput[i]];
            Output
                .AppendCommaIf(i != 0)
                .Append(parameter.Name);
        }

        Output.Append(")");
    }

    private void WithRequestHeader(OperationDescription operation)
    {
        Output
            .Append(".WithRequestHeader(Contract.")
            .Append(operation.GrpcMethodInputHeaderName)
            .Append(", new ")
            .AppendMessage(operation.HeaderRequestType!)
            .Append("(");

        for (var i = 0; i < operation.HeaderRequestTypeInput.Length; i++)
        {
            var parameter = operation.Method.Parameters[operation.HeaderRequestTypeInput[i]];
            Output
                .AppendCommaIf(i != 0)
                .Append(parameter.Name);
        }

        Output.AppendLine("))");
    }

    private void WithResponseHeader(OperationDescription operation)
    {
        Output
            .Append(".WithResponseHeader(Contract.")
            .Append(operation.GrpcMethodOutputHeaderName)
            .AppendLine(")");
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