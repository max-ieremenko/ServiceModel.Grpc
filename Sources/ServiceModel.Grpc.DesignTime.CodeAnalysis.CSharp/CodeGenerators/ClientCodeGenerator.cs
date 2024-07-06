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

using System;
using System.Collections.Generic;
using System.Globalization;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class ClientCodeGenerator : ICodeGenerator
{
    private const string VarCallOptionsBuilder = "__callOptionsBuilder";

    private readonly IContractDescription _contract;
    private readonly HashSet<string> _uniqueMemberNames;

    public ClientCodeGenerator(IContractDescription contract)
    {
        _contract = contract;
        _uniqueMemberNames = new(StringComparer.OrdinalIgnoreCase);
    }

    public string GetHintName() => Hints.Clients(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("internal sealed class ")
            .Append(NamingContract.Client.Class(_contract.BaseClassName))
            .Append(" : ")
            .WriteType(typeof(ClientBase<>))
            .AppendFormat("{0}>, ", NamingContract.Client.Class(_contract.BaseClassName))
            .WriteType(_contract.ContractInterface)
            .AppendLine();
        output.AppendLine("{");

        using (output.Indent())
        {
            BuildCtorCallInvoker(output);
            output.AppendLine();

            BuildCtorConfiguration(output);
            output.AppendLine();

            BuildProperties(output);

            foreach (var interfaceDescription in _contract.Interfaces)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(output, interfaceDescription.InterfaceType, method);
                    output.AppendLine();
                }
            }

            foreach (var interfaceDescription in _contract.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(output, interfaceDescription.InterfaceType, method);
                    output.AppendLine();
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    ImplementNotSupportedMethod(output, interfaceDescription.InterfaceType, method);
                    output.AppendLine();
                }

                foreach (var method in interfaceDescription.Operations)
                {
                    ImplementMethod(output, interfaceDescription.InterfaceType, method, null);
                    output.AppendLine();
                }

                foreach (var entry in interfaceDescription.SyncOverAsync)
                {
                    ImplementMethod(output, interfaceDescription.InterfaceType, entry.Sync, NamingContract.Contract.GrpcMethod(entry.Async.OperationName));
                    output.AppendLine();
                }
            }

            BuildMethodNewInstance(output);
        }

        output.AppendLine("}");
    }

    private void BuildCtorCallInvoker(ICodeStringBuilder output)
    {
        output
            .Append("public ")
            .Append(NamingContract.Client.Class(_contract.BaseClassName))
            .Append("(")
            .WriteType(typeof(CallInvoker)).Append(" callInvoker, ")
            .Append(NamingContract.Contract.Class(_contract.BaseClassName)).Append(" contract, ")
            .WriteType(typeof(IClientCallInvoker))
            .AppendLine(" clientCallInvoker)");

        using (output.Indent())
        {
            output.AppendLine(": base(callInvoker)");
        }

        output.AppendLine("{");
        using (output.Indent())
        {
            output.WriteArgumentNullException("contract");

            output.AppendLine("Contract = contract;");
            output.AppendLine("ClientCallInvoker = clientCallInvoker;");
        }

        output.AppendLine("}");
    }

    private void BuildCtorConfiguration(ICodeStringBuilder output)
    {
        output
            .Append("private ")
            .Append(NamingContract.Client.Class(_contract.BaseClassName))
            .Append("(")
            .Append("ClientBaseConfiguration configuration, ")
            .Append(NamingContract.Contract.Class(_contract.BaseClassName)).Append(" contract, ")
            .WriteType(typeof(IClientCallInvoker))
            .AppendLine(" clientCallInvoker)");

        using (output.Indent())
        {
            output.AppendLine(": base(configuration)");
        }

        output.AppendLine("{");
        using (output.Indent())
        {
            output.AppendLine("Contract = contract;");
            output.AppendLine("ClientCallInvoker = clientCallInvoker;");
        }

        output.AppendLine("}");
    }

    private void BuildMethodNewInstance(ICodeStringBuilder output)
    {
        output
            .Append("protected override ")
            .Append(NamingContract.Client.Class(_contract.BaseClassName))
            .AppendLine(" NewInstance(ClientBaseConfiguration configuration)");

        output.AppendLine("{");
        using (output.Indent())
        {
            output
                .Append("return new ")
                .Append(NamingContract.Client.Class(_contract.BaseClassName))
                .AppendLine("(configuration, Contract, ClientCallInvoker);");
        }

        output.AppendLine("}");
    }

    private void BuildProperties(ICodeStringBuilder output)
    {
        output
            .Append("public ")
            .Append(NamingContract.Contract.Class(_contract.BaseClassName))
            .AppendLine(" Contract { get; }")
            .AppendLine();

        output
            .Append("public ")
            .WriteType(typeof(IClientCallInvoker))
            .AppendLine(" ClientCallInvoker  { get; }")
            .AppendLine();
    }

    private void ImplementMethod(ICodeStringBuilder output, ITypeSymbol interfaceType, IOperationDescription operation, string? grpcMethodName)
    {
        CreateMethodWithSignature(output, interfaceType, operation.Method);
        output.AppendLine("{");

        Action? adapterBuilder = null;
        using (output.Indent())
        {
            switch (operation.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(output, operation, grpcMethodName);
                    break;
                case MethodType.ClientStreaming:
                    BuildClientStreaming(output, operation);
                    break;
                case MethodType.ServerStreaming:
                    adapterBuilder = BuildServerStreaming(output, operation);
                    break;
                case MethodType.DuplexStreaming:
                    adapterBuilder = BuildDuplexStreaming(output, operation);
                    break;
                default:
                    throw new NotImplementedException($"{operation.OperationType} operation is not implemented.");
            }
        }

        output.AppendLine("}");

        adapterBuilder?.Invoke();
    }

    private void BuildUnary(ICodeStringBuilder output, IOperationDescription operation, string? grpcMethodName)
    {
        InitializeCallOptionsBuilderVariable(output, operation);

        var hasReturn = operation.IsAsync || operation.ResponseType.Properties.Length > 0;

        // var __response = ClientCallInvoker.UnaryInvoke<TRequest, TResponse>(CallInvoker, method, __callOptionsBuilder, )
        output
            .Append(hasReturn ? "var __response = " : string.Empty)
            .Append("ClientCallInvoker.")
            .Append(operation.IsAsync ? nameof(IClientCallInvoker.UnaryInvokeAsync) : nameof(IClientCallInvoker.UnaryInvoke))
            .Append("<")
            .WriteMessage(operation.RequestType)
            .Append(", ")
            .WriteMessage(operation.ResponseType);

        if (operation.ResponseType.Properties.Length > 0)
        {
            output
                .Append(", ")
                .WriteType(operation.ResponseType.Properties[0]);
        }

        output
            .Append(">(CallInvoker, Contract.")
            .Append(grpcMethodName ?? NamingContract.Contract.GrpcMethod(operation.OperationName))
            .Append(", ")
            .Append(VarCallOptionsBuilder)
            .Append(", ");

        CreateRequestMessage(output, operation);

        output.AppendLine(");");

        if (hasReturn)
        {
            output.Append("return ");

            if (operation.Method.ReturnType.IsValueTask())
            {
                output
                    .Append("new ")
                    .WriteType(operation.Method.ReturnType)
                    .AppendLine("(__response);");
            }
            else
            {
                output.AppendLine("__response;");
            }
        }
    }

    private void BuildClientStreaming(ICodeStringBuilder output, IOperationDescription operation)
    {
        InitializeCallOptionsBuilderVariable(output, operation);

        // var __response = ClientCallInvoker.ClientInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponse>(CallInvoker, method, __callOptionsBuilder, )
        output
            .Append("var __response = ClientCallInvoker.")
            .Append(nameof(IClientCallInvoker.ClientInvokeAsync))
            .Append("<")
            .WriteMessageOrDefault(operation.HeaderRequestType)
            .Append(", ")
            .WriteMessage(operation.RequestType)
            .Append(", ")
            .WriteType(operation.RequestType.Properties[0])
            .Append(", ")
            .WriteMessage(operation.ResponseType);

        if (operation.ResponseType.Properties.Length > 0)
        {
            output
                .Append(", ")
                .WriteType(operation.ResponseType.Properties[0]);
        }

        output
            .Append(">(CallInvoker, Contract.")
            .Append(NamingContract.Contract.GrpcMethod(operation.OperationName))
            .Append(", ")
            .Append(VarCallOptionsBuilder)
            .Append(", ");

        WithRequestHeader(output, operation);

        output
            .Append(", ")
            .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name)
            .AppendLine(");");

        output.Append("return ");
        if (operation.Method.ReturnType.IsValueTask())
        {
            output
                .Append("new ")
                .WriteType(operation.Method.ReturnType)
                .Append("(__response)");
        }
        else
        {
            output.Append("__response");
        }

        output.AppendLine(";");
    }

    private Action? BuildServerStreaming(ICodeStringBuilder output, IOperationDescription operation)
    {
        InitializeCallOptionsBuilderVariable(output, operation);

        // var __response = ClientCallInvoker.ServerInvokeAsync<TRequest, TResponseHeader, TResponse, TResponseValue>(CallInvoker, method, __callOptionsBuilder, )
        output
            .Append("var __response = ClientCallInvoker.")
            .Append(operation.IsAsync ? nameof(IClientCallInvoker.ServerInvokeAsync) : nameof(IClientCallInvoker.ServerInvoke))
            .Append("<")
            .WriteMessage(operation.RequestType)
            .Append(", ")
            .WriteMessageOrDefault(operation.HeaderResponseType)
            .Append(", ")
            .WriteMessage(operation.ResponseType)
            .Append(", ")
            .WriteType(operation.ResponseType.Properties[0]);

        if (operation.HeaderResponseType != null)
        {
            output
                .Append(", ")
                .WriteType(operation.Method.ReturnType.GenericTypeArguments()[0]);
        }

        output
            .Append(">(CallInvoker, Contract.")
            .Append(NamingContract.Contract.GrpcMethod(operation.OperationName))
            .Append(", ")
            .Append(VarCallOptionsBuilder)
            .Append(", ");

        CreateRequestMessage(output, operation);

        Action? adapterBuilder = null;
        if (operation.HeaderResponseType != null)
        {
            var adapterFunctionName = GetUniqueMemberName("Adapt" + operation.Method.Name + "Response");
            adapterBuilder = () => BuildServerStreamingResultAdapter(output, operation, adapterFunctionName);
            output
                .Append(", ")
                .Append(adapterFunctionName);
        }

        output
            .AppendLine(");")
            .Append("return ");

        if (operation.IsAsync && operation.Method.ReturnType.IsValueTask())
        {
            output
                .Append("new ")
                .WriteType(operation.Method.ReturnType)
                .Append("(__response)");
        }
        else
        {
            output.Append("__response");
        }

        output.AppendLine(";");
        return adapterBuilder;
    }

    private void BuildServerStreamingResultAdapter(ICodeStringBuilder output, IOperationDescription operation, string functionName)
    {
        var returnType = operation.Method.ReturnType.GenericTypeArguments()[0];
        output
            .Append("private static ")
            .WriteType(returnType)
            .Append(" ")
            .Append(functionName)
            .Append("(")
            .WriteMessage(operation.HeaderResponseType!)
            .Append(" header, IAsyncEnumerable<")
            .WriteType(operation.ResponseType.Properties[0])
            .Append(">")
            .AppendLine(" response)");

        output.AppendLine("{");
        using (output.Indent())
        {
            output
                .Append("return new ")
                .WriteType(returnType)
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

                output
                    .WriteCommaIf(i > 0)
                    .Append(value);
            }

            output.AppendLine(");");
        }

        output.AppendLine("}");
    }

    private Action? BuildDuplexStreaming(ICodeStringBuilder output, IOperationDescription operation)
    {
        InitializeCallOptionsBuilderVariable(output, operation);

        // var __response = ClientCallInvoker.DuplexInvoke<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(CallInvoker, method, __callOptionsBuilder, )
        output
            .Append("var __response = ClientCallInvoker.")
            .Append(operation.IsAsync ? nameof(IClientCallInvoker.DuplexInvokeAsync) : nameof(IClientCallInvoker.DuplexInvoke))
            .Append("<")
            .WriteMessageOrDefault(operation.HeaderRequestType)
            .Append(", ")
            .WriteMessage(operation.RequestType)
            .Append(", ")
            .WriteType(operation.RequestType.Properties[0])
            .Append(", ")
            .WriteMessageOrDefault(operation.HeaderResponseType)
            .Append(", ")
            .WriteMessage(operation.ResponseType)
            .Append(", ")
            .WriteType(operation.ResponseType.Properties[0]);

        if (operation.HeaderResponseType != null)
        {
            output
                .Append(", ")
                .WriteType(operation.Method.ReturnType.GenericTypeArguments()[0]);
        }

        output
            .Append(">(CallInvoker, Contract.")
            .Append(NamingContract.Contract.GrpcMethod(operation.OperationName))
            .Append(", ")
            .Append(VarCallOptionsBuilder)
            .Append(", ");

        WithRequestHeader(output, operation);

        output
            .Append(", ")
            .Append(operation.Method.Parameters[operation.RequestTypeInput[0]].Name);

        Action? adapterBuilder = null;
        if (operation.HeaderResponseType != null)
        {
            var adapterFunctionName = GetUniqueMemberName("Adapt" + operation.Method.Name + "Response");
            adapterBuilder = () => BuildServerStreamingResultAdapter(output, operation, adapterFunctionName);
            output
                .Append(", ")
                .Append(adapterFunctionName);
        }

        output
            .AppendLine(");")
            .Append("return ");

        if (operation.IsAsync && operation.Method.ReturnType.IsValueTask())
        {
            output
                .Append("new ")
                .WriteType(operation.Method.ReturnType)
                .Append("(__response)");
        }
        else
        {
            output.Append("__response");
        }

        output.AppendLine(";");
        return adapterBuilder;
    }

    private void ImplementNotSupportedMethod(ICodeStringBuilder output, ITypeSymbol interfaceType, INotSupportedMethodDescription method)
    {
        CreateMethodWithSignature(output, interfaceType, method.Method);

        output.AppendLine("{");
        using (output.Indent())
        {
            output.WriteNotSupportedException(method.Error);
        }

        output.AppendLine("}");
    }

    private void CreateMethodWithSignature(ICodeStringBuilder output, ITypeSymbol interfaceType, IMethodSymbol method)
    {
        output
            .WriteType(method.ReturnType)
            .Append(" ")
            .WriteType(interfaceType)
            .Append(".")
            .Append(method.Name);

        if (method.TypeArguments.Length != 0)
        {
            output.Append("<");
            for (var i = 0; i < method.TypeArguments.Length; i++)
            {
                if (i > 0)
                {
                    output.Append(", ");
                }

                output.WriteType(method.TypeArguments[i]);
            }

            output.Append(">");
        }

        output.Append("(");
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            if (i > 0)
            {
                output.Append(", ");
            }

            var p = method.Parameters[i];
            if (p.IsOut())
            {
                output.Append("out ");
            }
            else if (p.IsRef())
            {
                output.Append("ref ");
            }

            output
                .WriteType(p.Type)
                .Append(" ")
                .Append(p.Name);
        }

        output.AppendLine(")");
    }

    private void InitializeCallOptionsBuilderVariable(ICodeStringBuilder output, IOperationDescription operation)
    {
        output
            .Append("var ")
            .Append(VarCallOptionsBuilder)
            .Append(" = ClientCallInvoker.")
            .Append(nameof(IClientCallInvoker.CreateOptionsBuilder))
            .Append("()");

        using (output.Indent())
        {
            for (var i = 0; i < operation.ContextInput.Length; i++)
            {
                var parameter = operation.Method.Parameters[operation.ContextInput[i]];
                var type = SyntaxTools.IsNullable(parameter.Type) ? parameter.Type.GenericTypeArguments()[0] : parameter.Type;
                output
                    .AppendLine()
                    .Append(".With").Append(type.Name).Append("(").Append(parameter.Name).Append(")");
            }

            output.AppendLine(";");
        }
    }

    private void CreateRequestMessage(ICodeStringBuilder output, IOperationDescription operation)
    {
        output
            .Append("new ")
            .WriteMessage(operation.RequestType)
            .Append("(");

        for (var i = 0; i < operation.RequestTypeInput.Length; i++)
        {
            var parameter = operation.Method.Parameters[operation.RequestTypeInput[i]];
            output
                .WriteCommaIf(i != 0)
                .Append(parameter.Name);
        }

        output.Append(")");
    }

    private void WithRequestHeader(ICodeStringBuilder output, IOperationDescription operation)
    {
        if (operation.HeaderRequestType == null)
        {
            output.Append("null");
            return;
        }

        output
            .Append("new ")
            .WriteMessage(operation.HeaderRequestType)
            .Append("(");

        for (var i = 0; i < operation.HeaderRequestTypeInput.Length; i++)
        {
            var parameter = operation.Method.Parameters[operation.HeaderRequestTypeInput[i]];
            output
                .WriteCommaIf(i != 0)
                .Append(parameter.Name);
        }

        output.Append(")");
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