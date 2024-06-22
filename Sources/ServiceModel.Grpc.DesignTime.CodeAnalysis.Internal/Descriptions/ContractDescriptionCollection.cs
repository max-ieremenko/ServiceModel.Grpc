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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal sealed class ContractDescriptionCollection : Collection<ContractDescription<ITypeSymbol>>, IContractDescriptionCollection
{
    private readonly Dictionary<ITypeSymbol, ContractDescription<ITypeSymbol>> _descriptionBySymbol = SyntaxTools.CreateTypeSymbolDictionary<ContractDescription<ITypeSymbol>>();

    public bool TryGet(INamedTypeSymbol typeSymbol, [NotNullWhen(true)] out ContractDescription<ITypeSymbol>? description) =>
        _descriptionBySymbol.TryGetValue(typeSymbol, out description);

    protected override void ClearItems()
    {
        _descriptionBySymbol.Clear();
        base.ClearItems();
    }

    protected override void RemoveItem(int index)
    {
        _descriptionBySymbol.Remove(this[index].ContractInterface);
        base.RemoveItem(index);
    }

    protected override void InsertItem(int index, ContractDescription<ITypeSymbol> item)
    {
        _descriptionBySymbol.Add(item.ContractInterface, item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, ContractDescription<ITypeSymbol> item)
    {
        _descriptionBySymbol.Remove(this[index].ContractInterface);
        base.SetItem(index, item);
        _descriptionBySymbol.Add(item.ContractInterface, item);
    }
}