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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class ContractCodeGenerator : ICodeGenerator
{
    private readonly IContractDescription _contract;

    public ContractCodeGenerator(IContractDescription contract)
    {
        _contract = contract;
    }

    public string GetHintName() => Hints.Contracts(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("internal sealed class ")
            .AppendLine(NamingContract.Contract.Class(_contract.BaseClassName))
            .AppendLine("{");

        using (output.Indent())
        {
            BuildCtor(output);
            output.AppendLine();

            BuildProperties(output);
            BuildGetDefinition(output);
        }

        output.AppendLine("}");
    }

    private void BuildCtor(ICodeStringBuilder output)
    {
        output
            .Append("public ")
            .Append(NamingContract.Contract.Class(_contract.BaseClassName))
            .Append("(")
            .WriteType(typeof(IMarshallerFactory))
            .AppendLine(" marshallerFactory)");

        output.AppendLine("{");

        using (output.Indent())
        {
            output.WriteArgumentNullException("marshallerFactory");

            foreach (var operation in GetAllOperations())
            {
                BuildMethodInitializer(output, operation);
            }
        }

        output.AppendLine("}");
    }

    private void BuildProperties(ICodeStringBuilder output)
    {
        foreach (var operation in GetAllOperations())
        {
            output
                .Append("public ")
                .WriteType(typeof(IMethod))
                .Append(" ")
                .Append(NamingContract.Contract.GrpcMethod(operation.OperationName))
                .AppendLine(" { get; }")
                .AppendLine();
        }
    }

    private void BuildMethodInitializer(ICodeStringBuilder output, IOperationDescription operation)
    {
        // Method = GrpcMethodFactory.Unary<>()
        output
            .Append(NamingContract.Contract.GrpcMethod(operation.OperationName))
            .Append(" = ")
            .WriteType(typeof(GrpcMethodFactory))
            .Append(".")
            .Append(operation.OperationType.ToString())
            .Append("<");

        switch (operation.OperationType)
        {
            case MethodType.Unary:
                output
                    .WriteMessage(operation.RequestType)
                    .Append(", ")
                    .WriteMessage(operation.ResponseType);
                break;
            case MethodType.ClientStreaming:
                output
                    .WriteMessageOrDefault(operation.HeaderRequestType)
                    .Append(", ")
                    .WriteMessage(operation.RequestType)
                    .Append(", ")
                    .WriteMessage(operation.ResponseType);
                break;
            case MethodType.ServerStreaming:
                output
                    .WriteMessage(operation.RequestType)
                    .Append(", ")
                    .WriteMessageOrDefault(operation.HeaderResponseType)
                    .Append(", ")
                    .WriteMessage(operation.ResponseType);
                break;
            case MethodType.DuplexStreaming:
                output
                    .WriteMessageOrDefault(operation.HeaderRequestType)
                    .Append(", ")
                    .WriteMessage(operation.RequestType)
                    .Append(", ")
                    .WriteMessageOrDefault(operation.HeaderResponseType)
                    .Append(", ")
                    .WriteMessage(operation.ResponseType);
                break;
        }

        output
            .Append(">(marshallerFactory, ")
            .WriteString(operation.ServiceName)
            .Append(", ")
            .WriteString(operation.OperationName);

        switch (operation.OperationType)
        {
            case MethodType.ClientStreaming:
                output
                    .Append(", ")
                    .WriteBoolean(operation.HeaderRequestType != null);
                break;
            case MethodType.ServerStreaming:
                output
                    .Append(", ")
                    .WriteBoolean(operation.HeaderResponseType != null);
                break;
            case MethodType.DuplexStreaming:
                output
                    .Append(", ")
                    .WriteBoolean(operation.HeaderRequestType != null)
                    .Append(", ")
                    .WriteBoolean(operation.HeaderResponseType != null);
                break;
        }

        output
            .AppendLine(");");
    }

    private void BuildGetDefinition(ICodeStringBuilder output)
    {
        foreach (var interfaceDescription in _contract.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                AddGetDefinition(output, interfaceDescription.InterfaceType, operation, NamingContract.Contract.ClrDefinitionMethod(operation.OperationName));
                output.AppendLine();
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                AddGetDefinition(output, interfaceDescription.InterfaceType, entry.Sync, NamingContract.Contract.ClrDefinitionMethodSync(entry.Async.OperationName));
                output.AppendLine();
            }
        }
    }

    private void AddGetDefinition(ICodeStringBuilder output, ITypeSymbol interfaceType, IOperationDescription operation, string methodName)
    {
        output
            .Append("public static ")
            .WriteType(typeof(MethodInfo))
            .Append(" ")
            .Append(methodName)
            .AppendLine("()")
            .AppendLine("{");

        using (output.Indent())
        {
            var source = operation.Method;
            for (var i = 0; i < source.Parameters.Length; i++)
            {
                var p = source.Parameters[i];
                if (p.IsOut())
                {
                    output
                        .WriteType(p.Type)
                        .Append(" ")
                        .Append(p.Name)
                        .AppendLine(";");
                }
                else if (p.IsRef())
                {
                    output
                        .WriteType(p.Type)
                        .Append(" ")
                        .Append(p.Name)
                        .AppendLine(" = default;");
                }
            }

            // Expression<Action<IDemoService>> __exp = s => s.Sum(default, default);
            output
                .WriteType(typeof(Expression))
                .Append("<Action<")
                .WriteType(interfaceType)
                .Append(">> __exp = s => s.")
                .Append(operation.Method.Name)
                .Append("(");

            for (var i = 0; i < source.Parameters.Length; i++)
            {
                if (i > 0)
                {
                    output.Append(", ");
                }

                var p = source.Parameters[i];
                if (p.IsOut())
                {
                    output
                        .Append("out ")
                        .Append(p.Name);
                }
                else if (p.IsRef())
                {
                    output
                        .Append("ref ")
                        .Append(p.Name);
                }
                else
                {
                    output
                        .Append("default(")
                        .WriteType(operation.Method.Parameters[i].Type)
                        .Append(")");
                }
            }

            output.AppendLine(");");

            output
                .Append("return ((")
                .WriteType(typeof(MethodCallExpression))
                .AppendLine(")__exp.Body).Method;");
        }

        output.AppendLine("}");
    }

    private IEnumerable<IOperationDescription> GetAllOperations() => _contract.Services.SelectMany(i => i.Operations);
}