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
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal static partial class CodeStringBuilderExtensions
{
    public static ICodeStringBuilder WriteType(this ICodeStringBuilder output, ITypeSymbol type)
    {
        if (type.Kind == SymbolKind.TypeParameter)
        {
            return output.Append(type.Name);
        }

        AppendTypeFullName(output, type);
        return output;
    }

    public static ICodeStringBuilder WriteTypeName(this ICodeStringBuilder output, string? typeNamespace, string name)
    {
        if (typeNamespace != null)
        {
            output
                .Append("global::")
                .Append(typeNamespace)
                .Append(".");
        }

        var index = name.IndexOf('`');
        if (index > 0)
        {
            output.Append(name.Substring(0, index));
            output.Append("<");
        }
        else
        {
            output.Append(name);
        }

        return output;
    }

    public static ICodeStringBuilder WriteType(this ICodeStringBuilder output, Type type) => WriteTypeName(output, type.Namespace, type.Name);

    private static void AppendTypeFullName(ICodeStringBuilder output, ITypeSymbol type)
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

            AppendTypeFullName(output, currentArray.ElementType);
            for (var i = 0; i < stack.Length; i++)
            {
                var rank = stack[i];
                output.Append("[");
                for (var r = 1; r < rank; r++)
                {
                    output.Append(",");
                }

                output.Append("]");
            }

            return;
        }

        var genericArguments = type.GenericTypeArguments();
        if (SyntaxTools.IsNullable(type))
        {
            AppendTypeFullName(output, genericArguments[0]);
            output.Append("?");
            return;
        }

        AppendType(output, type);

        // System.Tuple`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib
        if (!genericArguments.IsEmpty)
        {
            output.Append("<");

            for (var i = 0; i < genericArguments.Length; i++)
            {
                output.WriteCommaIf(i > 0);
                AppendTypeFullName(output, genericArguments[i]);
            }

            output.Append(">");
        }
    }

    private static void AppendType(ICodeStringBuilder output, ITypeSymbol type)
    {
        var ns = SyntaxTools.GetNamespace(type);

        if (SyntaxTools.TrySimplifyTypeName(ns, type.Name, out var simplified))
        {
            output.Append(simplified);
            return;
        }

        if (!string.IsNullOrEmpty(ns))
        {
            output.Append("global::").Append(ns!).Append(".");
        }

        output.Append(type.Name);
    }
}