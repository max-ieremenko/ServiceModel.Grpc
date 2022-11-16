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

using System.Collections.Generic;
using ServiceModel.Grpc.DesignTime.Generator.Internal;
using ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

namespace ServiceModel.Grpc.DesignTime.Generator;

internal sealed class CSharpSharedCodeGeneratorFactory : ICodeGeneratorFactory
{
    private readonly ContractDescription _contract;

    public CSharpSharedCodeGeneratorFactory(ContractDescription contract)
    {
        _contract = contract;
    }

    public IEnumerable<CodeGeneratorBase> GetGenerators()
    {
        yield return new CSharpContractBuilder(_contract);

        foreach (var propertiesCount in GetNonBuildInMessages(_contract))
        {
            yield return new CSharpMessageBuilder(propertiesCount);
        }
    }

    public string GetHintName() => _contract.ContractClassName;

    private static IEnumerable<int> GetNonBuildInMessages(ContractDescription contract)
    {
        var result = new SortedSet<int>();

        for (var i = 0; i < contract.Services.Count; i++)
        {
            var service = contract.Services[i];
            for (var j = 0; j < service.Operations.Count; j++)
            {
                var operation = service.Operations[j];
                AddPropertiesCount(result, operation.RequestType);
                AddPropertiesCount(result, operation.HeaderRequestType);
                AddPropertiesCount(result, operation.ResponseType);
                AddPropertiesCount(result, operation.HeaderResponseType);
            }
        }

        return result;
    }

    private static void AddPropertiesCount(SortedSet<int> target, MessageDescription? message)
    {
        if (message != null && !message.IsBuiltIn)
        {
            target.Add(message.Properties.Length);
        }
    }
}