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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;
using ServiceModel.Grpc.Hosting.Internal;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class EndpointBinderCodeGenerator : ICodeGenerator
{
    private readonly IContractDescription _contract;

    public EndpointBinderCodeGenerator(IContractDescription contract)
    {
        _contract = contract;
    }

    public string GetHintName() => Hints.EndpointBinders(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("internal sealed partial class ")
            .Append(NamingConventions.EndpointBinder.Class(_contract.BaseClassName))
            .Append(" : ")
            .WriteType(typeof(IServiceEndpointBinder<>))
            .WriteType(_contract.ContractInterface)
            .AppendLine(">");
        output.AppendLine("{");

        using (output.Indent())
        {
            BuildBind(output);
            output.AppendLine();

            BuildGetServiceMetadata(output);
            output.AppendLine();

            BuildGetServiceMetadataOverride(output);

            foreach (var interfaceDescription in _contract.Services)
            {
                foreach (var operation in interfaceDescription.Operations)
                {
                    output.AppendLine();
                    BuildGetMethodMetadata(output, interfaceDescription, operation);
                    output.AppendLine();
                    BuildGetMethodMetadataOverride(output, operation);
                }
            }
        }

        output.AppendLine("}");
    }

    internal static void WriteNewAttribute(ICodeStringBuilder output, AttributeData attribute)
    {
        output
            .Append("new ")
            .WriteType(attribute.AttributeClass!);

        if (attribute.ConstructorArguments.Length == 0 && attribute.NamedArguments.Length == 0)
        {
            output.Append("()");
            return;
        }

        if (attribute.ConstructorArguments.Length > 0)
        {
            output.Append("(");

            for (var i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                var arg = attribute.ConstructorArguments[i];
                output
                    .WriteCommaIf(i > 0)
                    .Append(arg.ToCSharpString());
            }

            output.Append(")");
        }

        if (attribute.NamedArguments.Length > 0)
        {
            output.Append(" { ");

            for (var i = 0; i < attribute.NamedArguments.Length; i++)
            {
                var arg = attribute.NamedArguments[i];
                output
                    .WriteCommaIf(i > 0)
                    .Append(arg.Key)
                    .Append(" = ")
                    .Append(arg.Value.ToCSharpString());
            }

            output.Append(" }");
        }
    }

    internal static IEnumerable<AttributeData> FilterAttributes(ImmutableArray<AttributeData> attributes)
    {
        for (var i = 0; i < attributes.Length; i++)
        {
            var attribute = attributes[i];
            var ns = SyntaxTools.GetNamespace(attribute.AttributeClass!);
            if (!string.IsNullOrEmpty(ns)
                && !ns!.StartsWith("System.Runtime.CompilerServices", StringComparison.OrdinalIgnoreCase)
                && !ns.StartsWith("System.Diagnostics", StringComparison.OrdinalIgnoreCase)
                && !ns.StartsWith("System.ServiceModel", StringComparison.OrdinalIgnoreCase))
            {
                yield return attribute;
            }
        }
    }

    private void BuildBind(ICodeStringBuilder output)
    {
        output
            .Append("public void Bind(")
            .WriteType(typeof(IServiceMethodBinder<>))
            .WriteType(_contract.ContractInterface)
            .AppendLine("> methodBinder)")
            .AppendLine("{");

        using (output.Indent())
        {
            output.WriteArgumentNullException("methodBinder");

            output
                .Append("var contract = new ")
                .Append(NamingConventions.Contract.Class(_contract.BaseClassName))
                .AppendLine("(methodBinder.MarshallerFactory);");

            output
                .Append("var endpoint = new ")
                .Append(NamingConventions.Endpoint.Class(_contract.BaseClassName))
                .AppendLine("();");

            foreach (var interfaceDescription in _contract.Services)
            {
                foreach (var method in interfaceDescription.Operations)
                {
                    output
                        .Append("methodBinder.Add")
                        .Append(method.OperationType.ToString())
                        .Append("Method(contract.")
                        .Append(NamingConventions.Contract.GrpcMethod(method.OperationName))
                        .Append(", ")
                        .Append(NamingConventions.Contract.Class(_contract.BaseClassName))
                        .Append(".")
                        .Append(method.ClrDefinitionMethodName);

                    if (method.OperationType == MethodType.ClientStreaming)
                    {
                        WriteHeaderMarshaller(output, method.HeaderRequestType, NamingConventions.Contract.GrpcMethodInputHeader(method.OperationName));
                    }
                    else if (method.OperationType == MethodType.ServerStreaming)
                    {
                        WriteHeaderMarshaller(output, method.HeaderResponseType, NamingConventions.Contract.GrpcMethodOutputHeader(method.OperationName));
                    }
                    else if (method.OperationType == MethodType.DuplexStreaming)
                    {
                        WriteHeaderMarshaller(output, method.HeaderRequestType, NamingConventions.Contract.GrpcMethodInputHeader(method.OperationName));
                        WriteHeaderMarshaller(output, method.HeaderResponseType, NamingConventions.Contract.GrpcMethodOutputHeader(method.OperationName));
                    }

                    output
                        .Append(", ")
                        .Append(GetMethodMetadataName(method))
                        .Append("(), endpoint.")
                        .Append(method.OperationName)
                        .AppendLine(");");
                }
            }
        }

        output.AppendLine("}");
    }

    private void WriteHeaderMarshaller(ICodeStringBuilder output, IMessageDescription? description, string propertyName)
    {
        output.Append(", ");
        if (description == null)
        {
            // (Marshaller<Message>)null
            output
                .Append("(")
                .WriteType(typeof(Marshaller<>))
                .WriteType(typeof(Message))
                .Append(">)null");
        }
        else
        {
            output
                .Append("contract.")
                .Append(propertyName);
        }
    }

    private void BuildGetServiceMetadata(ICodeStringBuilder output)
    {
        output
            .AppendLine("private void ServiceGetMetadata(IList<object> metadata)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .Append("// copy attributes from ")
                .Append(_contract.ContractInterface.TypeKind.ToString().ToLowerInvariant())
                .Append(" ")
                .AppendLine(_contract.ContractInterface.Name);

            var length = output.GetLength();

            foreach (var attribute in FilterAttributes(_contract.ContractInterface.GetAttributes()))
            {
                output.Append("metadata.Add(");
                WriteNewAttribute(output, attribute);
                output.AppendLine(");");
            }

            if (output.GetLength() == length)
            {
                output.AppendLine("// no applicable attributes found");
            }

            output.AppendLine("ServiceGetMetadataOverride(metadata);");
        }

        output.AppendLine("}");
    }

    private void BuildGetServiceMetadataOverride(ICodeStringBuilder output) =>
        output.AppendLine("partial void ServiceGetMetadataOverride(IList<object> metadata);");

    private void BuildGetMethodMetadata(ICodeStringBuilder output, IInterfaceDescription interfaceDescription, IOperationDescription operation)
    {
        output
            .Append("private IList<object> ")
            .Append(GetMethodMetadataName(operation))
            .AppendLine("()")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .AppendLine("var metadata = new List<object>();")
                .AppendLine("ServiceGetMetadata(metadata);");

            var implementation = operation.Method;
            if (SyntaxTools.IsInterface(_contract.ContractInterface))
            {
                output
                    .Append("// copy attributes from method ")
                    .Append(interfaceDescription.InterfaceType.Name)
                    .Append(".")
                    .AppendLine(implementation.Name);
            }
            else
            {
                implementation = _contract.ContractInterface.GetInterfaceImplementation(operation.Method);
                output
                    .Append("// copy attributes from method ")
                    .Append(implementation.Name)
                    .Append(", implementation of ")
                    .Append(interfaceDescription.InterfaceType.Name)
                    .Append(".")
                    .AppendLine(operation.Method.Name);
            }

            var length = output.GetLength();

            foreach (var attribute in FilterAttributes(implementation.GetAttributes()))
            {
                output.Append("metadata.Add(");
                WriteNewAttribute(output, attribute);
                output.AppendLine(");");
            }

            if (output.GetLength() == length)
            {
                output.AppendLine("// no applicable attributes found");
            }

            output
                .Append(GetMethodMetadataName(operation))
                .AppendLine("Override(metadata);")
                .AppendLine("return metadata;");
        }

        output.AppendLine("}");
    }

    private void BuildGetMethodMetadataOverride(ICodeStringBuilder output, IOperationDescription operation)
    {
        output
            .Append("partial void ")
            .Append(GetMethodMetadataName(operation))
            .AppendLine("Override(IList<object> metadata);");
    }

    private string GetMethodMetadataName(IOperationDescription operation) =>
        "Method" + operation.OperationName + "GetMetadata";
}