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
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

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
            .AppendLine(NamingConventions.Contract.Class(_contract.BaseClassName))
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
            .Append(NamingConventions.Contract.Class(_contract.BaseClassName))
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
                BuildRequestHeaderInitializer(output, operation);
                BuildResponseHeaderInitializer(output, operation);
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
                .WriteType(typeof(Method<,>))
                .WriteMessage(operation.RequestType)
                .Append(", ")
                .WriteMessage(operation.ResponseType)
                .Append("> ")
                .Append(NamingConventions.Contract.GrpcMethod(operation.OperationName))
                .AppendLine(" { get; }")
                .AppendLine();

            if (operation.HeaderRequestType != null)
            {
                output
                    .Append("public ")
                    .WriteType(typeof(Marshaller<>))
                    .WriteMessage(operation.HeaderRequestType)
                    .Append("> ")
                    .Append(NamingConventions.Contract.GrpcMethodInputHeader(operation.OperationName))
                    .AppendLine(" { get; }")
                    .AppendLine();
            }

            if (operation.HeaderResponseType != null)
            {
                output
                    .Append("public ")
                    .WriteType(typeof(Marshaller<>))
                    .WriteMessage(operation.HeaderResponseType)
                    .Append("> ")
                    .Append(NamingConventions.Contract.GrpcMethodOutputHeader(operation.OperationName))
                    .AppendLine(" { get; }")
                    .AppendLine();
            }
        }
    }

    private void BuildMethodInitializer(ICodeStringBuilder output, IOperationDescription operation)
    {
        output
            .Append(NamingConventions.Contract.GrpcMethod(operation.OperationName))
            .Append(" = new ")
            .WriteType(typeof(Method<,>))
            .WriteMessage(operation.RequestType)
            .Append(", ")
            .WriteMessage(operation.ResponseType)
            .Append(">(");

        output
            .WriteType(typeof(MethodType))
            .Append(".")
            .Append(operation.OperationType.ToString())
            .Append(",");

        output
            .Append(" \"")
            .Append(operation.ServiceName)
            .Append("\",");

        output
            .Append(" \"")
            .Append(operation.OperationName)
            .Append("\",");

        output
            .Append(" marshallerFactory.CreateMarshaller<")
            .WriteMessage(operation.RequestType)
            .Append(">(),");

        output
            .Append(" marshallerFactory.CreateMarshaller<")
            .WriteMessage(operation.ResponseType)
            .AppendLine(">());");
    }

    private void BuildRequestHeaderInitializer(ICodeStringBuilder output, IOperationDescription operation)
    {
        if (operation.HeaderRequestType == null)
        {
            return;
        }

        output
            .Append(NamingConventions.Contract.GrpcMethodInputHeader(operation.OperationName))
            .Append(" = marshallerFactory.CreateMarshaller<")
            .WriteMessage(operation.HeaderRequestType)
            .AppendLine(">();");
    }

    private void BuildResponseHeaderInitializer(ICodeStringBuilder output, IOperationDescription operation)
    {
        if (operation.HeaderResponseType == null)
        {
            return;
        }

        output
            .Append(NamingConventions.Contract.GrpcMethodOutputHeader(operation.OperationName))
            .Append(" = marshallerFactory.CreateMarshaller<")
            .WriteMessage(operation.HeaderResponseType)
            .AppendLine(">();");
    }

    private void BuildGetDefinition(ICodeStringBuilder output)
    {
        foreach (var interfaceDescription in _contract.Services)
        {
            foreach (var method in interfaceDescription.Operations)
            {
                AddGetDefinition(output, interfaceDescription.InterfaceType, method);
                output.AppendLine();
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                AddGetDefinition(output, interfaceDescription.InterfaceType, entry.Sync);
                output.AppendLine();
            }
        }
    }

    private void AddGetDefinition(ICodeStringBuilder output, INamedTypeSymbol interfaceType, IOperationDescription operation)
    {
        output
            .Append("public static ")
            .WriteType(typeof(MethodInfo))
            .Append(" ")
            .Append(operation.ClrDefinitionMethodName)
            .AppendLine("()")
            .AppendLine("{");

        using (output.Indent())
        {
            for (var i = 0; i < operation.Method.Parameters.Length; i++)
            {
                var p = operation.Method.Parameters[i];
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

            for (var i = 0; i < operation.Method.Parameters.Length; i++)
            {
                if (i > 0)
                {
                    output.Append(", ");
                }

                var p = operation.Method.Parameters[i];
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