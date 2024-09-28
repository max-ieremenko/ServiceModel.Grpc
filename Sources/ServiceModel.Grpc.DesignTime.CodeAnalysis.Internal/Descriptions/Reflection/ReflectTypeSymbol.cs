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
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions.Reflection;

internal sealed class ReflectTypeSymbol : IReflect<ITypeSymbol>
{
    public static readonly ReflectTypeSymbol Instance = new();

    public ICollection<ITypeSymbol> GetInterfaces(ITypeSymbol type) => SyntaxTools.ExpandInterface((INamedTypeSymbol)type);

    public bool IsPublicNonGenericInterface(ITypeSymbol type) =>
        type is INamedTypeSymbol named
        && SyntaxTools.IsInterface(type)
        && !named.IsUnboundGenericType;

    public bool TryGetCustomAttribute(ITypeSymbol owner, string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute)
    {
        var source = SyntaxTools.GetCustomAttribute(owner.GetAttributes(), attributeTypeFullName);
        if (source == null)
        {
            attribute = null;
            return false;
        }

        attribute = new CodeAnalysisAttributeInfo(source);
        return true;
    }

    public IMethodInfo<ITypeSymbol>[] GetMethods(ITypeSymbol type)
    {
        var sources = SyntaxTools.GetInstanceMethods(type);
        var result = new List<IMethodInfo<ITypeSymbol>>();
        foreach (var source in sources)
        {
            result.Add(AsMethodInfo(source));
        }

        return result.ToArray();
    }

    public IMethodInfo<ITypeSymbol> AsMethodInfo(IMethodSymbol method) => new CodeAnalysisMethodInfo(method);

    public bool IsAssignableFrom(ITypeSymbol type, ITypeSymbol assignedTo) => type.IsAssignableFrom(assignedTo);

    public bool IsAssignableFrom(ITypeSymbol type, Type assignedTo) => type.IsAssignableFrom(assignedTo) || type.Is(assignedTo);

    public bool Equals(ITypeSymbol x, ITypeSymbol y) => SymbolEqualityComparer.Default.Equals(x, y);

    public bool IsTaskOrValueTask(ITypeSymbol type) => SyntaxTools.IsTask(type) || type.IsValueTask();

    public ITypeSymbol[] GenericTypeArguments(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol typeSymbol || typeSymbol.TypeArguments.Length == 0)
        {
            return [];
        }

        var args = typeSymbol.TypeArguments;
        var result = new ITypeSymbol[args.Length];
        args.CopyTo(result);
        return result;
    }

    public bool IsAsyncEnumerable(ITypeSymbol type) => SyntaxTools.IsAsyncEnumerable(type);

    public bool IsValueTuple(ITypeSymbol type) => SyntaxTools.IsValueTuple(type);

    public string GetFullName(ITypeSymbol type) => SyntaxTools.GetFullName(type);

    public string GetName(ITypeSymbol type) => type.Name;

    public string GetSignature(IMethodInfo<ITypeSymbol> method) => SyntaxTools.GetSignature(((CodeAnalysisMethodInfo)method).Source);

    public bool TryGetArrayInfo(ITypeSymbol type, [NotNullWhen(true)] out ITypeSymbol? elementType, out int rank)
    {
        if (type is IArrayTypeSymbol array)
        {
            rank = array.Rank;
            elementType = array.ElementType;

            return true;
        }

        elementType = null;
        rank = 0;
        return false;
    }
}