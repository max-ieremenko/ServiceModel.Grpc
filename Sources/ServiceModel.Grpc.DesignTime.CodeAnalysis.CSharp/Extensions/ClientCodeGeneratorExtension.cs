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

using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

internal sealed class ClientCodeGeneratorExtension : ICodeGeneratorExtension
{
    private readonly INamedTypeSymbol _serviceType;
    private readonly bool _generateDiExtensions;
    private readonly bool _canUseStaticExtensions;

    public ClientCodeGeneratorExtension(INamedTypeSymbol serviceType, bool generateDiExtensions, bool canUseStaticExtensions)
    {
        _serviceType = serviceType;
        _generateDiExtensions = generateDiExtensions;
        _canUseStaticExtensions = canUseStaticExtensions;
    }

    public void ProvideGenerators(ICodeGeneratorCollection generators, IContractDescriptionCollection descriptions, IExtensionContext context)
    {
        if (!descriptions.TryGet(_serviceType, out var description))
        {
            return;
        }

        if (_generateDiExtensions)
        {
            generators.Add(new ClientDiCodeGenerator(description, _canUseStaticExtensions));
        }

        generators.Add(new ClientFactoryExtensionCodeGenerator(description, _canUseStaticExtensions));
        generators.Add(new ClientCodeGenerator(description));
        generators.Add(new ClientBuilderCodeGenerator(description));
    }
}