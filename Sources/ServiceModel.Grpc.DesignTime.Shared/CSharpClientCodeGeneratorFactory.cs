// <copyright>
// Copyright 2020-2023 Max Ieremenko
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

internal sealed class CSharpClientCodeGeneratorFactory : ICodeGeneratorFactory
{
    private readonly ContractDescription _contract;
    private readonly bool _generateDiExtensions;
    private readonly bool _canUseStaticExtensions;

    public CSharpClientCodeGeneratorFactory(
        ContractDescription contract,
        bool generateDiExtensions,
        bool canUseStaticExtensions)
    {
        _contract = contract;
        _generateDiExtensions = generateDiExtensions;
        _canUseStaticExtensions = canUseStaticExtensions;
    }

    public IEnumerable<CodeGeneratorBase> GetGenerators()
    {
        if (_generateDiExtensions)
        {
            yield return new CSharpClientDiExtensionBuilder(_contract, _canUseStaticExtensions);
        }

        yield return new CSharpClientFactoryExtensionBuilder(_contract, _canUseStaticExtensions);
        yield return new CSharpClientBuilderBuilder(_contract);
        yield return new CSharpClientBuilder(_contract);
    }

    public string GetHintName() => _contract.ClientClassName;
}