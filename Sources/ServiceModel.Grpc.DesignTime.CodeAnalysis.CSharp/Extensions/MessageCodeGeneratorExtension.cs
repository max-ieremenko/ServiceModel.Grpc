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

using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

internal sealed class MessageCodeGeneratorExtension : ICodeGeneratorExtension
{
    public void ProvideGenerators(ICodeGeneratorCollection generators, IContractDescriptionCollection descriptions, IExtensionContext context)
    {
        var result = new SortedSet<int>();

        for (var i = 0; i < descriptions.Count; i++)
        {
            AddNonBuildInMessages(descriptions[i], result);
        }

        foreach (var count in result)
        {
            generators.Add(new MessageCodeGenerator(count));
        }
    }

    private static void AddNonBuildInMessages(IContractDescription description, SortedSet<int> result)
    {
        for (var i = 0; i < description.Services.Length; i++)
        {
            var service = description.Services[i];
            for (var j = 0; j < service.Operations.Length; j++)
            {
                var operation = service.Operations[j];
                AddPropertiesCount(operation.RequestType, result);
                AddPropertiesCount(operation.HeaderRequestType, result);
                AddPropertiesCount(operation.ResponseType, result);
                AddPropertiesCount(operation.HeaderResponseType, result);
            }
        }
    }

    private static void AddPropertiesCount(IMessageDescription? message, SortedSet<int> result)
    {
        if (message != null && !message.IsBuiltIn)
        {
            result.Add(message.Properties.Length);
        }
    }
}