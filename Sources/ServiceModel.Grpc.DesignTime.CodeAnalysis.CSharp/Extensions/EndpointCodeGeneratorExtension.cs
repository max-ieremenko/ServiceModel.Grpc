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

internal sealed class EndpointCodeGeneratorExtension : ICodeGeneratorExtension
{
    private readonly INamedTypeSymbol _serviceType;
    private readonly bool _generateAspNetExtensions;
    private readonly bool _generateSelfHostExtensions;
    private readonly bool _canUseStaticExtensions;

    public EndpointCodeGeneratorExtension(
        INamedTypeSymbol serviceType,
        bool generateAspNetExtensions,
        bool generateSelfHostExtensions,
        bool canUseStaticExtensions)
    {
        _serviceType = serviceType;
        _generateAspNetExtensions = generateAspNetExtensions;
        _generateSelfHostExtensions = generateSelfHostExtensions;
        _canUseStaticExtensions = canUseStaticExtensions;
    }

    public void ProvideGenerators(ICodeGeneratorCollection generators, IContractDescriptionCollection descriptions, IExtensionContext context)
    {
        if (!descriptions.TryGet(_serviceType, out var description))
        {
            return;
        }

        if (_generateAspNetExtensions)
        {
            generators.Add(new EndpointAspNetAddOptionsCodeGenerator(description, _canUseStaticExtensions));
            generators.Add(new EndpointAspNetMapGrpcCodeGenerator(description, _canUseStaticExtensions));
        }

        if (_generateSelfHostExtensions)
        {
            generators.Add(new EndpointSelfHostAddSingletonCodeGenerator(description, _canUseStaticExtensions));
            generators.Add(new EndpointSelfHostAddTransientCodeGenerator(description, _canUseStaticExtensions));
            generators.Add(new EndpointSelfHostAddProviderCodeGenerator(description, _canUseStaticExtensions));

            generators.Add(new EndpointSelfHostBinderBaseBindCodeGenerator(description, _canUseStaticExtensions));
            generators.Add(new EndpointSelfHostBinderBaseBindTransientCodeGenerator(description, _canUseStaticExtensions));
        }

        generators.Add(new EndpointCodeGenerator(description));
        generators.Add(new EndpointBinderCodeGenerator(description));
    }
}