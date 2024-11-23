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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

public static class SyntaxTools
{
#pragma warning disable RS1024 // Compare symbols correctly
    public static Dictionary<ITypeSymbol, T> CreateTypeSymbolDictionary<T>() => new(SymbolEqualityComparer.Default);

    public static HashSet<INamedTypeSymbol> CreateNamedTypeHashSet() => new(SymbolEqualityComparer.Default);

    public static HashSet<ITypeSymbol> CreateTypeHashSet() => new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

    public static bool IsInterface(ITypeSymbol type) => type.TypeKind == TypeKind.Interface;

    public static ICollection<ITypeSymbol> ExpandInterface(ITypeSymbol type)
    {
        var result = CreateTypeHashSet();

        if (IsInterface(type))
        {
            result.Add(type);
        }

        foreach (var i in type.AllInterfaces)
        {
            result.Add(i);
        }

        return result;
    }

    public static AttributeData? GetCustomAttribute(in ImmutableArray<AttributeData> attributes, string attributeTypeFullName)
    {
        for (var i = 0; i < attributes.Length; i++)
        {
            var attribute = attributes[i];
            var fullName = attribute.AttributeClass?.ToDisplayString(NullableFlowState.None);
            if (string.Equals(attributeTypeFullName, fullName, StringComparison.Ordinal))
            {
                return attribute;
            }
        }

        return null;
    }

    public static IEnumerable<IMethodSymbol> GetInstanceMethods(ITypeSymbol type)
    {
        foreach (var m in type.GetMembers())
        {
            if (m is IMethodSymbol method && !method.IsStatic && method.MethodKind == MethodKind.Ordinary)
            {
                yield return method;
            }
        }
    }

    public static string GetFullName(ITypeSymbol? type)
    {
        if (type == null)
        {
            return string.Empty;
        }

        if (type.Kind == SymbolKind.TypeParameter)
        {
            return type.Name;
        }

        var result = new StringBuilder();
        WriteTypeFullName(type, result);

        return result.ToString();
    }

    public static void WriteFullName(ITypeSymbol type, StringBuilder output)
    {
        if (type.Kind == SymbolKind.TypeParameter)
        {
            output.Append(type.Name);
            return;
        }

        WriteTypeFullName(type, output);
    }

    public static string? GetNamespace(ITypeSymbol type)
    {
        if (type.ContainingType != null)
        {
            return type.ContainingType.ToDisplayString();
        }

        var ns = type.ContainingNamespace;
        if (ns == null || ns.IsGlobalNamespace)
        {
            return null;
        }

        return ns.ToDisplayString();
    }

    public static ImmutableArray<ITypeSymbol> GenericTypeArguments(this ITypeSymbol type)
        => (type as INamedTypeSymbol)?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;

    public static string GetSignature(IMethodSymbol method)
    {
        var result = new StringBuilder()
            .Append(GetFullName(method.ReturnType))
            .Append(" ")
            .Append(method.Name);

        if (method.TypeArguments.Length != 0)
        {
            result.Append("<");
            for (var i = 0; i < method.TypeArguments.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(", ");
                }

                result.Append(method.TypeArguments[i]);
            }

            result.Append(">");
        }

        result.Append("(");
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            if (i > 0)
            {
                result.Append(", ");
            }

            var p = method.Parameters[i];

            if (p.IsOut())
            {
                result.Append("out ");
            }
            else if (p.IsRef())
            {
                result.Append("ref ");
            }

