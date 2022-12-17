// <copyright>
// Copyright 2022 Max Ieremenko
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
using System.ServiceModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

internal static class ContractDescriptionFactory
{
    public static ContractDescription Create(Type serviceType)
    {
        var compilation = CSharpCompilation
            .Create(
                nameof(CSharpServiceSelfHostAddProviderServiceBuilderTest),
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ContractDescriptionFactory).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ServiceContractAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(serviceType.Assembly.Location)
                });

        var symbol = compilation.GetTypeByMetadataName(serviceType);
        return new ContractDescription(symbol);
    }
}