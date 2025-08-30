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

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

public sealed class TypeHandler
{
    private readonly List<Func<AttributeData, Type?>> _knownTypes = new(5);
    private readonly List<Func<AttributeData, ITypeSymbol?>> _knownSymbols = new(1);

    public void AddKnownAttribute(Func<AttributeData, Type?> analyzer) => _knownTypes.Add(analyzer);

    public void AddKnownAttribute(Func<AttributeData, ITypeSymbol?> analyzer) => _knownSymbols.Add(analyzer);

    public bool TryGetProviderType(AttributeData attribute, out ITypeSymbol? typeSymbol, out Type? type)
    {
        typeSymbol = null;
        type = null;

        for (var i = 0; i < _knownTypes.Count; i++)
        {
            type = _knownTypes[i](attribute);
            if (type != null)
            {
                return true;
            }
        }

        for (var i = 0; i < _knownSymbols.Count; i++)
        {
            typeSymbol = _knownSymbols[i](attribute);
            if (typeSymbol != null)
            {
                return true;
            }
        }

        return false;
    }

    public Assembly GetAssembly(string assemblyName, string location) => throw new NotImplementedException();
}