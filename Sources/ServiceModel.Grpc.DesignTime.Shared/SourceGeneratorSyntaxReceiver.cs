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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ServiceModel.Grpc.DesignTime.Generator;

internal sealed class SourceGeneratorSyntaxReceiver : ISyntaxReceiver
{
    public IList<ClassDeclarationSyntax> Candidates { get; } = new List<ClassDeclarationSyntax>();

    public static bool IsCandidate(SyntaxNode syntaxNode)
    {
        return syntaxNode is ClassDeclarationSyntax owner
               && ContainsGrpcServiceAttribute(owner.AttributeLists);
    }

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (IsCandidate(syntaxNode))
        {
            Candidates.Add((ClassDeclarationSyntax)syntaxNode);
        }
    }

    private static bool ContainsGrpcServiceAttribute(in SyntaxList<AttributeListSyntax> attributeLists)
    {
        for (var i = 0; i < attributeLists.Count; i++)
        {
            var attributes = attributeLists[i].Attributes;
            for (var j = 0; j < attributes.Count; j++)
            {
                if (DoesLookLikeGrpcServiceAttribute(attributes[j]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool DoesLookLikeGrpcServiceAttribute(AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString();
        return name.IndexOf("ExportGrpcService", StringComparison.Ordinal) >= 0
               || name.IndexOf("ImportGrpcService", StringComparison.Ordinal) >= 0;
    }
}