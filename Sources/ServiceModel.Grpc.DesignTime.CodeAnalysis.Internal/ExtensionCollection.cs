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
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

internal sealed class ExtensionCollection : List<IExtension>, IExtensionCollection
{
    public bool TryAddContractDescription(INamedTypeSymbol serviceType, AttributeData attribute)
    {
        for (var i = 0; i < Count; i++)
        {
            if (this[i] is DefaultContractDescriptionExtension description
                && SymbolEqualityComparer.Default.Equals(description.ServiceType, serviceType))
            {
                return false;
            }
        }

        Add(new DefaultContractDescriptionExtension(serviceType, attribute));
        return true;
    }

    public TExtension TryAdd<TExtension>()
        where TExtension : IExtension, new()
    {
        for (var i = 0; i < Count; i++)
        {
            if (this[i] is TExtension existing)
            {
                return existing;
            }
        }

        var result = new TExtension();
        Add(result);
        return result;
    }
}