// <copyright>
// Copyright 2023-2024 Max Ieremenko
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
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class ClientDiCodeGenerator : ICodeGenerator
{
    private readonly ContractDescription<ITypeSymbol> _contract;
    private readonly bool _isStaticClass;

    public ClientDiCodeGenerator(ContractDescription<ITypeSymbol> contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public string GetHintName() => Hints.ClientDiExtensions(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        GenerateClientFactoryBuilder(output);

        output.AppendLine();
        GenerateServiceCollection(output);

        output.AppendLine();
        GenerateHttpClientBuilder(output);
    }

    private void GenerateClientFactoryBuilder(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public static ")
            .WriteTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IClientFactoryBuilder")
            .Append(" Add")
            .Append(NamingConventions.Client.Class(_contract.BaseClassName))
            .AppendLine("(");

        using (output.Indent())
        {
            if (_isStaticClass)
            {
                output.Append("this ");
            }

            output
                .WriteTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IClientFactoryBuilder")
                .AppendLine(" factoryBuilder,")
                .Append("Action<")
                .WriteType(typeof(ServiceModelGrpcClientOptions))
                .Append(", ")
                .WriteType(typeof(IServiceProvider))
                .AppendLine("> configure = null,")
                .WriteTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IChannelProvider")
                .AppendLine(" channel = null)");
        }

        output.AppendLine("{");
        using (output.Indent())
        {
            output.WriteArgumentNullException("factoryBuilder");

            output
                .Append("return factoryBuilder.AddClientBuilder(new ")
                .Append(NamingConventions.ClientBuilder.Class(_contract.BaseClassName))
                .AppendLine("(), configure, channel);");
        }

        output.AppendLine("}");
    }

    private void GenerateServiceCollection(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public static ")
            .WriteTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
            .Append(" Add")
            .Append(NamingConventions.Client.Class(_contract.BaseClassName))
            .AppendLine("(");

        using (output.Indent())
        {
            if (_isStaticClass)
            {
                output.Append("this ");
            }

            output
                .WriteTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
                .AppendLine(" services,")
                .Append("Action<")
                .WriteType(typeof(ServiceModelGrpcClientOptions))
                .Append(", ")
                .WriteType(typeof(IServiceProvider))
                .AppendLine("> configure = null,")
                .WriteTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IChannelProvider")
                .AppendLine(" channel = null)");
        }

        output.AppendLine("{");
        using (output.Indent())
        {
            output.WriteArgumentNullException("services");

            output
                .Append("return ServiceModel.Grpc.Client.DependencyInjection.ClientServiceCollectionExtensions.AddServiceModelGrpcClientBuilder(services, new ")
                .Append(NamingConventions.ClientBuilder.Class(_contract.BaseClassName))
                .AppendLine("(), configure, channel);");
        }

        output.AppendLine("}");
    }

    private void GenerateHttpClientBuilder(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public static ")
            .WriteTypeName("Microsoft.Extensions.DependencyInjection", "IHttpClientBuilder")
            .Append(" Configure")
            .Append(NamingConventions.Client.Class(_contract.BaseClassName))
            .Append("Creator")
            .AppendLine("(");

        using (output.Indent())
        {
            if (_isStaticClass)
            {
                output.Append("this ");
            }

            output
                .WriteTypeName("Microsoft.Extensions.DependencyInjection", "IHttpClientBuilder")
                .AppendLine(" builder,")
                .Append("Action<")
                .WriteType(typeof(ServiceModelGrpcClientOptions))
                .Append(", ")
                .WriteType(typeof(IServiceProvider))
                .AppendLine("> configure = null)");
        }

        output.AppendLine("{");
        using (output.Indent())
        {
            output.WriteArgumentNullException("builder");

            output
                .Append("return ServiceModel.Grpc.Client.DependencyInjection.HttpClientBuilderExtensions.ConfigureServiceModelGrpcClientBuilder(builder, new ")
                .Append(NamingConventions.ClientBuilder.Class(_contract.BaseClassName))
                .AppendLine("(), configure);");
        }

        output.AppendLine("}");
    }
}