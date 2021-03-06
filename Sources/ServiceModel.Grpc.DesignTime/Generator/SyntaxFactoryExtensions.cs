﻿// <copyright>
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
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    internal static class SyntaxFactoryExtensions
    {
        public static string GetFullName(this ClassDeclarationSyntax node)
        {
            var result = new StringBuilder(node.Identifier.WithoutTrivia().ToString());
            foreach (var ancestor in node.Ancestors())
            {
                string? name = null;
                switch (ancestor)
                {
                    case NamespaceDeclarationSyntax ns:
                        name = ns.Name.WithoutTrivia().ToString();
                        break;

                    case ClassDeclarationSyntax c:
                        name = c.Identifier.WithoutTrivia().ToString();
                        break;
                }

                if (!string.IsNullOrEmpty(name))
                {
                    result.Insert(0, ".");
                    result.Insert(0, name);
                }
            }

            return result.ToString();
        }

        public static bool IsStatic(this ClassDeclarationSyntax node)
        {
            return node.Modifiers.Any(i => "static".Equals(i.ToString(), StringComparison.Ordinal));
        }
    }
}
