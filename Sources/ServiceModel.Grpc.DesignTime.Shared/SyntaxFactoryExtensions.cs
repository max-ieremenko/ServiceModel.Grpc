// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ServiceModel.Grpc.DesignTime.Generator;

internal static partial class SyntaxFactoryExtensions
{
    public static string GetFullName(this ClassDeclarationSyntax node)
    {
        var result = new StringBuilder(node.Identifier.WithoutTrivia().ToString());
        foreach (var ancestor in node.AncestorMembers())
        {
            result.Insert(0, ".");
            result.Insert(0, ancestor.Name);
        }

        return result.ToString();
    }

    public static bool IsStatic(this ClassDeclarationSyntax node)
    {
        return node.Modifiers.Any(i => "static".Equals(i.ToString(), StringComparison.Ordinal));
    }

    public static IEnumerable<(SyntaxKind Kind, string Name)> AncestorMembers(this ClassDeclarationSyntax node)
    {
        foreach (var ancestor in node.Ancestors().OfType<MemberDeclarationSyntax>())
        {
            string? name = null;
            var kind = default(SyntaxKind);

            if (ancestor is NamespaceDeclarationSyntax ns)
            {
                kind = SyntaxKind.NamespaceDeclaration;
                name = ns.Name.WithoutTrivia().ToString();
            }
            else if (ancestor is ClassDeclarationSyntax c)
            {
                kind = SyntaxKind.ClassDeclaration;
                name = c.Identifier.WithoutTrivia().ToString();
            }
            else
            {
                TryGetAncestorMember(ancestor, ref kind, ref name);
            }

            if (name != null)
            {
                yield return (kind, name);
            }
        }
    }
}