// <copyright>
// Copyright 2024 Max Ieremenko
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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal sealed class DefaultContractDescriptionExtension : IContractDescriptionExtension
{
    private readonly AttributeData _attribute;

    public DefaultContractDescriptionExtension(INamedTypeSymbol serviceType, AttributeData attribute)
    {
        _attribute = attribute;
        ServiceType = serviceType;
    }

    public INamedTypeSymbol ServiceType { get; }

    public void ProvideContractDescriptions(IContractDescriptionCollection descriptions, IExtensionContext context)
    {
        if (descriptions.TryGet(ServiceType, out _))
        {
            return;
        }

        var description = ContractDescriptionBuilder.Build(ServiceType);
        ShowCommonWarnings(description, context);
        descriptions.Add(description);
    }

    private void ShowCommonWarnings(IContractDescription contract, IExtensionContext context)
    {
        foreach (var interfaceDescription in contract.Interfaces)
        {
            context.ReportInheritsNotServiceContract(_attribute, ServiceType, interfaceDescription.InterfaceType);
        }

        foreach (var interfaceDescription in contract.Services)
        {
            foreach (var method in interfaceDescription.Methods)
            {
                context.ReportIsNotOperationContract(_attribute, ServiceType, method.Error);
            }

            foreach (var method in interfaceDescription.NotSupportedOperations)
            {
                context.ReportIsNotSupportedOperation(_attribute, ServiceType, method.Error);
            }
        }
    }
}