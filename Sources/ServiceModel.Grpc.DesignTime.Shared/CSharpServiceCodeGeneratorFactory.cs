// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using ServiceModel.Grpc.DesignTime.Generator.Internal;
using ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

namespace ServiceModel.Grpc.DesignTime.Generator;

internal sealed class CSharpServiceCodeGeneratorFactory : ICodeGeneratorFactory
{
    private readonly ContractDescription _contract;
    private readonly bool _generateAspNetExtensions;
    private readonly bool _generateSelfHostExtensions;
    private readonly bool _canUseStaticExtensions;

    public CSharpServiceCodeGeneratorFactory(
        ContractDescription contract,
        bool generateAspNetExtensions,
        bool generateSelfHostExtensions,
        bool canUseStaticExtensions)
    {
        _contract = contract;
        _generateAspNetExtensions = generateAspNetExtensions;
        _generateSelfHostExtensions = generateSelfHostExtensions;
        _canUseStaticExtensions = canUseStaticExtensions;
    }

    public IEnumerable<CodeGeneratorBase> GetGenerators()
    {
        if (_generateAspNetExtensions)
        {
            yield return new CSharpServiceAspNetAddOptionsBuilder(_contract, _canUseStaticExtensions);
            yield return new CSharpServiceAspNetMapGrpcServiceBuilder(_contract, _canUseStaticExtensions);
        }

        if (_generateSelfHostExtensions)
        {
            yield return new CSharpServiceSelfHostAddSingletonServiceBuilder(_contract, _canUseStaticExtensions);
            yield return new CSharpServiceSelfHostAddTransientServiceBuilder(_contract, _canUseStaticExtensions);
            yield return new CSharpServiceSelfHostAddProviderServiceBuilder(_contract, _canUseStaticExtensions);

            yield return new CSharpServiceBinderBaseBindProviderBuilder(_contract, _canUseStaticExtensions);
            yield return new CSharpServiceBinderBaseBindTransientBuilder(_contract, _canUseStaticExtensions);
        }

        yield return new CSharpServiceEndpointBuilder(_contract);
        yield return new CSharpServiceEndpointBinderBuilder(_contract);
    }

    public string GetHintName() => _contract.EndpointClassName;
}