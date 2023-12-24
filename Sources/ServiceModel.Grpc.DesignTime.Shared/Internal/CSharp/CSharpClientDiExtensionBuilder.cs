// <copyright>
// Copyright 2023 Max Ieremenko
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
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

internal sealed class CSharpClientDiExtensionBuilder : CodeGeneratorBase
{
    private readonly ContractDescription _contract;
    private readonly bool _isStaticClass;

    public CSharpClientDiExtensionBuilder(ContractDescription contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public override string GetGeneratedMemberName() => "AddServiceCollection" + _contract.ClientClassName;

    protected override void Generate()
    {
        GenerateClientFactoryBuilder();

        Output.AppendLine();
        GenerateServiceCollection();

        Output.AppendLine();
        GenerateHttpClientBuilder();
    }

    private void GenerateClientFactoryBuilder()
    {
        WriteMetadata();
        Output
            .Append("public static ")
            .AppendTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IClientFactoryBuilder")
            .Append(" Add")
            .Append(_contract.ClientClassName)
            .AppendLine("(");

        using (Output.Indent())
        {
            if (_isStaticClass)
            {
                Output.Append("this ");
            }

            Output
                .AppendTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IClientFactoryBuilder")
                .AppendLine(" factoryBuilder,")
                .Append("Action<")
                .AppendType(typeof(ServiceModelGrpcClientOptions))
                .Append(", ")
                .AppendType(typeof(IServiceProvider))
                .AppendLine("> configure = null,")
                .AppendTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IChannelProvider")
                .AppendLine(" channel = null)");
        }

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output.AppendArgumentNullException("factoryBuilder");

            Output
                .Append("return factoryBuilder.AddClientBuilder(new ")
                .Append(_contract.ClientBuilderClassName)
                .AppendLine("(), configure, channel);");
        }

        Output.AppendLine("}");
    }

    private void GenerateServiceCollection()
    {
        WriteMetadata();
        Output
            .Append("public static ")
            .AppendTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
            .Append(" Add")
            .Append(_contract.ClientClassName)
            .AppendLine("(");

        using (Output.Indent())
        {
            if (_isStaticClass)
            {
                Output.Append("this ");
            }

            Output
                .AppendTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
                .AppendLine(" services,")
                .Append("Action<")
                .AppendType(typeof(ServiceModelGrpcClientOptions))
                .Append(", ")
                .AppendType(typeof(IServiceProvider))
                .AppendLine("> configure = null,")
                .AppendTypeName("ServiceModel.Grpc.Client.DependencyInjection", "IChannelProvider")
                .AppendLine(" channel = null)");
        }

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output.AppendArgumentNullException("services");

            Output
                .Append("return ServiceModel.Grpc.Client.DependencyInjection.ClientServiceCollectionExtensions.AddServiceModelGrpcClientBuilder(services, new ")
                .Append(_contract.ClientBuilderClassName)
                .AppendLine("(), configure, channel);");
        }

        Output.AppendLine("}");
    }

    private void GenerateHttpClientBuilder()
    {
        WriteMetadata();
        Output
            .Append("public static ")
            .AppendTypeName("Microsoft.Extensions.DependencyInjection", "IHttpClientBuilder")
            .Append(" Configure")
            .Append(_contract.ClientClassName)
            .Append("Creator")
            .AppendLine("(");

        using (Output.Indent())
        {
            if (_isStaticClass)
            {
                Output.Append("this ");
            }

            Output
                .AppendTypeName("Microsoft.Extensions.DependencyInjection", "IHttpClientBuilder")
                .AppendLine(" builder,")
                .Append("Action<")
                .AppendType(typeof(ServiceModelGrpcClientOptions))
                .Append(", ")
                .AppendType(typeof(IServiceProvider))
                .AppendLine("> configure = null)");
        }

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output.AppendArgumentNullException("builder");

            Output
                .Append("return ServiceModel.Grpc.Client.DependencyInjection.HttpClientBuilderExtensions.ConfigureServiceModelGrpcClientBuilder(builder, new ")
                .Append(_contract.ClientBuilderClassName)
                .AppendLine("(), configure);");
        }

        Output.AppendLine("}");
    }
}