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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpServiceEndpointBinderBuilder : CodeGeneratorBase
    {
        private readonly ContractDescription _contract;

        public CSharpServiceEndpointBinderBuilder(ContractDescription contract)
        {
            _contract = contract;
        }

        public override string GetGeneratedMemberName() => _contract.EndpointBinderClassName;

        public override IEnumerable<string> GetUsing()
        {
            yield return typeof(IServiceMethodBinder<>).Namespace!;
            yield return typeof(IServiceEndpointBinder<>).Namespace!;
        }

        internal static void WriteNewAttribute(CodeStringBuilder output, AttributeData attribute)
        {
            output
                .Append("new ")
                .Append(SyntaxTools.GetFullName(attribute.AttributeClass!));

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
                    if (i > 0)
                    {
                        output.Append(", ");
                    }

                    output.Append(arg.ToCSharpString());
                }

                output.Append(")");
            }

            if (attribute.NamedArguments.Length > 0)
            {
                output.Append(" { ");

                for (var i = 0; i < attribute.NamedArguments.Length; i++)
                {
                    var arg = attribute.NamedArguments[i];
                    if (i > 0)
                    {
                        output.Append(", ");
                    }

                    output
                        .Append(arg.Key)
                        .Append(" = ")
                        .Append(arg.Value.ToCSharpString());
                }

                output.Append(" }");
            }
        }

        internal static IEnumerable<AttributeData> FilterAttributes(ImmutableArray<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
            {
                var ns = SyntaxTools.GetNamespace(attribute.AttributeClass!);
                if (!string.IsNullOrEmpty(ns)
                    && !ns.StartsWith("System.Runtime.CompilerServices", StringComparison.OrdinalIgnoreCase)
                    && !ns.StartsWith("System.Diagnostics", StringComparison.OrdinalIgnoreCase)
                    && !ns.StartsWith("System.ServiceModel", StringComparison.OrdinalIgnoreCase))
                {
                    yield return attribute;
                }
            }
        }

        protected override void Generate()
        {
            WriteMetadata();
            Output
                .Append("internal sealed partial class ")
                .Append(GetGeneratedMemberName())
                .Append(" : ")
                .Append(nameof(IServiceEndpointBinder<string>))
                .Append("<")
                .Append(_contract.ContractInterfaceName)
                .AppendLine(">");
            Output.AppendLine("{");

            using (Output.Indent())
            {
                BuildBind();
                Output.AppendLine();

                BuildGetServiceMetadata();
                Output.AppendLine();

                BuildGetServiceMetadataOverride();

                foreach (var interfaceDescription in _contract.Services)
                {
                    foreach (var method in interfaceDescription.Operations)
                    {
                        Output.AppendLine();
                        BuildGetMethodMetadata(interfaceDescription, method);
                        Output.AppendLine();
                        BuildGetMethodMetadataOverride(method);
                    }
                }
            }

            Output.AppendLine("}");
        }

        private void BuildBind()
        {
            Output
                .Append("public void Bind(")
                .Append(nameof(IServiceMethodBinder<string>))
                .Append("<")
                .Append(_contract.ContractInterfaceName)
                .AppendLine("> methodBinder)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output.AppendLine("if (methodBinder == null) throw new ArgumentNullException(\"methodBinder\");");

                Output
                    .Append("var contract = new ")
                    .Append(_contract.ContractClassName)
                    .AppendLine("(methodBinder.MarshallerFactory);");

                Output
                    .Append("var endpoint = new ")
                    .Append(_contract.EndpointClassName)
                    .AppendLine("(contract);");

                foreach (var interfaceDescription in _contract.Services)
                {
                    foreach (var method in interfaceDescription.Operations)
                    {
                        Output
                            .Append("methodBinder.Add")
                            .Append(method.OperationType.ToString())
                            .Append("Method(contract.")
                            .Append(method.GrpcMethodName)
                            .Append(", ")
                            .Append("methodBinder.RequiresMetadata ? ")
                            .Append(GetMethodMetadataName(method))
                            .Append("() : Array.Empty<object>(), endpoint.")
                            .Append(method.OperationName)
                            .AppendLine(");");
                    }
                }
            }

            Output.AppendLine("}");
        }

        private void BuildGetServiceMetadata()
        {
            Output
                .AppendLine("private void ServiceGetMetadata(IList<object> metadata)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output
                    .Append("// copy attributes from ")
                    .Append(_contract.ContractInterface.TypeKind.ToString().ToLowerInvariant())
                    .Append(" ")
                    .AppendLine(_contract.ContractInterface.Name);

                var length = Output.Length;

                foreach (var attribute in FilterAttributes(_contract.ContractInterface.GetAttributes()))
                {
                    Output.Append("metadata.Add(");
                    WriteNewAttribute(Output, attribute);
                    Output.AppendLine(");");
                }

                if (Output.Length == length)
                {
                    Output.AppendLine("// no applicable attributes found");
                }

                Output.AppendLine("ServiceGetMetadataOverride(metadata);");
            }

            Output.AppendLine("}");
        }

        private void BuildGetServiceMetadataOverride()
        {
            Output.AppendLine("partial void ServiceGetMetadataOverride(IList<object> metadata);");
        }

        private void BuildGetMethodMetadata(InterfaceDescription interfaceDescription, OperationDescription method)
        {
            Output
                .Append("private IList<object> ")
                .Append(GetMethodMetadataName(method))
                .AppendLine("()")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output
                    .AppendLine("var metadata = new List<object>();")
                    .AppendLine("ServiceGetMetadata(metadata);");

                var implementation = method.Method.Source;
                if (SyntaxTools.IsInterface(_contract.ContractInterface))
                {
                    Output
                        .Append("// copy attributes from method ")
                        .Append(interfaceDescription.InterfaceType.Name)
                        .Append(".")
                        .AppendLine(implementation.Name);
                }
                else
                {
                    implementation = _contract.ContractInterface.GetInterfaceImplementation(method.Method.Source);
                    Output
                        .Append("// copy attributes from method ")
                        .Append(implementation.Name)
                        .Append(", implementation of ")
                        .Append(interfaceDescription.InterfaceType.Name)
                        .Append(".")
                        .AppendLine(method.Method.Name);
                }

                var length = Output.Length;

                foreach (var attribute in FilterAttributes(implementation.GetAttributes()))
                {
                    Output.Append("metadata.Add(");
                    WriteNewAttribute(Output, attribute);
                    Output.AppendLine(");");
                }

                if (Output.Length == length)
                {
                    Output.AppendLine("// no applicable attributes found");
                }

                Output
                    .Append(GetMethodMetadataName(method))
                    .AppendLine("Override(metadata);")
                    .AppendLine("return metadata;");
            }

            Output.AppendLine("}");
        }

        private void BuildGetMethodMetadataOverride(OperationDescription method)
        {
            Output
                .Append("partial void ")
                .Append(GetMethodMetadataName(method))
                .AppendLine("Override(IList<object> metadata);");
        }

        private string GetMethodMetadataName(OperationDescription method)
        {
            return "Method" + method.OperationName + "GetMetadata";
        }
    }
}
