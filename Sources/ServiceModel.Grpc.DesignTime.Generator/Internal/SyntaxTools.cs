// <copyright>
// Copyright 2020 Max Ieremenko
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    internal static class SyntaxTools
    {
        public static bool IsInterface(INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Interface;
        }

        public static ICollection<INamedTypeSymbol> ExpandInterface(INamedTypeSymbol type)
        {
            var result = new HashSet<INamedTypeSymbol>();

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

        public static AttributeData? GetCustomAttribute(ISymbol owner, string attributeTypeFullName)
        {
            foreach (var attribute in owner.GetAttributes())
            {
                var fullName = GetFullName(attribute.AttributeClass);
                if (string.Equals(attributeTypeFullName, fullName, StringComparison.Ordinal))
                {
                    return attribute;
                }
            }

            return null;
        }

        public static IEnumerable<IMethodSymbol> GetInstanceMethods(INamedTypeSymbol type)
        {
            foreach (var m in type.GetMembers())
            {
                if (m is IMethodSymbol method && !method.IsStatic && method.MethodKind == MethodKind.Ordinary)
                {
                    yield return method;
                }
            }
        }

        public static string GetFullName(ITypeSymbol type)
        {
            if (type.Kind == SymbolKind.TypeParameter)
            {
                return type.Name;
            }

            var result = new StringBuilder();
            WriteTypeFullName(type, result);

            return result.ToString();
        }

        public static string GetNamespace(ITypeSymbol type)
        {
            var result = new StringBuilder();

            var test = type;
            if (test.ContainingType != null)
            {
                if (result.Length != 0)
                {
                    result.Insert(0, '.');
                }

                result.Insert(0, test.ContainingType.Name);
                test = test.ContainingType;
            }

            foreach (var ns in ExpandNamespace(test.ContainingNamespace))
            {
                if (result.Length != 0)
                {
                    result.Insert(0, '.');
                }

                result.Insert(0, ns.Name);
            }

            return result.ToString();
        }

        public static ImmutableArray<ITypeSymbol> GenericTypeArguments(this ITypeSymbol type)
        {
            return (type as INamedTypeSymbol)?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        }

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

            foreach (var i in type.Interfaces)
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

        public static bool IsVoid(ITypeSymbol type) => IsMatch(type, typeof(void));

        public static bool IsTask(ITypeSymbol type)
        {
            return IsMatch(type, typeof(Task))
                   || IsMatch(type, typeof(Task<>))
                   || IsValueTask(type);
        }

        public static bool IsValueTask(this ITypeSymbol type)
        {
            return IsMatch(type, typeof(ValueTask))
                   || IsMatch(type, typeof(ValueTask<>));
        }

        public static bool IsAsyncEnumerable(ITypeSymbol type) => IsMatch(type, typeof(IAsyncEnumerable<>));

        public static bool IsStream(ITypeSymbol type) => IsMatch(type, typeof(Stream));

        public static bool IsOut(this IParameterSymbol parameter)
        {
            return parameter.RefKind == RefKind.Out;
        }

        public static bool IsRef(this IParameterSymbol parameter)
        {
            return parameter.RefKind == RefKind.Ref;
        }

        public static bool IsMatch(ITypeSymbol type, Type expected)
        {
            if (!ReflectionTools.GetNamespace(expected).Equals(GetNamespace(type), StringComparison.Ordinal))
            {
                return false;
            }

            if (expected.IsGenericType)
            {
                if (!expected.Name.AsSpan(0, expected.Name.IndexOf('`')).Equals(type.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var args = GenericTypeArguments(type);
                return expected.GetGenericArguments().Length == args.Length;
            }

            return expected.Name.Equals(type.Name, StringComparison.Ordinal)
                   && GenericTypeArguments(type).IsEmpty;
        }

        private static bool IsNullable(ITypeSymbol type) => IsMatch(type, typeof(Nullable<>));

        private static IEnumerable<INamespaceSymbol> ExpandNamespace(INamespaceSymbol? startFrom)
        {
            var ns = startFrom;
            while (ns != null && !ns.IsGlobalNamespace)
            {
                yield return ns;
                ns = ns.ContainingNamespace;
            }
        }

        private static void WriteTypeFullName(ITypeSymbol type, StringBuilder result)
        {
            if (type.TypeKind == TypeKind.Array)
            {
                WriteTypeFullName(((IArrayTypeSymbol)type).ElementType, result);
                result.Append("[]");
                return;
            }

            var genericArguments = GenericTypeArguments(type);
            if (IsNullable(type))
            {
                WriteTypeFullName(genericArguments[0], result);
                result.Append("?");
                return;
            }

            WriteTypeFullName(GetNamespace(type), type.Name, result);

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

        private static void WriteTypeFullName(string? ns, string name, StringBuilder result)
        {
            if ("System".Equals(ns, StringComparison.Ordinal)
                || "System.Collections.Generic".Equals(ns, StringComparison.Ordinal)
                || "System.Threading.Tasks".Equals(ns, StringComparison.Ordinal)
                || "System.Threading".Equals(ns, StringComparison.Ordinal)
                || "ServiceModel.Grpc".Equals(ns, StringComparison.Ordinal)
                || "Grpc.Core".Equals(ns, StringComparison.Ordinal))
            {
                result.Append(SimplifyTypeName(name));
                return;
            }

            if (!string.IsNullOrEmpty(ns))
            {
                result.Append(ns).Append(".");
            }

            result.Append(name);
        }

        private static string SimplifyTypeName(string name)
        {
            if (typeof(string).Name.Equals(name, StringComparison.Ordinal))
            {
                return "string";
            }

            if (typeof(short).Name.Equals(name, StringComparison.Ordinal))
            {
                return "short";
            }

            if (typeof(int).Name.Equals(name, StringComparison.Ordinal))
            {
                return "int";
            }

            if (typeof(long).Name.Equals(name, StringComparison.Ordinal))
            {
                return "long";
            }

            if (typeof(void).Name.Equals(name, StringComparison.Ordinal))
            {
                return "void";
            }

            return name;
        }
    }
}
