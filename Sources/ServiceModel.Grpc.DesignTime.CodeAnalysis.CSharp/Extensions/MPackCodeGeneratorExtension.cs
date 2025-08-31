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

internal sealed class MPackCodeGeneratorExtension : ICodeGeneratorExtension
{
    public bool MessagePack { get; set; }

    public bool MemoryPack { get; set; }

    public bool NerdbankMessagePack { get; set; }

    public void ProvideGenerators(ICodeGeneratorCollection generators, IContractDescriptionCollection descriptions, IExtensionContext context)
    {
        var distinct = new HashSet<IMessageDescription>(MessageDescriptionComparer.Default);
        var newFormatters = new SortedSet<int>();

        for (var i = 0; i < descriptions.Count; i++)
        {
            var contract = descriptions[i];
            distinct.Clear();

            var knownFormatters = new List<IMessageDescription>();

            foreach (var service in contract.Services)
            {
                foreach (var operation in service.Operations)
                {
                    AddKnownFormatter(distinct, knownFormatters, operation.HeaderRequestType);
                    AddKnownFormatter(distinct, knownFormatters, operation.RequestType);
                    AddKnownFormatter(distinct, knownFormatters, operation.HeaderResponseType);
                    AddKnownFormatter(distinct, knownFormatters, operation.ResponseType);

                    AddNewFormatter(newFormatters, operation.HeaderRequestType);
                    AddNewFormatter(newFormatters, operation.RequestType);
                    AddNewFormatter(newFormatters, operation.HeaderResponseType);
                    AddNewFormatter(newFormatters, operation.ResponseType);
                }
            }

            ExtendContractGenerators(generators, contract, knownFormatters);
        }

        foreach (var propertiesCount in newFormatters)
        {
            AddFormatterGenerator(generators, propertiesCount);
        }
    }

    private static void AddKnownFormatter(HashSet<IMessageDescription> distinct, List<IMessageDescription> data, IMessageDescription? message)
    {
        if (message?.Properties.Length > 0 && distinct.Add(message))
        {
            data.Add(message);
        }
    }

    private static void AddNewFormatter(SortedSet<int> formatters, IMessageDescription? message)
    {
        if (message != null && !message.IsBuiltIn)
        {
            formatters.Add(message.Properties.Length);
        }
    }

    private void ExtendContractGenerators(ICodeGeneratorCollection generators, IContractDescription contract, List<IMessageDescription> knownFormatters)
    {
        if (knownFormatters.Count == 0)
        {
            return;
        }

        if (MessagePack)
        {
            generators.TryGetMetadata<ContractCodeGeneratorMetadata>()?.RequestPartialCctor(MessagePackContractCodeGenerator.PartialCctorMethodName, contract);
            generators.Add(new MessagePackContractCodeGenerator(contract, knownFormatters));
        }

        if (MemoryPack)
        {
            generators.TryGetMetadata<ContractCodeGeneratorMetadata>()?.RequestPartialCctor(MemoryPackContractCodeGenerator.PartialCctorMethodName, contract);
            generators.Add(new MemoryPackContractCodeGenerator(contract, knownFormatters));
        }

        if (NerdbankMessagePack)
        {
            generators.TryGetMetadata<ContractCodeGeneratorMetadata>()?.RequestPartialCctor(NerdbankMessagePackContractCodeGenerator.PartialCctorMethodName, contract);
            generators.Add(new NerdbankMessagePackContractCodeGenerator(contract, knownFormatters));
        }
    }

    private void AddFormatterGenerator(ICodeGeneratorCollection generators, int propertiesCount)
    {
        if (MessagePack)
        {
            generators.Add(new MessagePackMessageFormatterCodeGenerator(propertiesCount));
        }

        if (MemoryPack)
        {
            generators.Add(new MemoryPackMessageFormatterCodeGenerator(propertiesCount));
        }
    }
}