            result.Append(GetFullName(p.Type));
        }

        result.Append(")");
        return result.ToString();
    }

    public static bool Is(this ITypeSymbol type, Type expected) => IsMatch(type, expected);

    public static bool IsAssignableFrom(this ITypeSymbol type, Type expected)
    {
        if (expected == typeof(object))
        {
            return true;
        }

        foreach (var i in type.AllInterfaces)
        {
            if (IsMatch(i, expected))
            {
                return true;
            }
        }

        var test = type;
        while (test != null)
        {
            if (IsMatch(test, expected))
            {
                return true;
            }

            test = test.BaseType;
        }

        return false;
    }

    public static bool IsAssignableFrom(this ITypeSymbol type, ITypeSymbol expected)
    {
        if (SymbolEqualityComparer.Default.Equals(type, expected))
        {
            return true;
        }

        foreach (var i in expected.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(i, type))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsVoid(ITypeSymbol type)
    {
        return type.Name.Equals("Void", StringComparison.Ordinal)
               && type.GenericTypeArguments().Length == 0
               && "System".Equals(GetNamespace(type), StringComparison.Ordinal);
    }

    public static bool IsTask(ITypeSymbol type)
    {
        if (type.GenericTypeArguments().Length > 1)
        {
            return false;
        }

        return ("Task".Equals(type.Name, StringComparison.Ordinal)
                || "ValueTask".Equals(type.Name, StringComparison.Ordinal))
               && "System.Threading.Tasks".Equals(GetNamespace(type), StringComparison.Ordinal);
    }

    public static bool IsValueTask(this ITypeSymbol type)
    {
        return type.GenericTypeArguments().Length < 2
               && "ValueTask".Equals(type.Name, StringComparison.Ordinal)
               && "System.Threading.Tasks".Equals(GetNamespace(type), StringComparison.Ordinal);
    }

    public static bool IsAsyncEnumerable(ITypeSymbol type)
    {
        return type.GenericTypeArguments().Length == 1
               && "IAsyncEnumerable".Equals(type.Name, StringComparison.Ordinal)
               && "System.Collections.Generic".Equals(GetNamespace(type), StringComparison.Ordinal);
    }

    public static bool IsStream(ITypeSymbol type) => IsMatch(type, typeof(Stream));

    public static bool IsValueTuple(ITypeSymbol type)
    {
        return type.IsTupleType
               && type.Name.Equals(nameof(ValueTuple), StringComparison.Ordinal)
               && type.GenericTypeArguments().Length > 0
               && "System".Equals(GetNamespace(type), StringComparison.Ordinal);
    }

    public static IMethodSymbol GetInterfaceImplementation(this ITypeSymbol type, IMethodSymbol interfaceMethod)
    {
        if (SymbolEqualityComparer.Default.Equals(type, interfaceMethod.ContainingType))
        {
            return interfaceMethod;
        }

        var result = type.FindImplementationForInterfaceMember(interfaceMethod);
        if (result == null)
        {
            throw new ArgumentNullException(nameof(interfaceMethod));
        }

        return (IMethodSymbol)result;
    }

    public static bool IsOut(this IParameterSymbol parameter) => parameter.RefKind == RefKind.Out;

    public static bool IsRef(this IParameterSymbol parameter) => parameter.RefKind == RefKind.Ref;

    public static bool IsMatch(ITypeSymbol type, Type expected)
    {
        var expectedNamespace = expected.Namespace;
        if (expected.IsNested && expected.DeclaringType != null)
        {
            expectedNamespace += "." + expected.DeclaringType.Name;
        }

        if (!string.Equals(expectedNamespace, GetNamespace(type), StringComparison.Ordinal))
        {
            return false;
        }

        if (expected.IsGenericType)
        {
            if (!expected.Name.AsSpan(0, expected.Name.IndexOf('`')).Equals(type.Name.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var typeArgs = GenericTypeArguments(type);
            var expectedArgs = expected.GetGenericArguments();
            if (typeArgs.Length != expectedArgs.Length)
            {
                return false;
            }

            if (expected.IsGenericTypeDefinition)
            {
                return true;
            }

            for (var i = 0; i < typeArgs.Length; i++)
            {
                if (!IsMatch(typeArgs[i], expectedArgs[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return expected.Name.Equals(type.Name, StringComparison.Ordinal)
               && GenericTypeArguments(type).IsEmpty;
    }

    public static bool IsNullable(ITypeSymbol type) =>
        type.Name.Equals(nameof(Nullable), StringComparison.Ordinal)
        && type.GenericTypeArguments().Length == 1
        && "System".Equals(GetNamespace(type), StringComparison.Ordinal);

    public static bool TryGetPrimitiveValue<T>(this in TypedConstant constant, [NotNullWhen(true)] out T? value)
    {
        if (constant.Kind != TypedConstantKind.Error && constant.Kind != TypedConstantKind.Array && constant.Value is T result)
        {
            value = result;
            return true;
        }

        value = default;
        return false;
    }

    public static bool TryGetArrayValue<T>(this in TypedConstant constant, [NotNullWhen(true)] out T[]? value)
    {
        if (constant.Kind != TypedConstantKind.Array)
        {
            value = null;
            return false;
        }

        var values = constant.Values;
        var result = new T[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            if (!values[i].TryGetPrimitiveValue<T>(out var item))
            {
                value = null;
                return false;
            }

            result[i] = item;
        }

        value = result;
        return true;
    }

    public static bool TryGetNamedArgumentValue(this AttributeData attributeData, string argumentName, out TypedConstant constant)
    {
        for (var i = 0; i < attributeData.NamedArguments.Length; i++)
        {
            var arg = attributeData.NamedArguments[i];
            if (argumentName.Equals(arg.Key, StringComparison.Ordinal))
            {
                constant = arg.Value;
                return true;
            }
        }

        constant = default;
        return false;
    }

    public static bool TrySimplifyTypeName(string? @namespace, string name, [NotNullWhen(true)] out string? simplifiedName)
    {
        if ("System".Equals(@namespace, StringComparison.Ordinal))
        {
            simplifiedName = SimplifyTypeName(name);
            return true;
        }

        if ("System.Collections.Generic".Equals(@namespace, StringComparison.Ordinal)
            || "System.Threading.Tasks".Equals(@namespace, StringComparison.Ordinal)
            || "System.Threading".Equals(@namespace, StringComparison.Ordinal))
        {
            simplifiedName = name;
            return true;
        }

        simplifiedName = null;
        return false;
    }

    private static void WriteTypeFullName(ITypeSymbol type, StringBuilder result)
    {
        if (type is IArrayTypeSymbol array)
        {
            var stack = ImmutableArray.Create(array.Rank);

            var currentArray = array;
            while (currentArray.ElementType is IArrayTypeSymbol subArray)
            {
                stack = stack.Add(subArray.Rank);
                currentArray = subArray;
            }

            WriteTypeFullName(currentArray.ElementType, result);
            for (var i = 0; i < stack.Length; i++)
            {
                var rank = stack[i];
                result.Append('[');
                for (var r = 1; r < rank; r++)
                {
                    result.Append(',');
                }

                result.Append(']');
            }

            return;
        }

        var genericArguments = GenericTypeArguments(type);
        if (IsNullable(type))
        {
            WriteTypeFullName(genericArguments[0], result);
            result.Append("?");
            return;
        }

        WriteType(type, result);

        // System.Tuple`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib
        if (!genericArguments.IsEmpty)
        {
            result.Append("<");

            for (var i = 0; i < genericArguments.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(", ");
                }

                WriteTypeFullName(genericArguments[i], result);
            }

            result.Append(">");
        }
    }

    private static void WriteType(ITypeSymbol type, StringBuilder result)
    {
        var ns = GetNamespace(type);

        if (TrySimplifyTypeName(ns, type.Name, out var simplified))
        {
            result.Append(simplified);
        }
        else
        {
            if (!string.IsNullOrEmpty(ns))
            {
                result.Append(ns).Append(".");
            }

            result.Append(type.Name);
        }
    }

    private static string SimplifyTypeName(string name)
    {
        if (nameof(String).Equals(name, StringComparison.Ordinal))
        {
            return "string";
        }

        if (nameof(Int16).Equals(name, StringComparison.Ordinal))
        {
            return "short";
        }

        if (nameof(UInt16).Equals(name, StringComparison.Ordinal))
        {
            return "ushort";
        }

        if (nameof(Int32).Equals(name, StringComparison.Ordinal))
        {
            return "int";
        }

        if (nameof(UInt32).Equals(name, StringComparison.Ordinal))
        {
            return "uint";
        }

        if (nameof(Int64).Equals(name, StringComparison.Ordinal))
        {
            return "long";
        }

        if (nameof(UInt64).Equals(name, StringComparison.Ordinal))
        {
            return "ulong";
        }

        if (nameof(Double).Equals(name, StringComparison.Ordinal))
        {
            return "double";
        }

        if (nameof(Decimal).Equals(name, StringComparison.Ordinal))
        {
            return "decimal";
        }

        if (nameof(Single).Equals(name, StringComparison.Ordinal))
        {
            return "float";
        }

        if (nameof(Byte).Equals(name, StringComparison.Ordinal))
        {
            return "byte";
        }

        if (nameof(SByte).Equals(name, StringComparison.Ordinal))
        {
            return "sbyte";
        }

        if (nameof(Char).Equals(name, StringComparison.Ordinal))
        {
            return "char";
        }

        if (nameof(Boolean).Equals(name, StringComparison.Ordinal))
        {
            return "bool";
        }

        if (typeof(void).Name.Equals(name, StringComparison.Ordinal))
        {
            return "void";
        }

        if (nameof(Object).Equals(name, StringComparison.Ordinal))
        {
            return "object";
        }

        return name;
    }
}