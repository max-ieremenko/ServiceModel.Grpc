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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal readonly ref struct InterfaceTree
{
    public InterfaceTree(INamedTypeSymbol rootType)
    {
        Services = new List<(string ServiceName, INamedTypeSymbol ServiceType)>();
        Interfaces = new List<INamedTypeSymbol>();

        var interfaces = SyntaxTools.ExpandInterface(rootType).ToList();
        ExtractServiceContracts(interfaces);
        ExtractAttachedContracts(interfaces);
        Interfaces.AddRange(interfaces);
    }

    public List<(string ServiceName, INamedTypeSymbol ServiceType)> Services { get; }

    public List<INamedTypeSymbol> Interfaces { get; }

    private static bool ContainsOperation(INamedTypeSymbol type)
    {
        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            if (ServiceContract.IsServiceOperation(method))
            {
                return true;
            }
        }

        return false;
    }

    private void ExtractServiceContracts(List<INamedTypeSymbol> interfaces)
    {
        for (var i = 0; i < interfaces.Count; i++)
        {
            var interfaceType = interfaces[i];
            if (!ServiceContract.IsServiceContractInterface(interfaceType))
            {
                continue;
            }

            var serviceName = ServiceContract.GetServiceName(interfaceType);
            Services.Add((serviceName, interfaceType));
            interfaces.RemoveAt(i);
            i--;
        }
    }

    private void ExtractAttachedContracts(List<INamedTypeSymbol> interfaces)
    {
        // take into account only ServiceContracts
        var servicesIndex = Services.Count;

        for (var i = 0; i < interfaces.Count; i++)
        {
            var interfaceType = interfaces[i];
            if (!ContainsOperation(interfaceType) || !TryFindParentService(interfaceType, servicesIndex, out var serviceName))
            {
                continue;
            }

            Services.Add((serviceName, interfaceType));
            interfaces.RemoveAt(i);
            i--;
        }
    }

    private bool TryFindParentService(INamedTypeSymbol interfaceType, int servicesIndex, [NotNullWhen(true)] out string? serviceName)
    {
        serviceName = null;
        INamedTypeSymbol? parent = null;

        for (var i = 0; i < servicesIndex; i++)
        {
            var test = Services[i];
            if (!interfaceType.IsAssignableFrom(test.ServiceType))
            {
                continue;
            }

            if (parent == null || parent.IsAssignableFrom(test.ServiceType))
            {
                parent = test.ServiceType;
                serviceName = test.ServiceName;
            }
        }

        return parent != null;
    }
}