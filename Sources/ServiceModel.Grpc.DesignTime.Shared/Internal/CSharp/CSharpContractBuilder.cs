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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

internal sealed class CSharpContractBuilder : CodeGeneratorBase
{
    private readonly ContractDescription _contract;

    public CSharpContractBuilder(ContractDescription contract)
    {
        _contract = contract;
    }

    public override string GetGeneratedMemberName() => _contract.ContractClassName;

    protected override void Generate()
    {
        WriteMetadata();
        Output.AppendLine($"internal sealed class {_contract.ContractClassName}");
        Output.AppendLine("{");

        using (Output.Indent())
        {
            BuildCtor();
            Output.AppendLine();

            BuildProperties();
            BuildGetDefinition();
        }

        Output.AppendLine("}");
    }

    private void BuildCtor()
    {
        Output
            .Append("public ")
            .Append(_contract.ContractClassName)
            .Append("(")
            .AppendType(typeof(IMarshallerFactory))
            .AppendLine(" marshallerFactory)");

        Output.AppendLine("{");

        using (Output.Indent())
        {
            Output.AppendArgumentNullException("marshallerFactory");

            foreach (var operation in GetAllOperations())
            {
                BuildMethodInitializer(operation);
                BuildRequestHeaderInitializer(operation);
                BuildResponseHeaderInitializer(operation);
            }
        }

        Output.AppendLine("}");
    }

    private void BuildProperties()
    {
        foreach (var operation in GetAllOperations())
        {
            Output
                .Append("public ")
                .AppendType(typeof(Method<,>))
                .AppendMessage(operation.RequestType)
                .Append(", ")
                .AppendMessage(operation.ResponseType)
                .Append("> ")
                .Append(operation.GrpcMethodName)
                .AppendLine(" { get; }")
                .AppendLine();

            if (operation.HeaderRequestType != null)
            {
                Output
                    .Append("public ")
                    .AppendType(typeof(Marshaller<>))
                    .AppendMessage(operation.HeaderRequestType)
                    .Append("> ")
                    .Append(operation.GrpcMethodInputHeaderName)
                    .AppendLine(" { get; }")
                    .AppendLine();
            }

            if (operation.HeaderResponseType != null)
            {
                Output
                    .Append("public ")
                    .AppendType(typeof(Marshaller<>))
                    .AppendMessage(operation.HeaderResponseType)
                    .Append("> ")
                    .Append(operation.GrpcMethodOutputHeaderName)
                    .AppendLine(" { get; }")
                    .AppendLine();
            }
        }
    }

    private void BuildMethodInitializer(OperationDescription operation)
    {
        Output
            .Append(operation.GrpcMethodName)
            .Append(" = new ")
            .AppendType(typeof(Method<,>))
            .AppendMessage(operation.RequestType)
            .Append(", ")
            .AppendMessage(operation.ResponseType)
            .Append(">(");

        Output
            .AppendType(typeof(MethodType))
            .Append(".")
            .Append(operation.OperationType.ToString())
            .Append(",");

        Output
            .Append(" \"")
            .Append(operation.ServiceName)
            .Append("\",");

        Output
            .Append(" \"")
            .Append(operation.OperationName)
            .Append("\",");

        Output
            .Append(" marshallerFactory.CreateMarshaller<")
            .AppendMessage(operation.RequestType)
            .Append(">(),");

        Output
            .Append(" marshallerFactory.CreateMarshaller<")
            .AppendMessage(operation.ResponseType)
            .AppendLine(">());");
    }

    private void BuildRequestHeaderInitializer(OperationDescription operation)
    {
        if (operation.HeaderRequestType == null)
        {
            return;
        }

        Output
            .Append(operation.GrpcMethodInputHeaderName)
            .Append(" = marshallerFactory.CreateMarshaller<")
            .AppendMessage(operation.HeaderRequestType)
            .AppendLine(">();");
    }

    private void BuildResponseHeaderInitializer(OperationDescription operation)
    {
        if (operation.HeaderResponseType == null)
        {
            return;
        }

        Output
            .Append(operation.GrpcMethodOutputHeaderName)
            .Append(" = marshallerFactory.CreateMarshaller<")
            .AppendMessage(operation.HeaderResponseType)
            .AppendLine(">();");
    }

    private void BuildGetDefinition()
    {
        foreach (var interfaceDescription in _contract.Services)
        {
            foreach (var method in interfaceDescription.Operations)
            {
                AddGetDefinition(interfaceDescription.InterfaceTypeName, method);
                Output.AppendLine();
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                AddGetDefinition(interfaceDescription.InterfaceTypeName, entry.Sync);
                Output.AppendLine();
            }
        }
    }

    private IEnumerable<OperationDescription> GetAllOperations()
    {
        return _contract.Services.SelectMany(i => i.Operations);
    }

    private void AddGetDefinition(string interfaceType, OperationDescription operation)
    {
        Output
            .Append("public static ")
            .AppendType(typeof(MethodInfo))
            .Append(" ")
            .Append(operation.ClrDefinitionMethodName)
            .AppendLine("()")
            .AppendLine("{");

        using (Output.Indent())
        {
            for (var i = 0; i < operation.Method.Parameters.Length; i++)
            {
                var p = operation.Method.Parameters[i];
                if (p.IsOut)
                {
                    Output
                        .Append(p.Type)
                        .Append(" ")
                        .Append(p.Name)
                        .AppendLine(";");
                }
                else if (p.IsRef)
                {
                    Output
                        .Append(p.Type)
                        .Append(" ")
                        .Append(p.Name)
                        .AppendLine(" = default;");
                }
            }

            // Expression<Action<IDemoService>> __exp = s => s.Sum(default, default);
            Output
                .AppendType(typeof(Expression))
                .Append("<Action<")
                .Append(interfaceType)
                .Append(">> __exp = s => s.")
                .Append(operation.Method.Name)
                .Append("(");

            for (var i = 0; i < operation.Method.Parameters.Length; i++)
            {
                if (i > 0)
                {
                    Output.Append(", ");
                }

                var p = operation.Method.Parameters[i];
                if (p.IsOut)
                {
                    Output
                        .Append("out ")
                        .Append(p.Name);
                }
                else if (p.IsRef)
                {
                    Output
                        .Append("ref ")
                        .Append(p.Name);
                }
                else
                {
                    Output
                        .Append("default(")
                        .Append(operation.Method.Parameters[i].Type)
                        .Append(")");
                }
            }

            Output.AppendLine(");");

            Output
                .Append("return ((")
                .AppendType(typeof(MethodCallExpression))
                .AppendLine(")__exp.Body).Method;");
        }

        Output.AppendLine("}");
    }
}